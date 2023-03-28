namespace Candoumbe.DataAccess.EFStore.UnitTests
{
    using Candoumbe.DataAccess.Abstractions;

    using FakeItEasy;

    using FsCheck;
    using FsCheck.Xunit;

    using Moq;

    using Xunit;
    using Xunit.Categories;

    using Times = Moq.Times;

    [UnitTest]
    public class EntityFrameworkUnitOfWorkShould
    {
        private readonly FakeDbContext _fakeDbContext;
        private readonly Mock<IRepositoryFactory<FakeDbContext>> _repositoryFactoryMock;
        private readonly EntityFrameworkUnitOfWork<FakeDbContext> _sut;

        public EntityFrameworkUnitOfWorkShould()
        {
            _fakeDbContext = A.Fake<FakeDbContext>();
            _repositoryFactoryMock = new Mock<IRepositoryFactory<FakeDbContext>>();
            _sut = new EntityFrameworkUnitOfWork<FakeDbContext>(_fakeDbContext, _repositoryFactoryMock.Object);
        }

        [Fact]
        public void Call_Factory_On_first_call_to_get_a_repository()
        {
            // Act
            IRepository<Dummy> repository = _sut.Repository<Dummy>();

            // Arrange
            _repositoryFactoryMock.Verify(mock => mock.NewRepository<Dummy>(It.IsAny<FakeDbContext>()), Times.Once);
            _repositoryFactoryMock.VerifyNoOtherCalls();
        }

        [Property]
        public void Call_Factory_only_once_first_call_to_get_a_repository(PositiveInt positiveInt)
        {
            // Arrange
            int tries = positiveInt.Item;

            // Act
            for (int i = 0; i < tries; i++)
            {
                IRepository<Dummy> repository = _sut.Repository<Dummy>();
            }

            // Arrange
            _repositoryFactoryMock.Verify(mock => mock.NewRepository<Dummy>(It.IsAny<FakeDbContext>()), Times.Once);
            _repositoryFactoryMock.VerifyNoOtherCalls();
        }
    }
}
