using Core.Events;
using MediatR;

namespace ECommerce.Core.Projections;

public interface IProjectionPublisher
{
    Task PublishAsync<T>(EventEnvelope<T> streamEvent, CancellationToken cancellationToken = default)
        where T : INotification;

    Task PublishAsync(EventEnvelope streamEvent, CancellationToken cancellationToken = default);
}
