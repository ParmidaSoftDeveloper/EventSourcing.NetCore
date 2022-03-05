using Core.Events;

namespace ECommerce.ShoppingCarts.GettingCarts;

public class ShoppingCartShortInfoProjection
{
    public static ShoppingCartShortInfo Handle(EventEnvelope<ShoppingCartInitialized> @event)
    {
        var (shoppingCartId, clientId) = @event.Data;

        return new ShoppingCartShortInfo
        {
            Id = shoppingCartId,
            ClientId = clientId,
            TotalItemsCount = 0,
            Status = ShoppingCartStatus.Pending,
            Version = 0,
            LastProcessedPosition = @event.Metadata.LogPosition
        };
    }

    public static void Handle(EventEnvelope<ShoppingCartConfirmed> @event, ShoppingCartShortInfo view)
    {
        if (view.LastProcessedPosition >= @event.Metadata.LogPosition)
            return;

        view.Status = ShoppingCartStatus.Confirmed;
        view.Version++;
        view.LastProcessedPosition = @event.Metadata.LogPosition;
    }

    public static void Handle(EventEnvelope<ProductItemAddedToShoppingCart> @event, ShoppingCartShortInfo view)
    {
        if (view.LastProcessedPosition >= @event.Metadata.LogPosition)
            return;

        var productItem = @event.Data.ProductItem;

        view.TotalItemsCount += productItem.Quantity;
        view.TotalPrice += productItem.TotalPrice;
        view.Version++;
        view.LastProcessedPosition = @event.Metadata.LogPosition;
    }

    public static void Handle(EventEnvelope<ProductItemRemovedFromShoppingCart> @event, ShoppingCartShortInfo view)
    {
        if (view.LastProcessedPosition >= @event.Metadata.LogPosition)
            return;

        var productItem = @event.Data.ProductItem;

        view.TotalItemsCount -= productItem.Quantity;
        view.TotalPrice -= productItem.TotalPrice;
        view.Version++;
        view.LastProcessedPosition = @event.Metadata.LogPosition;
    }
}
