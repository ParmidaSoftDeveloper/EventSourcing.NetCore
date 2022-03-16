using Core.Events;

namespace Core.Aggregates;

// https://www.eventstore.com/blog/what-is-event-sourcing
// https://event-driven.io/en/how_to_get_the_current_entity_state_in_event_sourcing/?utm_source=event_sourcing_net
// https://zimarev.com/blog/event-sourcing/entities-as-streams/
// https://github.com/VenomAV/EventSourcingCQRS/blob/master/EventSourcingCQRS.Domain/Core/AggregateBase.cs
// https://github.com/gautema/CQRSlite/blob/master/Framework/CQRSlite/Domain/AggregateRoot.cs
// https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/Core/Aggregates/HaveAggregate.cs
// https://github.com/Eventuous/eventuous/blob/dev/src/Core/src/Eventuous/HaveAggregate.cs
public abstract class HaveAggregate: HaveAggregate<Guid>, IHaveAggregate
{
}

public abstract class HaveAggregate<T>: IHaveAggregate<T> where T : notnull
{
    public T Id { get; protected set; } = default!;

    public int Version { get; protected set; }

    [NonSerialized] private readonly Queue<IEvent> uncommittedEvents = new();

    public virtual void When(object @event) { }

    public void Apply(object @event)
    {
        When(@event);
    }

    public IEvent[] DequeueUncommittedEvents()
    {
        var dequeuedEvents = uncommittedEvents.ToArray();

        uncommittedEvents.Clear();

        return dequeuedEvents;
    }

    protected void Enqueue(IEvent @event)
    {
        uncommittedEvents.Enqueue(@event);
    }
}
