using Core.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Core.Projections;

public class ProjectionPublisher: IProjectionPublisher
{
    private readonly IServiceProvider _serviceProvider;

    public ProjectionPublisher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task PublishAsync<T>(StreamEvent<T> streamEvent, CancellationToken cancellationToken = default)
        where T : INotification
    {
        using var scope = _serviceProvider.CreateScope();
        var projections = scope.ServiceProvider.GetRequiredService<IEnumerable<IProjection>>();
        foreach (var projection in projections)
        {
            await projection.ProjectAsync(streamEvent, cancellationToken);
        }
    }

    public Task PublishAsync(StreamEvent streamEvent, CancellationToken cancellationToken = default)
    {
        var streamData = streamEvent.Data.GetType();

        var method = typeof(IProjectionPublisher)
            .GetMethods()
            .Single(m => m.Name == nameof(PublishAsync) && m.GetGenericArguments().Any())
            .MakeGenericMethod(streamData);

        return (Task)method
            .Invoke(this, new object[] { streamEvent, cancellationToken })!;
    }
}
