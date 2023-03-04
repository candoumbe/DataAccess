namespace Candoumbe.DataAccess.Tests.Repositories
{
    using Bogus;

    using FsCheck;
    using FsCheck.Xunit;

    using Xunit.Categories;

    [UnitTest]
    public class PageSizeTests
    {
        private readonly static Faker Faker = new Faker();

        [Property]
        public void Given_input_is_negative_When_calling_From_Then_ArgumentOutOfRangeException_should_be_thrown(NegativeInt negativeInt)
        {
        }
    }
}
