namespace MedEasy.DAL.Interfaces
{
    public interface IUnitOfWorkFactory
    {
        /// <summary>
        /// Creates a new <see cref="IUnitOfWork"/>
        /// </summary>
        /// <returns>instance implementing <see cref="IUnitOfWork"/></returns>
        IUnitOfWork NewUnitOfWork();
    }
}