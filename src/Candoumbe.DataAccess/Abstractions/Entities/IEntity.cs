namespace Candoumbe.DataAccess.Abstractions.Entities;

/// <summary>
/// Classes implementing this interface agree that <see cref="Id"/> should uniquely identifies them
/// </summary>
/// <typeparam name="TKey">Type of the repository identifier</typeparam>
public interface IEntity<out TKey>
{
    /// <summary>
    /// Identifier of the entity.
    /// </summary>
    TKey Id { get; }
}