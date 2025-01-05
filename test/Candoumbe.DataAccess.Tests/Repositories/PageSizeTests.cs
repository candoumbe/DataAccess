namespace Candoumbe.DataAccess.UnitTests.Repositories;

using System;
using Candoumbe.DataAccess.Repositories;
using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using Generators;
using Xunit.Categories;

[UnitTest]
public class PageSizeTests
{
    [Property]
    public void Given_input_is_negative_When_calling_From_Then_ArgumentOutOfRangeException_should_be_thrown(NegativeInt negativeInt)
    {
        // Act
        Action ctorWithValueLessThanOne = () => PageSize.From(negativeInt.Item);

        // Assert
        ctorWithValueLessThanOne.Should()
            .Throw<ArgumentOutOfRangeException>("the input value cannot be negative");
    }

    [Property]
    public void Given_input_lt_0_When_calling_From_Then_No_Exception_should_be_thrown(NegativeInt negativeInt)
    {
        // Act
        Action ctorWithValueLessThanOne = () => PageSize.From(negativeInt.Item);

        // Assert
        ctorWithValueLessThanOne.Should()
            .Throw<ArgumentOutOfRangeException>("the input value cannot be zero")
            .Where(ex => !string.IsNullOrWhiteSpace(ex.ParamName))
            .Where(ex => !string.IsNullOrWhiteSpace(ex.Message));
    }

    [Property(Arbitrary = [typeof(Generators)])]
    public Property Given_an_initial_PageSize_When_adding_a_positive_integer_Then_resulting_PageSize_should_remains_within_MinValue_and_MaxValue(PageSize pageSize, PositiveInt positiveInt)
    {
        // Act
        PageSize actual = pageSize + positiveInt.Item;

        //Assert
        return (PageSize.MinValue <= actual && actual <= PageSize.MaxValue).ToProperty();
    }

    [Property(Arbitrary = [typeof(Generators)])]
    public Property Given_an_initial_PageSize_When_substracting_a_positive_integer_Then_resulting_PageSize_should_remains_within_MinValue_and_MaxValue(PageSize pageSize, PositiveInt positiveInt)
    {
        // Act
        PageSize actual = pageSize - positiveInt.Item;

        //Assert
        return (PageSize.MinValue <= actual && actual <= PageSize.MaxValue).ToProperty();
    }

    [Property]
    public Property Given_an_initial_PageSize_that_contain_MaxValue_When_adding_one_Then_the_result_should_be_MaxValue()
        => (PageSize.MaxValue + 1 == PageSize.MaxValue).ToProperty();

    [Property]
    public Property Given_an_initial_PageSize_that_contains_MinValue_When_substracting_1_Then_the_result_should_be_MinValue()
        => (PageSize.MinValue - 1 == PageSize.MinValue).ToProperty();

    [Property]
    public Property Given_an_initial_PageSize_that_contains_Zero_When_calling_increment_operator_Then_the_result_should_be_MaxValue()
        => (PageSize.One - 1 == PageSize.One).ToProperty();

    [Property(Arbitrary = [typeof(Generators)])]
    public Property Given_a_PageSize_and_a_value_When_calling_lt_operator_Then_PageSize_lt_value_should_be_the_same_as_PageSize_dot_Value_lt_value(PageSize pageSize, int right)
        => (pageSize.Value < right == pageSize < right).ToProperty();

    [Property(Arbitrary = [typeof(Generators)])]
    public Property Given_a_PageSize_and_a_value_When_calling_gt_operator_Then_PageSize_lt_value_should_be_the_same_as_PageSize_dot_Value_gt_value(PageSize pageSize, int right)
        => (pageSize.Value > right == pageSize > right).ToProperty();

    [Property(Arbitrary = [typeof(Generators)])]
    public void Given_a_PageSize_and_a_value_When_calling_lte_operator_Then_PageSize_lt_value_should_be_the_same_as_PageSize_dot_Value_lte_value(PageSize pageSize, int right)
        => (pageSize.Value > right == pageSize > right).ToProperty();

    [Property(Arbitrary = [typeof(Generators)])]
    public void Given_a_PageSize_and_a_value_When_calling_gte_operator_Then_PageSize_lt_value_should_be_the_same_as_PageSize_dot_Value_gte_value(PageSize pageSize, int right)
    {
        // Arrange
        bool expected = pageSize.Value >= right;

        // Act
        bool actual = pageSize >= right;

        // Assert
        actual.Should().Be(expected);
    }

    [Property(Arbitrary = [typeof(Generators)])]
    public void Given_a_PageSize_and_a_value_When_calling_eq_operator_Then_PageSize_lt_value_should_be_the_same_as_PageSize_dot_Value_eq_value(PageSize pageSize, int right)
    {
        // Arrange
        bool expected = pageSize.Value == right;

        // Act
        bool actual = pageSize == right;

        // Assert
        actual.Should().Be(expected);
    }

    [Property(Arbitrary = [typeof(Generators)])]
    public void Given_a_PageSize_and_a_value_When_calling_neq_operator_Then_PageSize_lt_value_should_be_the_same_as_PageSize_dot_Value_neq_value(PageSize pageSize, int right)
    {
        // Arrange
        bool expected = pageSize.Value != right;

        // Act
        bool actual = pageSize != right;

        // Assert
        actual.Should().Be(expected);
    }
}