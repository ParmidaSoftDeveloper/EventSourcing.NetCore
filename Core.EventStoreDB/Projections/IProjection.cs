using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace ECommerce.Core.Projections;

public interface IProjection
{
    Task ProcessEventAsync(INotification @event, CancellationToken cancellationToken = default);
}