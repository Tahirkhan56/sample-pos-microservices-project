namespace Pos.BuildingBlocks.Domain.Primitives;

/// <summary>
/// Base class for entities: objects defined by identity rather than by their attributes.
/// </summary>
/// <typeparam name="TId">The identifier type.</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    protected Entity(TId id)
    {
        Id = id;
    }

    /// <summary>Required by ORMs for materialization.</summary>
    protected Entity()
    {
        Id = default!;
    }

    public TId Id { get; protected init; }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => Equals(left, right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !Equals(left, right);

    public bool Equals(Entity<TId>? other)
    {
        if (other is null)
        {
            return false;
        }

        return ReferenceEquals(this, other) || (other.GetType() == GetType() && Id.Equals(other.Id));
    }

    public override bool Equals(object? obj) => obj is Entity<TId> entity && Equals(entity);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
