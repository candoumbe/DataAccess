namespace MedEasy.DAL.Interfaces
{
    public interface IUnitOfWorkFactory
    {
        /// <summary>
        /// Creates a new UnitOfWork
        /// </summary>
        /// <returns></returns>
        IUnitOfWork NewUnitOfWork();
    }
}