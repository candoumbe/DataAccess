namespace Candoumbe.DataAccess.Abstractions.Entities
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Base class for entities
    /// </summary>
    /// <typeparam name="TKey">Type of the identifier of the entity</typeparam>
    public abstract class BaseEntity<TKey> : IEqualityComparer<TKey>, IEquatable<TKey>, IEntity<TKey>
        where TKey : IEquatable<TKey>

    {
        /// <inheritdoc/>
        public virtual TKey Id { get; }

        ///<inheritdoc/>
        public virtual bool Equals(TKey x, TKey y) => x?.Equals(y) ?? false;

        ///<inheritdoc/>
        public virtual int GetHashCode(TKey obj) => obj?.GetHashCode() ?? 0;

        ///<inheritdoc/>
        public virtual bool Equals(TKey other) => Equals(this, other);
    }
}