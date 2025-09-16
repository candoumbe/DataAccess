using System;
using System.Linq;
using System.Threading.Tasks;
using Candoumbe.DataAccess.Abstractions;
using Candoumbe.DataAccess.EFStore.UnitTests.Entities;
using Candoumbe.DataAccess.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Categories;

namespace Candoumbe.DataAccess.EFStore.UnitTests;

[UnitTest]
public class FirstTests(SqliteDatabaseFixture databaseFixture) : EntityFrameworkRepositoryTestsBase(databaseFixture), IClassFixture<SqliteDatabaseFixture>
{
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
        FilterSpecification<Hero> filter = new(x => x.Id == hero.Id);

        // Act
        Hero actual = await repository.First(filter);

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
        FilterSpecification<Hero> predicate = new(x => x.Id == hero.Id);

        // Act
        Hero actual = await repository.First(predicate,
                                             includedProperties: [IncludeClause<Hero>.Create(x => x.Acolytes.Where(item => item.Name == acolyte.Name))]);

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
        FilterSpecification<Hero> predicate = new(x => x.Id == hero.Id);

        // Act
        string actual = await repository.First(selector: x => x.Name,
                                               predicate);

        // Assert
        actual.Should().Be(acolyte.Name);
    }
}