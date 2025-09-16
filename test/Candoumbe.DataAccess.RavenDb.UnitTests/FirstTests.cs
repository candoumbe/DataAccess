using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using Candoumbe.DataAccess.Abstractions;
using Candoumbe.DataAccess.Repositories;
using FluentAssertions;
using Raven.Client.Documents.Session;
using Xunit;
using Xunit.Categories;

namespace Candoumbe.DataAccess.RavenDb.UnitTests;

[UnitTest]
public class FirstTests(RavenDbDatabaseFixture fixture) : IClassFixture<RavenDbDatabaseFixture>, IAsyncLifetime
{
    private static readonly Faker s_faker = new();
    private IAsyncDocumentSession _session;

    /// <inheritdoc />
    public Task InitializeAsync()
    {
        _session = fixture.Store.OpenAsyncSession();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DisposeAsync()
    {
        _session.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Given_hero_exists_and_has_an_acolyte_When_calling_First_without_including_acolytes_Then_result_should_not_have_acolyte()
    {
        // Arrange
        Hero hero = new Hero(Guid.NewGuid().ToString(), s_faker.Name.FullName());
        Acolyte acolyte = new Acolyte(Guid.NewGuid().ToString(), s_faker.Name.FullName());

        hero.Enrolls(acolyte);
        using (IAsyncDocumentSession session = fixture.Store.OpenAsyncSession())
        {
            await session.StoreAsync(hero, hero.Id.ToString());
            await session.SaveChangesAsync();
        }

        IRepository<Hero> repository = new RavenDbRepository<Hero>(_session);
        FilterSpecification<Hero> predicate = new(x => x.Id == hero.Id);

        // Act
        Hero actual = await repository.First(predicate, cancellationToken: CancellationToken.None);

        // Assert
        actual.Acolytes.Should()
            .NotBeEmpty("no instruction were defined to automatically include the acolytes property but in a document with an acolyte, the acolytes property should be included");
    }

    [Fact]
    public async Task Given_hero_exists_and_has_an_acolyte_When_calling_First_with_including_acolytes_Then_result_should_not_have_acolyte()
    {
        // Arrange
        Hero hero = new Hero(Guid.NewGuid().ToString(), s_faker.Person.FullName);
        Acolyte acolyte = new Acolyte(Guid.NewGuid().ToString(), s_faker.Person.FullName);
        Weapon weapon = new Weapon(Guid.NewGuid(), "Bow", 1);
        acolyte.Take(weapon);
        hero.Enrolls(acolyte);

        using (IAsyncDocumentSession session = fixture.Store.OpenAsyncSession())
        {
            await session.StoreAsync(hero);
            await session.SaveChangesAsync();
        }

        RavenDbRepository<Hero> repository = new(_session);
        FilterSpecification<Hero> predicate = new(x => x.Id == hero.Id);

        // Act
        Hero actual = await repository.First(predicate,
                                             includedProperties: [ IncludeClause<Hero>.Create(x => x.Acolytes.Select(item => item.Id)) ]);

        // Assert
        actual.Acolytes.Should()
            .HaveSameCount(hero.Acolytes, $"'{nameof(Hero.Acolytes)}' was explicitly included").And
            .OnlyContain(acolyteActual => acolyteActual.Name == acolyte.Name);
    }

    [Fact]
    public async Task Given_hero_exists_and_has_an_acolyte_When_calling_First_with_selector_Then_result_match_expectation()
    {
        // Arrange
        Hero hero = new Hero(Guid.NewGuid().ToString(), s_faker.Person.FullName);
        Acolyte acolyte = new Acolyte(Guid.NewGuid().ToString(), s_faker.Person.FullName);
        Weapon weapon = new Weapon(Guid.NewGuid(), "Bow", 1);
        acolyte.Take(weapon);
        hero.Enrolls(acolyte);

        using (IAsyncDocumentSession session = fixture.Store.OpenAsyncSession())
        {
            await session.StoreAsync(hero, hero.Id.ToString());
            await session.SaveChangesAsync();
        }

        RavenDbRepository<Hero> repository = new(_session);
        FilterSpecification<Hero> predicate = new(x => x.Id == hero.Id);

        // Act
        string actual = await repository.First(selector: x => x.Name,
                                               predicate);

        // Assert
        actual.Should().Be(acolyte.Name);
    }
}