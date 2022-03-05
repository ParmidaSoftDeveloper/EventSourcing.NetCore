using System;
using System.Threading;
using System.Threading.Tasks;
using Core.EventStoreDB.OptimisticConcurrency;
using Core.EventStoreDB.Repository;
using Core.Tracing;
using Core.Tracing.Causation;
using Core.Tracing.Correlation;
using ECommerce.Core.EventStoreDB;
using ECommerce.Pricing.ProductPricing;
using ECommerce.ShoppingCarts.ProductItems;
using EventStore.Client;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.ShoppingCarts.AddingProductItem;

public record AddProductItemToShoppingCart(
    Guid ShoppingCartId,
    ProductItem ProductItem
): IRequest<ProductItemAddedToShoppingCart>
{
    public static AddProductItemToShoppingCart From(Guid? cartId, ProductItem? productItem)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));
        if (productItem == null)
            throw new ArgumentOutOfRangeException(nameof(productItem));

        return new AddProductItemToShoppingCart(cartId.Value, productItem);
    }
}

public class AddProductItemToShoppingCartHandler:
    IRequestHandler<AddProductItemToShoppingCart, ProductItemAddedToShoppingCart>
{
    private readonly IProductPriceCalculator productPriceCalculator;
    private readonly EventStoreClient eventStoreClient;
    private readonly IServiceProvider serviceProvider;


    public AddProductItemToShoppingCartHandler(IProductPriceCalculator productPriceCalculator,
        EventStoreClient eventStoreClient, IServiceProvider serviceProvider)
    {
        this.productPriceCalculator = productPriceCalculator;
        this.eventStoreClient = eventStoreClient;
        this.serviceProvider = serviceProvider;
    }

    public async Task<ProductItemAddedToShoppingCart> Handle(AddProductItemToShoppingCart command, CancellationToken
        cancellationToken)
    {
        var (cartId, productItem) = command;

        var shoppingCart = await eventStoreClient.Find(
            ShoppingCart.Default,
            ShoppingCart.When,
            ShoppingCart.MapToStreamId(cartId),
            cancellationToken);

        if (shoppingCart.IsClosed)
            throw new InvalidOperationException(
                $"Adding product item for cart in '{shoppingCart.Status}' status is not allowed.");

        var pricedProductItem = productPriceCalculator.Calculate(productItem);
        var @event = new ProductItemAddedToShoppingCart(
            cartId,
            pricedProductItem
        );

        var version = serviceProvider.GetRequiredService<EventStoreDBExpectedStreamRevisionProvider>().Value;

        var traceMetadata = new TraceMetadata(
            serviceProvider.GetRequiredService<ICorrelationIdProvider>().Get(),
            serviceProvider.GetRequiredService<ICausationIdProvider>().Get()
        );

        // handling optimistic concurrency
        var nextVersion = await eventStoreClient.Append(ShoppingCart.MapToStreamId(cartId), @event, version ?? 0,
            traceMetadata,
            cancellationToken);

        serviceProvider.GetRequiredService<EventStoreDBNextStreamRevisionProvider>().Set(nextVersion);

        return @event;
    }
}
