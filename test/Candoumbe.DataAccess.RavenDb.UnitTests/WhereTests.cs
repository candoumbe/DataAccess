namespace Candoumbe.DataAccess.RavenDb.UnitTests;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using DataFilters;
using Entities;
using FluentAssertions;
using Repositories;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using Xunit.Extensions.AssemblyFixture;

[UnitTest]
public class WhereTests(RavenDbFixture databaseFixture, ITestOutputHelper outputHelper)
    : RavenDbRepositoryTestsBase(databaseFixture), IAssemblyFixture<RavenDbFixture>
{
    public static TheoryData<IReadOnlyList<Hero>, Expression<Func<Hero, bool>>, PageSize, PageIndex, IOrder<Hero>, Expression<Func<Page<Hero>, bool>>> WhereWithNoIncludesCases
    {
        get
        {
            TheoryData<IReadOnlyList<Hero>, Expression<Func<Hero, bool>>, PageSize, PageIndex, IOrder<Hero>, Expression<Func<Page<Hero>, bool>>> cases = new()
            {
                {
                    [],
                    hero => hero.Name == "Superman",
                    PageSize.From(10),
                    PageIndex.From(1),
                    new Order<Hero>(nameof(Hero.Name)),
                    page => page.Count == 1
                            && page.Size == 10
                            && page.Total == 0
                            && page.Entries.Exactly(0)
                 }
            };

            {
                Hero hero = new(Guid.NewGuid().ToString(), Faker.Person.FullName);
                Acolyte acolyte = new(Guid.NewGuid().ToString(), Faker.Person.FullName);
                Weapon weapon = new(Guid.NewGuid().ToString(), "Bow", 1);
                acolyte.Take(weapon);
                hero.Enrolls(acolyte);

                cases.Add(
                 [hero],
                    h => h.Id == hero.Id,
                    PageSize.From(10),
                    PageIndex.From(1),
                    new Order<Hero>(nameof(Hero.Name)),
                    page => page.Count == 1
                           && page.Size == PageSize.From(10)
                           && page.Total == 1
                           && page.Entries.Once()
                           && page.Entries.Once(h => h.Id == hero.Id
                                                     && h.Acolytes.Exactly(0))
                );
            }

            {
                Hero batman = new(Guid.NewGuid().ToString(), "Batman");
                Acolyte robin = new(Guid.NewGuid().ToString(), "Robin");
                Weapon longStick = new(Guid.NewGuid().ToString(), "Long stick", 2);
                robin.Take(longStick);
                batman.Enrolls(robin);

                Hero greenArrow = new(Guid.NewGuid().ToString(), "Green Arrow");
                Acolyte redArrow = new(Guid.NewGuid().ToString(), "Red Arrow");
                Weapon bow = new(Guid.NewGuid().ToString(), "Bow & arrow", 2);
                redArrow.Take(bow);
                greenArrow.Enrolls(robin);

                Hero wonderWoman = new(Guid.NewGuid().ToString(), "Wonder Woman");

                cases.Add
                (
                    [batman, greenArrow, wonderWoman],
                    h => h.Id == batman.Id || h.Id == greenArrow.Id || h.Id == wonderWoman.Id,
                    PageSize.From(10),
                    PageIndex.From(1),
                    new Order<Hero>(nameof(Hero.Name)),
                    page => page.Count == 1
                            && page.Size == PageSize.From(10)
                            && page.Total == 3
                            && page.Entries.Exactly(3)
                            && page.Entries.Once(h => h.Id == batman.Id && h.Acolytes.Exactly(0))
                            && page.Entries.Once(h => h.Id == greenArrow.Id && h.Acolytes.Exactly(0))
                            && page.Entries.Once(h => h.Id == wonderWoman.Id && h.Acolytes.Exactly(0))
                );

                cases.Add
                (
                    [batman, greenArrow, wonderWoman],
                    h => h.Id == batman.Id,
                    PageSize.From(1),
                    PageIndex.From(1),
                    new Order<Hero>(nameof(Hero.Name)),
                    page => page.Count == 1
                            && page.Size == PageSize.From(1)
                            && page.Total == 1
                            && page.Entries.Once()
                            && page.Entries.Once(h => h.Id == batman.Id && h.Acolytes.Exactly(0))
                );

                cases.Add
                (
                    [batman, greenArrow, wonderWoman],
                    h => h.Id == batman.Id || h.Id == greenArrow.Id,
                    PageSize.From(1),
                    PageIndex.From(2),
                    new Order<Hero>(nameof(Hero.Name), OrderDirection.Descending),
                    page => page.Count == 2
                            && page.Size == PageSize.From(1)
                            && page.Total == 2
                            && page.Entries.Once()
                            && page.Entries.Once(h => h.Id == batman.Id && h.Acolytes.Exactly(0))
                );
            }

            return cases;
        }
    }

    [Theory]
    [MemberData(nameof(WhereWithNoIncludesCases))]
    public async Task Given_hero_exists_and_has_an_acolyte_When_calling_Where_without_including_acolytes_Then_result_should_not_have_acolyte(IReadOnlyList<Hero> heroes,
        Expression<Func<Hero, bool>> predicate,
        PageSize pageSize,
        PageIndex pageIndex,
        IOrder<Hero> orderBy,
        Expression<Func<Page<Hero>, bool>> pageExpectation)
    {
        // Arrange
        using var session = Store.OpenAsyncSession();
        foreach (var hero in heroes)
        {
            await session.StoreAsync(hero, $"hero/{hero.Id}");
        }
        await session.SaveChangesAsync();

        RavenDbRepository<Hero> repository = new (Store.OpenAsyncSession());

        // Act
        Page<Hero> page = await repository.Where(predicate: predicate,
                pageSize: pageSize,
                page: pageIndex,
                orderBy: orderBy,
                cancellationToken: CancellationToken.None);

        // Assert
        outputHelper.WriteLine($"Page expectation is {pageExpectation}");
        page.Should().Match(pageExpectation);
    }
}