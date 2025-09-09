using System;
using System.Linq;
using System.Threading.Tasks;
using Candoumbe.DataAccess.EFStore.UnitTests.Entities;
using Candoumbe.DataAccess.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Categories;

namespace Candoumbe.DataAccess.EFStore.UnitTests;

[UnitTest]
public class SingleTests : EntityFrameworkRepositoryTestsBase, IClassFixture<SqliteDatabaseFixture>, IAsyncLifetime
{
    public SingleTests(SqliteDatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task Given_hero_exists_and_has_an_acolyte_When_calling_SingleAsync_without_including_acolytes_Then_result_should_not_have_acolyte()
    {
        // Arrange
        Hero hero = new Hero(Guid.NewGuid(), Faker.Person.FullName);
        Acolyte acolyte = new Acolyte(Guid.NewGuid(), Faker.Person.FullName);

        hero.Enrolls(acolyte);

        SqliteStore.Heroes.Add(hero);
        await SqliteStore.SaveChangesAsync();

        DbContextOptionsBuilder<SqliteStore> optionsBuilder = new DbContextOptionsBuilder<SqliteStore>();
        optionsBuilder.UseSqlite(DatabaseFixture.Connection);
        SqliteStore context = new SqliteStore(optionsBuilder.Options);
        EntityFrameworkRepository<Hero, SqliteStore> repository = new EntityFrameworkRepository<Hero, SqliteStore>(context);

        // Act
        Hero actual = await repository.Single(x => x.Id == hero.Id, default);

        // Assert
        actual.Acolytes.Should()
            .BeEmpty("No instruction were defined to automatically include the acolytes property");
    }

    [Fact]
    public async Task Given_hero_exists_and_has_an_acolyte_When_calling_SingleAsync_with_including_acolytes_Then_result_should_not_have_acolyte()
    {
        // Arrange
        Hero hero = new Hero(Guid.NewGuid(), Faker.Person.FullName);
        Acolyte acolyte = new Acolyte(Guid.NewGuid(), Faker.Person.FullName);
        Weapon weapon = new Weapon(Guid.NewGuid(), "Bow", 1);
        acolyte.Take(weapon);
        hero.Enrolls(acolyte);

        SqliteStore.Heroes.Add(hero);
        await SqliteStore.SaveChangesAsync();

        DbContextOptionsBuilder<SqliteStore> optionsBuilder = new DbContextOptionsBuilder<SqliteStore>();
        optionsBuilder.UseSqlite(DatabaseFixture.Connection);
        SqliteStore context = new SqliteStore(optionsBuilder.Options);
        EntityFrameworkRepository<Hero, SqliteStore> repository = new EntityFrameworkRepository<Hero, SqliteStore>(context);

        // Act
        Hero actual = await repository.Single(x => x.Id == hero.Id,
            new[] { IncludeClause<Hero>.Create(x => x.Acolytes.Where(item => item.Name == acolyte.Name)) },
            default);

        // Assert
        actual.Acolytes.Should()
            .HaveSameCount(hero.Acolytes, $"'{nameof(Hero.Acolytes)}' was explicitely included").And
            .OnlyContain(acolyteActual => acolyteActual.Name == acolyte.Name);
    }
}