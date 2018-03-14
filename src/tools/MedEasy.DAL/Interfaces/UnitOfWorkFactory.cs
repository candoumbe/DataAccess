namespace MedEasy.DAL.Interfaces
{
    /// <summary>
    /// Factory to create <see cref="IUnitOfWork"/>s.
    /// </summary>
    public abstract class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        /// <summary>
        /// Starts a new unit of work
        /// </summary>
        /// <returns></returns>
        public abstract IUnitOfWork NewUnitOfWork();
    }
}