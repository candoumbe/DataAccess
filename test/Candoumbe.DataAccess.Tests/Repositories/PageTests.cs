namespace Candoumbe.DataAccess.Tests.Repositories;

using Candoumbe.DataAccess.Repositories;

using FluentAssertions;

using FsCheck;
using FsCheck.Xunit;

using System;
using System.Linq;

using Xunit;
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

    [Fact]
    public void CtorWithNullEntriesShouldThrowArgumentNullException()
    {
        //Act
        Action action = () => _ = new Page<object>(null, 0, PageSize.From(0));

        //Assert
        action.Should()
            .Throw<ArgumentNullException>()
            .Where(ex => !string.IsNullOrWhiteSpace(ex.ParamName))
            .Where(ex => !string.IsNullOrWhiteSpace(ex.Message));
    }

    [Property]
    public void Given_a_pageSize_When_calling_Empty_Then_the_resulting_instance_should_have_correct_values_for_all_properties(PositiveInt pageSize)
    {
        //Act
        Page<object> pagedResult = Page<object>.Empty(PageSize.From(pageSize.Item));

        //Assert
        pagedResult.Should().NotBeNull();
        pagedResult.Size.Value.Should().Be(pageSize.Item);
        pagedResult.Count.Should().Be(1);
        pagedResult.Total.Should().Be(0);
        pagedResult.Entries.Should().BeEmpty();
    }

    [Property]
    public void CtorWithNegativePageSizeShouldThrowArgumentOutOfRangeException(NegativeInt pageSize)
    {
        _outputTestHelper.WriteLine($"Page size : {pageSize.Item}");
        Action action = () => _ = new Page<object>([], 0, PageSize.From(pageSize.Item));
        action.Should().Throw<ArgumentOutOfRangeException>().Which
            .ParamName.Should()
            .BeEquivalentTo(nameof(Page<object>.Size));
    }

    [Property]
    public void CtorWithNegativeTotalShouldThrowArgumentOutOfRangeException(NegativeInt total)
    {
        _outputTestHelper.WriteLine($"{nameof(Page<object>.Total)} : {total.Item}");

        //Act
        Action action = () => _ = new Page<object>([], total.Item, PageSize.One);

        // Assert
        action.Should().Throw<ArgumentOutOfRangeException>().Which
            .ParamName.Should()
            .BeEquivalentTo(nameof(Page<object>.Total));
    }

    [Property(Arbitrary = [typeof(Generators)])]
    public void CheckPageCount(NonNegativeInt total, PageSize pageSize)
    {
        // Arrange 
        (int expected, string reason) = (int)Math.Ceiling((double)total.Item / pageSize) switch
        {
            < 1       => (1, "page count cannot be less than 1"),
            var count => (count, "Page count must be an numeric value")
        };

        //Act
        Page<object> page = new([], total.Item, pageSize);

        //Assert
        page.Count.Should().Be(expected, reason);
    }
}