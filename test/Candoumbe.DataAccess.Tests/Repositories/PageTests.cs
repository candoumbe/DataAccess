namespace Candoumbe.DataAccess.Tests.Repositories
{
    using Candoumbe.DataAccess.Repositories;
    using Candoumbe.Types.Numerics;

    using FluentAssertions;

    using FsCheck;
    using FsCheck.Xunit;

    using System;
    using System.Linq;

    using Xunit.Abstractions;
    using Xunit.Categories;

    [UnitTest]
    public class PageTests
    {
        private readonly ITestOutputHelper _outputTestHelper;

        public PageTests(ITestOutputHelper outputHelper)
        {
            _outputTestHelper = outputHelper;
        }

        [Property(Arbitrary = new[] { typeof(Generators) })]
        public void CtorWithNullEntriesShouldThrowArgumentNullException(NonNegativeInt totalGenerator, PageSize pageSize)
        {
            // Arrange
            NonNegativeInteger total = NonNegativeInteger.From(totalGenerator.Item);

            //Act
            Action action = () => new Page<object>(null, total, pageSize);

            //Assert
            action.Should()
                  .Throw<ArgumentNullException>()
                  .Where(ex => !string.IsNullOrWhiteSpace(ex.ParamName))
                  .Where(ex => !string.IsNullOrWhiteSpace(ex.Message));
        }

        [Property(Arbitrary = new[] { typeof(Generators) })]
        public void Given_a_pageSize_When_calling_Empty_Then_the_resulting_instance_should_have_correct_values_for_all_properties(PageSize pageSize)
        {
            //Act
            Page<object> pagedResult = Page<object>.Empty(pageSize);

            //Assert
            pagedResult.Should().NotBeNull();
            pagedResult.Size.Should().Be(pageSize);
            pagedResult.Count.Should().Be(PositiveInteger.One);
            pagedResult.Total.Should().Be(NonNegativeInteger.Zero);
            pagedResult.Entries.Should().BeEmpty();
        }

        [Property(Arbitrary = new[] { typeof(Generators) })]
        public void CheckPageCount(NonNegativeInt totalGenerator, PageSize pageSize)
        {
            // Arrange 
            NonNegativeInteger total = NonNegativeInteger.From(totalGenerator.Item);
            (PositiveInteger expected, string reason) = (int)Math.Ceiling((double)total.Value / pageSize) switch
            {
                < 1 => (PositiveInteger.One, "page count cannot be less than 1"),
                int count => (PositiveInteger.From(count), "Page count must be an numeric value")
            };

            //Act
            Page<object> page = new(Enumerable.Empty<object>(), total, pageSize);

            //Assert
            page.Count.Should().Be(expected, reason);
        }
    }
}