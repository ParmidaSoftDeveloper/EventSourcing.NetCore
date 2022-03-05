using Core.EventStoreDB.OptimisticConcurrency;
using Core.Tracing;
using Core.Tracing.Causation;
using Core.Tracing.Correlation;
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

public class InitializeShoppingCartHandler: IRequestHandler<InitializeShoppingCart, ShoppingCartInitialized>
{
    private readonly EventStoreClient eventStoreClient;
    private readonly IServiceProvider serviceProvider;

    public InitializeShoppingCartHandler(EventStoreClient eventStoreClient, IServiceProvider serviceProvider)
    {
        this.eventStoreClient = eventStoreClient;
        this.serviceProvider = serviceProvider;
    }

    public async Task<ShoppingCartInitialized> Handle(InitializeShoppingCart command,
        CancellationToken cancellationToken)
    {
        var (shoppingCartId, clientId) = command;

        var @event = new ShoppingCartInitialized(
            shoppingCartId,
            clientId
        );

        var eventStoreDbNextStreamRevisionProvider =
            serviceProvider.GetRequiredService<EventStoreDBNextStreamRevisionProvider>();

        var traceMetadata = new TraceMetadata(
            serviceProvider.GetRequiredService<ICorrelationIdProvider>().Get(),
            serviceProvider.GetRequiredService<ICausationIdProvider>().Get()
        );

        var nextVersion = await eventStoreClient.Append(ShoppingCart.MapToStreamId(command.ShoppingCartId), @event,
            traceMetadata, cancellationToken);

        eventStoreDbNextStreamRevisionProvider.Set(nextVersion);

        return @event;
    }
}
