using Core.Aggregates;
using Core.Events;

namespace Core.Testing;

public static class AggregateExtensions
{
    public static T? PublishedEvent<T>(this IHaveAggregate haveAggregate) where T : class, IEvent
    {
        return haveAggregate.DequeueUncommittedEvents().LastOrDefault() as T;
    }
}
