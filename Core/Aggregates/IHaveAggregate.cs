using Core.Events;
using Core.Projections;

namespace Core.Aggregates;

public interface IHaveAggregate: IHaveAggregate<Guid>
{
}

public interface IHaveAggregate<out T>: IHaveAggregateStateProjection
{
    T Id { get; }
    int Version { get; }

    IEvent[] DequeueUncommittedEvents();
}
