namespace MockInterview.Domain.Common;

/// <summary>
/// An Aggregate Root is the "boss" entity — the entry point for a cluster of related entities.
/// External code should only interact with the aggregate root, never directly with child entities.
/// Examples: CvProfile, Interview.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    protected AggregateRoot(TId id) : base(id)
    {
    }
}
