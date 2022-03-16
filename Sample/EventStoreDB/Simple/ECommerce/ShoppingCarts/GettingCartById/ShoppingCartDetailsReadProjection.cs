using Core.Events;
using Core.Projections;
using ECommerce.Storage;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.ShoppingCarts.GettingCartById;

// https://zimarev.com/blog/event-sourcing/projections/
public class ShoppingCartDetailsReadProjection: IReadProjection
{
    private readonly ECommerceDbContext eCommerceDbContext;

    public ShoppingCartDetailsReadProjection(ECommerceDbContext eCommerceDbContext)
    {
        this.eCommerceDbContext = eCommerceDbContext;
    }

    public async Task ProjectAsync<T>(EventEnvelope<T> streamEvent, CancellationToken cancellationToken = default)
        where T : INotification
    {
        switch (streamEvent.Data)
        {
            case ShoppingCartInitialized shoppingCartInitialized:
                await Apply(shoppingCartInitialized, streamEvent.Metadata, cancellationToken);
                break;
            case ShoppingCartConfirmed shoppingCartConfirmed:
                await Apply(shoppingCartConfirmed, streamEvent.Metadata, cancellationToken);
                break;
            case ProductItemAddedToShoppingCart productItemAddedToShoppingCart:
                await Apply(productItemAddedToShoppingCart, streamEvent.Metadata, cancellationToken);
                break;
            case ProductItemRemovedFromShoppingCart productItemRemovedFromShoppingCart:
                await Apply(productItemRemovedFromShoppingCart, streamEvent.Metadata, cancellationToken);
                break;
        }
    }

    private async Task Apply(ShoppingCartInitialized @event, EventMetadata metadata,
        CancellationToken cancellationToken = default)
    {
        var (shoppingCartId, clientId) = @event;

        var newShoppingCartDetails = new ShoppingCartDetails
        {
            Id = shoppingCartId,
            ClientId = clientId,
            Status = ShoppingCartStatus.Pending,
            Version = 0,
            LastProcessedPosition = metadata.LogPosition
        };
        await eCommerceDbContext.Set<ShoppingCartDetails>().AddAsync(newShoppingCartDetails, cancellationToken);
        await eCommerceDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task Apply(ShoppingCartConfirmed @event, EventMetadata metadata,
        CancellationToken cancellationToken = default)
    {
        var shoppingCartDetails = await ShoppingCartDetails(@event.ShoppingCartId, cancellationToken);

        if (shoppingCartDetails is null)
            return;

        shoppingCartDetails.Status = ShoppingCartStatus.Confirmed;
        shoppingCartDetails.Version++;
        shoppingCartDetails.LastProcessedPosition = metadata.LogPosition;

        await eCommerceDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task Apply(ProductItemAddedToShoppingCart @event, EventMetadata metadata,
        CancellationToken cancellationToken = default)
    {
        var shoppingCartDetails = await ShoppingCartDetails(@event.ShoppingCartId, cancellationToken);

        if (shoppingCartDetails is null)
            return;

        var productItem = @event.ProductItem;
        var existingProductItem = shoppingCartDetails.ProductItems
            .FirstOrDefault(x => x.ProductId == @event.ProductItem.ProductId);

        if (existingProductItem == null)
        {
            shoppingCartDetails.ProductItems.Add(new ShoppingCartDetailsProductItem
            {
                ProductId = productItem.ProductId,
                Quantity = productItem.Quantity,
                UnitPrice = productItem.UnitPrice
            });
        }
        else
        {
            existingProductItem.Quantity += productItem.Quantity;
        }

        shoppingCartDetails.Version++;
        shoppingCartDetails.LastProcessedPosition = metadata.LogPosition;

        await eCommerceDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task Apply(ProductItemRemovedFromShoppingCart @event, EventMetadata metadata,
        CancellationToken cancellationToken = default)
    {
        var shoppingCartDetails = await ShoppingCartDetails(@event.ShoppingCartId, cancellationToken);

        if (shoppingCartDetails is null)
            return;

        var productItem = @event.ProductItem;
        var existingProductItem = shoppingCartDetails.ProductItems
            .Single(x => x.ProductId == @event.ProductItem.ProductId);

        if (existingProductItem.Quantity == productItem.Quantity)
        {
            shoppingCartDetails.ProductItems.Remove(existingProductItem);
        }
        else
        {
            existingProductItem.Quantity -= productItem.Quantity;
        }

        shoppingCartDetails.Version++;
        shoppingCartDetails.LastProcessedPosition = metadata.LogPosition;

        await eCommerceDbContext.SaveChangesAsync(cancellationToken);
    }

    private Task<ShoppingCartDetails?> ShoppingCartDetails(Guid id, CancellationToken cancellationToken)
    {
        return eCommerceDbContext.Set<ShoppingCartDetails>().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
