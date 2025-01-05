namespace Candoumbe.DataAccess.EFStore.UnitTests;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Xunit;
using Xunit.Categories;

[UnitTest]
public class SingleTests : EntityFrameworkRepositoryTestsBase, IClassFixture<SqliteDatabaseFixture>
{
    public SingleTests(SqliteDatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task Given_hero_exists_and_has_an_acolyte_When_calling_SingleAsync_without_including_acolytes_Then_result_should_not_have_acolyte()
    {
        // Arrange
        Hero hero = new(Guid.NewGuid(), Faker.Person.FullName);
        Acolyte acolyte = new(Guid.NewGuid(), Faker.Person.FullName);

        hero.Enrolls(acolyte);

        SqliteDbContext.Heroes.Add(hero);
        await SqliteDbContext.SaveChangesAsync();

        DbContextOptionsBuilder<SqliteDbContext> optionsBuilder = new();
        optionsBuilder.UseSqlite(DatabaseFixture.Connection);
        SqliteDbContext context = new(optionsBuilder.Options);
        EntityFrameworkRepository<Hero, SqliteDbContext> repository = new(context);

        // Act
        Hero actual = await repository.Single(x => x.Id == hero.Id, CancellationToken.None);

        // Assert
        actual.Acolytes.Should()
            .BeEmpty("No instruction were defined to automatically include the acolytes property");
    }

    [Fact]
    public async Task Given_hero_exists_and_has_an_acolyte_When_calling_SingleAsync_with_including_acolytes_Then_result_should_not_have_acolyte()
    {
        // Arrange
        Hero hero = new(Guid.NewGuid(), Faker.Person.FullName);
        Acolyte acolyte = new(Guid.NewGuid(), Faker.Person.FullName);
        Weapon weapon = new(Guid.NewGuid(), "Bow", 1);
        acolyte.Take(weapon);
        hero.Enrolls(acolyte);

        SqliteDbContext.Heroes.Add(hero);
        await SqliteDbContext.SaveChangesAsync();

        DbContextOptionsBuilder<SqliteDbContext> optionsBuilder = new();
        optionsBuilder.UseSqlite(DatabaseFixture.Connection);
        SqliteDbContext context = new(optionsBuilder.Options);
        EntityFrameworkRepository<Hero, SqliteDbContext> repository = new(context);

        // Act
        Hero actual = await repository.Single(x => x.Id == hero.Id,
            [IncludeClause<Hero>.Create(x => x.Acolytes.Where(item => item.Name == acolyte.Name))],
            CancellationToken.None);

        // Assert
        actual.Acolytes.Should()
            .HaveSameCount(hero.Acolytes, $"'{nameof(Hero.Acolytes)}' was explicitly included").And
            .OnlyContain(acolyteActual => acolyteActual.Name == acolyte.Name);
    }
}