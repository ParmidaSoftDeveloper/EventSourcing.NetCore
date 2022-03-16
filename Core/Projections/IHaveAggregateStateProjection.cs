namespace Core.Projections;

public interface IHaveAggregateStateProjection
{
    void When(object @event);
}

public interface IVersionedHaveAggregateStateProjection: IHaveAggregateStateProjection
{
    public ulong LastProcessedPosition { get; set; }
}
