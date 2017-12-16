using FluentAssertions;
using MedEasy.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MedEasy.DAL.Tests
{
    public class PagedResultTests
    {
        private ITestOutputHelper _outputTestHelper;

        public PagedResultTests(ITestOutputHelper outputHelper)
        {
            _outputTestHelper = outputHelper;
        }


        [Fact]
        public void CtorWithNullEntriesShouldThrowArgumentNullException()
        {
            //Act
            Action action = () => new Page<object>(null, 0, 0);

            //Assert
            ArgumentNullException exception = action.ShouldThrow<ArgumentNullException>().Which;
            exception.ParamName.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Default()
        {
            //Act
            IPagedResult<object> pagedResult = Page<object>.Default;

            //Assert
            pagedResult.Should().NotBeNull();
            pagedResult.PageSize.Should().Be(0);
            pagedResult.PageCount.Should().Be(0);
            pagedResult.Total.Should().Be(0);
            pagedResult.Entries.Should().BeEmpty();

        }


        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void CtorWithNegativePageSizeShouldThrowArgumentOutOfRangeException(int pageSize)
        {
            _outputTestHelper.WriteLine($"Page size : {pageSize}");

            Action action = () => new Page<object>(Enumerable.Empty<object>(), 0, pageSize);
            
            action.ShouldThrow<ArgumentOutOfRangeException>().Which
                .ParamName.Should()
                    .BeEquivalentTo($"{nameof(IPagedResult<object>.PageSize)}");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void CtorWithNegativeTotalShouldThrowArgumentOutOfRangeException(int total)
        {
            _outputTestHelper.WriteLine($"{nameof(IPagedResult<object>.Total)} : {total}");

            //Act
            Action action = () => new Page<object>(Enumerable.Empty<object>(), total, 1);

            // Assert
            action.ShouldThrow<ArgumentOutOfRangeException>().Which
                .ParamName.Should()
                    .BeEquivalentTo($"{nameof(IPagedResult<object>.Total)}");
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(1, 30, 1)]
        [InlineData(10, 5, 2)]
        [InlineData(12, 5, 3)]
        [InlineData(400, 30, 14)]
        public void CheckPageCount(int total, int pageSize, int expectedPageCount)
        {
            //Act
            IPagedResult<object> pagedResult = new Page<object>(Enumerable.Empty<object>(), total, pageSize);

            //Assert
            pagedResult.PageCount.Should().Be(expectedPageCount);
        }
    }
}
