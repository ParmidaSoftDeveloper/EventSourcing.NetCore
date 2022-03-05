using Core.Events;
using ECommerce.Core.Projections;
using ECommerce.Storage;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.ShoppingCarts.GettingCartById;

public class ShoppingCartDetailsProjection: IProjection
{
    private readonly ECommerceDbContext eCommerceDbContext;

    public ShoppingCartDetailsProjection(ECommerceDbContext eCommerceDbContext)
    {
        this.eCommerceDbContext = eCommerceDbContext;
    }

    public async Task ProjectAsync<T>(StreamEvent<T> streamEvent, CancellationToken cancellationToken = default)
    where T : INotification
    {
        switch (streamEvent.Data)
        {
            case ShoppingCartInitialized shoppingCartInitialized:
                await Apply(shoppingCartInitialized, cancellationToken);
                break;
            case ShoppingCartConfirmed shoppingCartConfirmed:
                await Apply(shoppingCartConfirmed, cancellationToken);
                break;
            case ProductItemAddedToShoppingCart productItemAddedToShoppingCart:
                await Apply(productItemAddedToShoppingCart, cancellationToken);
                break;
            case ProductItemRemovedFromShoppingCart productItemRemovedFromShoppingCart:
                await Apply(productItemRemovedFromShoppingCart, cancellationToken);
                break;
        }
    }

    private async Task Apply(ShoppingCartInitialized @event, CancellationToken cancellationToken = default)
    {
        var (shoppingCartId, clientId) = @event;

        var newShoppingCartDetails = new ShoppingCartDetails
        {
            Id = shoppingCartId, ClientId = clientId, Status = ShoppingCartStatus.Pending, Version = 0
        };
        await eCommerceDbContext.Set<ShoppingCartDetails>().AddAsync(newShoppingCartDetails, cancellationToken);
        await eCommerceDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task Apply(ShoppingCartConfirmed @event, CancellationToken cancellationToken = default)
    {
        var shoppingCartDetails = await ShoppingCartDetails(@event.ShoppingCartId, cancellationToken);

        if (shoppingCartDetails is null)
            return;

        shoppingCartDetails.Status = ShoppingCartStatus.Confirmed;
        shoppingCartDetails.Version++;
    }

    private async Task Apply(ProductItemAddedToShoppingCart @event, CancellationToken cancellationToken = default)
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

        await eCommerceDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task Apply(ProductItemRemovedFromShoppingCart @event, CancellationToken cancellationToken = default)
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

        await eCommerceDbContext.SaveChangesAsync(cancellationToken);
    }

    private Task<ShoppingCartDetails?> ShoppingCartDetails(Guid id, CancellationToken cancellationToken)
    {
        return eCommerceDbContext.Set<ShoppingCartDetails>().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

}
