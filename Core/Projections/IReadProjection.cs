using Core.Events;
using MediatR;

namespace Core.Projections;

// https://zimarev.com/blog/event-sourcing/projections/
// https://event-driven.io/en/how_to_do_events_projections_with_entity_framework/
// https://www.youtube.com/watch?v=bTRjO6JK4Ws
// https://www.eventstore.com/blog/event-sourcing-and-cqrs
public interface IReadProjection
{
    Task ProjectAsync<T>(EventEnvelope<T> streamEvent, CancellationToken cancellationToken = default)
        where T : INotification;
}
