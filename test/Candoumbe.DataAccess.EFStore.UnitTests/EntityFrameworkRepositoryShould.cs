
using System;
using Candoumbe.DataAccess.EFStore.UnitTests.Entities;
using FluentAssertions;
using Xunit;

namespace Candoumbe.DataAccess.EFStore.UnitTests;

public class EntityFrameworkRepositoryShould
{
    [Fact]
    public void Throw_ArgumentNullException_When_constructor_is_called_with_a_null_parameter()
    {
        // Act
        Action ctor = () => new EntityFrameworkRepository<Hero, SqliteStore>(null);

        // Assert
        ctor.Should()
            .ThrowExactly<ArgumentNullException>("The dbcontext cannot be null")
            .Which.Message.Should().NotBeNullOrWhiteSpace();
    }
}