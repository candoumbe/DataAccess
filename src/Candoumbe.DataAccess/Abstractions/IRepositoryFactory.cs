namespace Candoumbe.DataAccess.Abstractions
{
    /// <summary>
    /// Defines the contract of a factory that builds <see cref="IRepository{TEntry}"/>s.
    /// </summary>
    public interface IRepositoryFactory<TContext> where TContext : IStore
    {
        /// <summary>
        /// Creates a new <see cref="IRepository{TEntry}"/> that can handle <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">Type of entities <see cref="IRepository{TEntry}"/> will help interact with.</typeparam>
        /// <param name="dbContext"></param>
        /// <returns>a new <see cref="IRepository{TEntry}"/></returns>
        IRepository<TEntity> NewRepository<TEntity>(TContext dbContext) where TEntity : class;
    }
}