using Core.Aggregates;
using Core.Events;
using Marten;

namespace Core.Marten.Aggregates;

public static class AggregateExtensions
{
    public static async Task StoreAndPublishEvents(
        this IHaveAggregate haveAggregate,
        IDocumentSession session,
        IEventBus eventBus,
        CancellationToken cancellationToken = default
    )
    {
        var uncommitedEvents = haveAggregate.DequeueUncommittedEvents();
        session.Events.Append(haveAggregate.Id, uncommitedEvents);
        await session.SaveChangesAsync(cancellationToken);
        await eventBus.Publish(uncommitedEvents, cancellationToken);
    }
}
