namespace Candoumbe.DataAccess.Abstractions;

/// <summary>
/// Defines the contract of a factory that builds <see cref="IRepository{TEntry}"/>s.
/// </summary>
public interface IRepositoryFactory
{
    /// <summary>
    /// Creates a new repository to handle <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">Type of entities <see cref="IRepository{TEntry}"/> will help interact with.</typeparam>
    /// <param name="store"></param>
    /// <returns></returns>
    IRepository<TEntity> NewRepository<TEntity>(IStore store) where TEntity : class;
}