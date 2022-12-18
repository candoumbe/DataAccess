namespace Candoumbe.DataAccess.Abstractions
{
    using Candoumbe.DataAccess.Repositories;

    /// <summary>
    /// Defines the contract of a factory that builds <see cref="IUnitOfWork"/>s.
    /// </summary>
    public interface IUnitOfWorkFactory
    {
        /// <summary>
        /// Creates a new <see cref="IUnitOfWork"/>
        /// </summary>
        /// <returns>instance implementing <see cref="IUnitOfWork"/></returns>
        IUnitOfWork NewUnitOfWork();
    }
}