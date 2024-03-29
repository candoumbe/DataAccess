﻿namespace Candoumbe.DataAccess.Tests.Repositories
{
    using Bogus;

    using Candoumbe.DataAccess.Repositories;

    using FluentAssertions;

    using FsCheck;
    using FsCheck.Fluent;
    using FsCheck.Xunit;

    using System;

    using Xunit;
    using Xunit.Categories;

#if NET7_0_OR_GREATER
    using System.Diagnostics;
#endif

    [UnitTest]
    public class PageIndexTests
    {
        private static readonly Faker Faker = new();

        [Property]
        public void Given_input_is_negative_When_calling_From_Then_ArgumentOutOfRangeException_should_be_thrown(NonNegativeInt value)
        {
            // Act
            Action ctorWithValueLessThanOne = () => PageIndex.From(-value.Item);

            // Assert
            ctorWithValueLessThanOne.Should()
                .Throw<ArgumentOutOfRangeException>("the input value cannot be less than 1")
                .Which.Message.Should().MatchEquivalentOf("'?*' cannot be less than 1 *");
        }

        [Property(Arbitrary = new[] { typeof(Generators) })]
        public void Given_an_initial_PageIndex_When_adding_AdditiveIdentity_Then_the_resulting_PageIndex_should_be_equal_initial_value(PageIndex expected)
            => (expected + PageIndex.AdditiveIdentity == expected).ToProperty();

        [Property(Arbitrary = new[] { typeof(Generators) })]
        public Property Given_an_initial_PageIndex_When_substracting_by_AdditiveIdentity_Then_the_resulting_PageIndex_should_be_equal_initial_value(PageIndex value)
            => (value + PageIndex.AdditiveIdentity == value).ToProperty();

        [Property(Arbitrary = new[] { typeof(Generators) })]
        public void Given_an_initial_PageIndex_When_incrementing_by_positive_integer_Then_the_resulting_PageIndex_should_be_incremented_by_one(PageIndex pageIndex)
        {
            // Arrange
            PageIndex expected = PageIndex.From(pageIndex + 1);

            // Act
            PageIndex actual = PageIndex.From(pageIndex) + 1;

            // Assert
            actual.Should().Be(expected);
        }

        [Property(Arbitrary = new[] { typeof(Generators) })]
        public void Given_an_initial_PageIndex_When_substracting_any_integer_Then_the_resulting_PageIndex_should_be_decremented_by_one(PageIndex index, int value)
        {
            // Arrange

            // Act
            PageIndex actual = index + value;

            // Assert
            object _ = (index.Value, value) switch
            {
                (0, <= 0) => actual.Should().Be(PageIndex.Zero),
                ( >= 1, <= 0) => actual.Value.Should().BeGreaterThanOrEqualTo(PageIndex.MinValue),
                (int.MaxValue, _) or (_, int.MaxValue) => PageIndex.MaxValue,
                ( >= 1, >= 0) => actual.Value.Should()
                                             .BeInRange(index.Value, PageIndex.MaxValue.Value),
#if NET7_0_OR_GREATER
                _ => throw new UnreachableException("This case should never happen")
#else
                _ => throw new NotSupportedException("This case should never happen")
#endif

            };
        }

        [Fact]
        public void Given_an_initial_PageIndex_that_contain_max_value_When_calling_increment_operator_Then_the_result_should_be_MaxValue()
        {
            // Act
            PageIndex actual = PageIndex.MaxValue + 1;

            // Assert
            actual.Should().Be(PageIndex.MaxValue);
        }

        [Property(Arbitrary = new[] { typeof(Generators) })]
        public void Given_a_PageIndex_and_a_value_When_calling_lt_operator_Then_PageIndex_lt_value_should_be_the_same_as_PageIndex_dot_Value_lt_value(PageIndex pageIndex, int right)
        {
            // Arrange
            bool expected = pageIndex.Value < right;

            // Act
            bool actual = pageIndex < right;

            // Assert
            actual.Should().Be(expected);
        }

        [Property(Arbitrary = new[] { typeof(Generators) })]
        public void Given_a_PageIndex_and_a_value_When_calling_gt_operator_Then_PageIndex_lt_value_should_be_the_same_as_PageIndex_dot_Value_gt_value(PageIndex pageIndex, int right)
        {
            // Arrange
            bool expected = pageIndex.Value > right;

            // Act
            bool actual = pageIndex > right;

            // Assert
            actual.Should().Be(expected);
        }

        [Property(Arbitrary = new[] { typeof(Generators) })]
        public void Given_a_PageIndex_and_a_value_When_calling_lte_operator_Then_PageIndex_lt_value_should_be_the_same_as_PageIndex_dot_Value_lte_value(PageIndex pageIndex, int right)
        {
            // Arrange
            bool expected = pageIndex.Value <= right;

            // Act
            bool actual = pageIndex <= right;

            // Assert
            actual.Should().Be(expected);
        }

        [Property(Arbitrary = new[] { typeof(Generators) })]
        public void Given_a_PageIndex_and_a_value_When_calling_gte_operator_Then_PageIndex_lt_value_should_be_the_same_as_PageIndex_dot_Value_gte_value(PageIndex pageIndex, int right)
        {
            // Arrange
            bool expected = pageIndex.Value >= right;

            // Act
            bool actual = pageIndex >= right;

            // Assert
            actual.Should().Be(expected);
        }

        [Property(Arbitrary = new[] { typeof(Generators) })]
        public Property Given_a_PageIndex_and_a_value_When_calling_eq_operator_Then_PageIndex_lt_value_should_be_the_same_as_PageIndex_dot_Value_eq_value(PageIndex pageIndex, int right)
            => (pageIndex.Value == right == (pageIndex == right)).ToProperty();

        [Property(Arbitrary = new[] { typeof(Generators) })]
        public void Given_a_PageIndex_and_a_value_When_calling_neq_operator_Then_PageIndex_lt_value_should_be_the_same_as_PageIndex_dot_Value_neq_value(PageIndex pageIndex, int right)
             => (pageIndex.Value != right == (pageIndex != right)).ToProperty();

        [Property(Arbitrary = new[] { typeof(Generators) })]
        public void Given_a_PageIndex_When_multiplying_by_an_integer_Then_result_should_stay_withing_min_an_max_PageIndex_ranges(PageIndex initialValue, int right)
        {
            // Act
            PageIndex actual = initialValue * right;

            // Assert
            actual.Value.Should().BeInRange(PageIndex.Zero, PageIndex.MaxValue);
        }

        [Property(Arbitrary = new[] { typeof(Generators) })]
        public void Given_a_PageIndex_When_summing_with_an_integer_Then_result_should_stay_within_min_an_max_PageIndex_ranges(PageIndex initialValue, int right)
        {
            // Act
            PageIndex actual = initialValue + right;

            // Assert
            actual.Value.Should().BeInRange(PageIndex.Zero, PageIndex.MaxValue);
        }
    }
}
