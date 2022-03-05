using Core.Events;
using MediatR;

namespace ECommerce.Core.Projections;

// https://zimarev.com/blog/event-sourcing/projections/
public interface IProjection
{
    Task ProjectAsync<T>(EventEnvelope<T> streamEvent, CancellationToken cancellationToken = default)
        where T : INotification;
}
