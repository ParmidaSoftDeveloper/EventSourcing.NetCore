using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Core.Projections;

public class ProjectionPublisher : IProjectionPublisher
{
    private readonly IServiceProvider _serviceProvider;

    public ProjectionPublisher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : INotification
    {
        using var scope = _serviceProvider.CreateScope();
        var projections = scope.ServiceProvider.GetRequiredService<IEnumerable<IProjection>>();
        foreach (var projection in projections)
        {
            await projection.ProcessEventAsync(@event, cancellationToken);
        }
    }
}