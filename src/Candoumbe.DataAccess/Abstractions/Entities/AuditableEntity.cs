namespace Candoumbe.DataAccess.Abstractions.Entities;

using NodaTime;

using System;

/// <summary>
/// Represents an entity with auditable properties
/// </summary>
/// <typeparam name="TKey">Type of the identifier of the entity</typeparam>
/// <typeparam name="TEntry">Type of the entity</typeparam>
public abstract class AuditableEntity<TKey, TEntry> : Entity<TKey, TEntry>, IAuditableEntity
    where TKey : IEquatable<TKey>
    where TEntry : class
{
    ///<inheritdoc/>
    public Instant? CreatedDate { get; set; }

    ///<inheritdoc/>
    public string CreatedBy { get; set; }

    ///<inheritdoc/>
    public Instant? UpdatedDate { get; set; }

    ///<inheritdoc/>
    public string UpdatedBy { get; set; }

    ///<inheritdoc/>
    protected AuditableEntity(TKey id) : base(id) { }
}