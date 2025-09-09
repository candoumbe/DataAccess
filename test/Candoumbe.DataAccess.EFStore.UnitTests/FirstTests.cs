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
public class FirstTests : EntityFrameworkRepositoryTestsBase, IClassFixture<SqliteDatabaseFixture>
{
    public FirstTests(SqliteDatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task Given_hero_exists_and_has_an_acolyte_When_calling_First_without_including_acolytes_Then_result_should_not_have_acolyte()
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
        Hero actual = await repository.First(predicate: x => x.Id == hero.Id, cancellationToken: default);

        // Assert
        actual.Acolytes.Should()
            .BeEmpty("No instruction were defined to automatically include the acolytes property");
    }

    [Fact]
    public async Task Given_hero_exists_and_has_an_acolyte_When_calling_First_with_including_acolytes_Then_result_should_not_have_acolyte()
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
        Hero actual = await repository.First(predicate: x => x.Id == hero.Id,
            includedProperties: new[] { IncludeClause<Hero>.Create(x => x.Acolytes.Where(item => item.Name == acolyte.Name)) },
            cancellationToken: default);

        // Assert
        actual.Acolytes.Should()
            .HaveSameCount(hero.Acolytes, $"'{nameof(Hero.Acolytes)}' was explicitely included").And
            .OnlyContain(acolyteActual => acolyteActual.Name == acolyte.Name);
    }

    [Fact]
    public async Task Given_hero_exists_and_has_an_acolyte_When_calling_First_with_selector_Then_result_match_expectation()
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
        string actual = await repository.First(predicate: x => x.Id == hero.Id,
            selector: x => x.Name,
            cancellationToken: default);

        // Assert
        actual.Should().Be(acolyte.Name);
    }
}