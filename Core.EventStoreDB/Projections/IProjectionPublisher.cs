using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ECommerce.Core.Projections;

public interface IProjectionPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : INotification;
}