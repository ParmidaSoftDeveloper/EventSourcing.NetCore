using System.Threading;
using System.Threading.Tasks;
using Core.Events;
using MediatR;

namespace ECommerce.Core.Projections;

public interface IProjection
{
    Task ProcessEventAsync<T>(StreamEvent<T> streamEvent, CancellationToken cancellationToken = default)
        where T : INotification;
}
