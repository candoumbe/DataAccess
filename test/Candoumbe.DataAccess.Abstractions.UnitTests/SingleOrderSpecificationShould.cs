using System;
using FluentAssertions;
using FsCheck.Xunit;
using Xunit;

namespace Candoumbe.DataAccess.Abstractions.UnitTests;

public class SingleOrderSpecificationShould
{
    [Property]
    public void Throw_ArgumentNullException_When_null_is_passed(OrderDirection direction)
    {
        // Act
        Action ctorWithNullExpression = () => _ = new SingleOrderSpecification<object>(null, direction);

        // Assert
        ctorWithNullExpression.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void Create_instance_with_ascending_direction_when_direction_is_not_specified()
    {
        // Act
        SingleOrderSpecification<Person> specification = new (x => x.Name);

        // Assert
        specification.Orders.Should()
            .HaveCount(1)
            .And.ContainSingle(x => x.Direction == OrderDirection.Ascending);
    }
}