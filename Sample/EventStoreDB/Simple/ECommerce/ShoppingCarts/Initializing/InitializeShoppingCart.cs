using Core.EventStoreDB.OptimisticConcurrency;
using ECommerce.Core.EventStoreDB;
using EventStore.Client;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.ShoppingCarts.Initializing;

public record InitializeShoppingCart(
        Guid ShoppingCartId,
        Guid ClientId)
    : IRequest<ShoppingCartInitialized>
{
    public static InitializeShoppingCart From(Guid? cartId, Guid? clientId)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));
        if (clientId == null || clientId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(clientId));

        return new InitializeShoppingCart(cartId.Value, clientId.Value);
    }
}

public class InitializeShoppingCartHandler : IRequestHandler<InitializeShoppingCart, ShoppingCartInitialized>
{
    private readonly EventStoreClient eventStoreClient;
    private readonly EventStoreDBNextStreamRevisionProvider eventStoreDbNextStreamRevisionProvider;

    public InitializeShoppingCartHandler(EventStoreClient eventStoreClient, EventStoreDBNextStreamRevisionProvider eventStoreDbNextStreamRevisionProvider)
    {
        this.eventStoreClient = eventStoreClient;
        this.eventStoreDbNextStreamRevisionProvider = eventStoreDbNextStreamRevisionProvider;
    }

    public async Task<ShoppingCartInitialized> Handle(InitializeShoppingCart command,
        CancellationToken cancellationToken)
    {
        var (shoppingCartId, clientId) = command;

        var @event = new ShoppingCartInitialized(
            shoppingCartId,
            clientId
        );

       var nextVersion = await eventStoreClient.Append(ShoppingCart.MapToStreamId(command.ShoppingCartId), @event,
       cancellationToken);

       eventStoreDbNextStreamRevisionProvider.Set(nextVersion);

        return @event;
    }
}
