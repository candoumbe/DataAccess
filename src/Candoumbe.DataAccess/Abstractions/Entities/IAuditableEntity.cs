namespace Candoumbe.DataAccess.Abstractions.Entities;

using NodaTime;

/// <summary>
/// Contract for auditable entities
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// Gets the creation date of the entity.
    /// </summary>
    Instant? CreatedDate { get; set; }

    /// <summary>
    /// Gets the creator identifier.
    /// </summary>
    string CreatedBy { get; set; }

    /// <summary>
    /// Gets the last updated date of the entity.
    /// </summary>
    Instant? UpdatedDate { get; set; }

    /// <summary>
    /// Gets the identifier of the last modifier.
    /// </summary>
    string UpdatedBy { get; set; }
}