using System;
using FluentAssertions;
using Xunit;

namespace Candoumbe.DataAccess.Abstractions.UnitTests;

public class MultiOrderSpecificationShould
{
    [Fact]
    public void Throw_ArgumentNullException_When_null_is_passed()
    {
        // Act
        Action ctorWithNullExpression = () => _ = new MultiOrderSpecification<object>(null);

        // Assert
        ctorWithNullExpression.Should().ThrowExactly<ArgumentNullException>();
    }
}