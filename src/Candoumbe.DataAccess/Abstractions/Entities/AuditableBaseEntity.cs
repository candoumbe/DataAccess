namespace Candoumbe.DataAccess.Abstractions.Entities
{
    using NodaTime;

    using System;

    /// <summary>
    /// Base class for entity with auditable properties.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class AuditableBaseEntity<TKey> : BaseEntity<TKey>, IAuditableEntity
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Gets the creation
        /// </summary>
        public Instant? CreatedDate { get; set; }

        ///<inheritdoc/>
        public string CreatedBy { get; set; }

        ///<inheritdoc/>
        public Instant? UpdatedDate { get; set; }

        ///<inheritdoc/>
        public string UpdatedBy { get; set; }
    }
}