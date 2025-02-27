using Carts.ShoppingCarts;
using Carts.ShoppingCarts.InitializingCart;
using Core.Events;

namespace Carts.Tests.Builders;

internal class CartBuilder
{
    private readonly Queue<IEvent> eventsToApply = new();

    public CartBuilder Initialized()
    {
        var cartId = Guid.NewGuid();
        var clientId = Guid.NewGuid();

        eventsToApply.Enqueue(new ShoppingCartInitialized(cartId, clientId, ShoppingCartStatus.Pending));

        return this;
    }

    public static CartBuilder Create() => new();

    public ShoppingCart Build()
    {
        var cart = (ShoppingCart) Activator.CreateInstance(typeof(ShoppingCart), true)!;

        foreach (var @event in eventsToApply)
        {
            cart.When(@event);
        }

        return cart;
    }
}