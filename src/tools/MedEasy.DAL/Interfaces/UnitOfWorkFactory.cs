namespace MedEasy.DAL.Interfaces
{
    public abstract class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        public abstract IUnitOfWork New();
    }
}