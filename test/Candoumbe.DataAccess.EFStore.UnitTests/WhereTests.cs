namespace Candoumbe.DataAccess.EFStore.UnitTests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Candoumbe.DataAccess.EFStore;
using Candoumbe.DataAccess.EFStore.UnitTests.Entities;
using Candoumbe.DataAccess.Repositories;
using DataFilters;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

[UnitTest]
public class WhereTests : EntityFrameworkRepositoryTestsBase, IClassFixture<SqliteDatabaseFixture>
{
    private readonly ITestOutputHelper _outputHelper;

    public WhereTests(SqliteDatabaseFixture databaseFixture, ITestOutputHelper outputHelper) : base(databaseFixture)
    {
        _outputHelper = outputHelper;
    }

    public static IEnumerable<object[]> WhereWithNoIncludesCases
    {
        get
        {
            yield return
            [
                Enumerable.Empty<Hero>(),
                (Expression<Func<Hero, bool>>)(hero => hero.Name == "Superman"),
                PageSize.From(10),
                PageIndex.From(1),
                new Order<Hero>(nameof(Hero.Name)),
                (Expression<Func<Page<Hero>, bool>>)(page => page.Count == 1
                                                             && page.Size == 10
                                                             && page.Total == 0
                                                             && page.Entries.Exactly(0) )
            ];

            {
                Hero hero = new(Guid.NewGuid(), Faker.Person.FullName);
                Acolyte acolyte = new(Guid.NewGuid(), Faker.Person.FullName);
                Weapon weapon = new(Guid.NewGuid(), "Bow", 1);
                acolyte.Take(weapon);
                hero.Enrolls(acolyte);

                yield return
                [
                    new []{hero},
                    (Expression<Func<Hero, bool>>)(h => h.Id == hero.Id),
                    PageSize.From(10),
                    PageIndex.From(1),
                    new Order<Hero>(nameof(Hero.Name)),
                    (Expression<Func<Page<Hero>, bool>>)(page => page.Count == 1
                                                                 && page.Size == PageSize.From(10)
                                                                 && page.Total == 1
                                                                 && page.Entries.Once()
                                                                 && page.Entries.Once(h => h.Id == hero.Id
                                                                                           && h.Acolytes.Exactly(0)))
                ];
            }

            {
                Hero batman = new(Guid.NewGuid(), "Batman");
                Acolyte robin = new(Guid.NewGuid(), "Robin");
                Weapon longStick = new(Guid.NewGuid(), "Long stick", 2);
                robin.Take(longStick);
                batman.Enrolls(robin);

                Hero greenArrow = new(Guid.NewGuid(), "Green Arrow");
                Acolyte redArrow = new(Guid.NewGuid(), "Red Arrow");
                Weapon bow = new(Guid.NewGuid(), "Bow & arrow", 2);
                redArrow.Take(bow);
                greenArrow.Enrolls(robin);

                Hero wonderWoman = new(Guid.NewGuid(), "Wonder Woman");

                yield return
                [
                    new []{batman, greenArrow, wonderWoman },
                    (Expression<Func<Hero, bool>>)(h => h.Id == batman.Id || h.Id == greenArrow.Id || h.Id == wonderWoman.Id),
                    PageSize.From(10),
                    PageIndex.From(1),
                    new Order<Hero>(nameof(Hero.Name)),
                    (Expression<Func<Page<Hero>, bool>>)(page => page.Count == 1
                                                                 && page.Size == PageSize.From(10)
                                                                 && page.Total == 3
                                                                 && page.Entries.Exactly(3)
                                                                 && page.Entries.Once(h => h.Id == batman.Id && h.Acolytes.Exactly(0))
                                                                 && page.Entries.Once(h => h.Id == greenArrow.Id && h.Acolytes.Exactly(0))
                                                                 && page.Entries.Once(h => h.Id == wonderWoman.Id && h.Acolytes.Exactly(0)))
                ];

                yield return
                [
                    new []{batman, greenArrow, wonderWoman },
                    (Expression<Func<Hero, bool>>)(h => h.Id == batman.Id),
                    PageSize.From(1),
                    PageIndex.From(1),
                    new Order<Hero>(nameof(Hero.Name)),
                    (Expression<Func<Page<Hero>, bool>>)(page => page.Count == 1
                                                                 && page.Size == PageSize.From(1)
                                                                 && page.Total == 1
                                                                 && page.Entries.Once()
                                                                 && page.Entries.Once(h => h.Id == batman.Id && h.Acolytes.Exactly(0)))
                ];

                yield return
                [
                    new []{batman, greenArrow, wonderWoman },
                    (Expression<Func<Hero, bool>>)(h => h.Id == batman.Id || h.Id == greenArrow.Id),
                    PageSize.From(1),
                    PageIndex.From(2),
                    new Order<Hero>(nameof(Hero.Name), OrderDirection.Descending),
                    (Expression<Func<Page<Hero>, bool>>)(page => page.Count == 2
                                                                 && page.Size == PageSize.From(1)
                                                                 && page.Total == 2
                                                                 && page.Entries.Once()
                                                                 && page.Entries.Once(h => h.Id == batman.Id && h.Acolytes.Exactly(0)))
                ];
            }
        }
    }

    [Theory]
    [MemberData(nameof(WhereWithNoIncludesCases))]
    public async Task Given_hero_exists_and_has_an_acolyte_When_calling_Where_without_including_acolytes_Then_result_should_not_have_acolyte(IEnumerable<Hero> heroes,
        Expression<Func<Hero, bool>> predicate,
        PageSize pageSize,
        PageIndex pageIndex,
        IOrder<Hero> orderBy,
        Expression<Func<Page<Hero>, bool>> pageExpectation)
    {
        // Arrange
        SqliteDbContext.Heroes.AddRange(heroes);
        await SqliteDbContext.SaveChangesAsync();

        DbContextOptionsBuilder<SqliteDbContext> optionsBuilder = new();
        optionsBuilder.UseSqlite(DatabaseFixture.Connection);
        SqliteDbContext context = new(optionsBuilder.Options);

        EntityFrameworkRepository<Hero, SqliteDbContext> repository = new(context);

        // Act
        Page<Hero> page = await repository.Where(predicate: predicate,
                pageSize: pageSize,
                page: pageIndex,
                orderBy: orderBy,
                cancellationToken: CancellationToken.None)
            ;

        // Assert
        _outputHelper.WriteLine($"Page expectation is {pageExpectation}");
        page.Should().Match(pageExpectation);
    }
}