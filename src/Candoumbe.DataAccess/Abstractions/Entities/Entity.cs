﻿namespace Candoumbe.DataAccess.Abstractions.Entities
{
    using System;

    /// <summary>
    /// Base class for an entity.
    /// </summary>
    /// <typeparam name="TKey">Type of the identifier of the entity</typeparam>
    /// <typeparam name="TEntry">Type of the entity</typeparam>
    public abstract class Entity<TKey, TEntry> : BaseEntity<TKey>
        where TKey : IEquatable<TKey>
        where TEntry : class
    {
        private readonly TKey _id;

        ///<inheritdoc/>
        public override TKey Id => _id;

        /// <summary>
        /// Builds a new <see cref="Entity{TKey, TEntry}"/> instance.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> is default</exception>
        protected Entity(TKey id)
        {
            if (Equals(id, default))
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }
            _id = id;
        }
    }
}