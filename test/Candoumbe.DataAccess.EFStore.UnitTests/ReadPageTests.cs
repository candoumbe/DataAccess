namespace Candoumbe.DataAccess.Tests.Repositories
{
    using Bogus;

    using Candoumbe.DataAccess.Abstractions;
    using Candoumbe.DataAccess.EFStore;
    using Candoumbe.DataAccess.EFStore.UnitTests;
    using Candoumbe.DataAccess.EFStore.UnitTests.Entities;
    using Candoumbe.DataAccess.Repositories;
    using Candoumbe.Types.Numerics;

    using DataFilters;

    using FluentAssertions;

    using Microsoft.EntityFrameworkCore;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    public class ReadPageTests : EntityFrameworkRepositoryTestsBase, IClassFixture<SqliteDatabaseFixture>
    {
        public ReadPageTests(SqliteDatabaseFixture databaseFixture) : base(databaseFixture)
        {
        }

        public static IEnumerable<object[]> ReadPagesWithNoIncludesCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<Hero>(),
                    new PageSize(PositiveInteger.From(10)),
                    new PageIndex(PositiveInteger.From(1)),
                    new Order<Hero>(nameof(Hero.Name)),
                    (Expression<Func<Page<Hero>, bool>>)(page => page.Count == 1
                                                                 && page.Size == 10
                                                                 && page.Total == 0
                                                                 && page.Entries.Exactly(0) )
                };

                {
                    Hero hero = new Hero(Guid.NewGuid(), Faker.Person.FullName);
                    Acolyte acolyte = new Acolyte(Guid.NewGuid(), Faker.Person.FullName);
                    Weapon weapon = new Weapon(Guid.NewGuid(), "Bow", 1);
                    acolyte.Take(weapon);
                    hero.Enrolls(acolyte);

                    yield return new object[]
                    {
                        new []{hero},
                        new PageSize(PositiveInteger.From(10)),
                        new PageIndex(PositiveInteger.From(1)),
                        new Order<Hero>(nameof(Hero.Name)),
                        (Expression<Func<Page<Hero>, bool>>)(page => page.Count == 1
                                                                     && page.Size == 10
                                                                     && page.Total == 1
                                                                     && page.Entries.Once(h => h.Id == hero.Id
                                                                                               && h.Acolytes.Exactly(0)))
                    };
                }

                {
                    Hero batman = new Hero(Guid.NewGuid(), "Batman");
                    Acolyte robin = new Acolyte(Guid.NewGuid(), "Robin");
                    Weapon longStick = new Weapon(Guid.NewGuid(), "Long stick", 2);
                    robin.Take(longStick);
                    batman.Enrolls(robin);

                    Hero greenArrow = new Hero(Guid.NewGuid(), "Green Arrow");
                    Acolyte redArrow = new Acolyte(Guid.NewGuid(), "Red Arrow");
                    Weapon bow = new Weapon(Guid.NewGuid(), "Bow & arrow", 2);
                    redArrow.Take(bow);
                    greenArrow.Enrolls(robin);

                    Hero wonderWoman = new Hero(Guid.NewGuid(), "Wonder Woman");

                    yield return new object[]
                    {
                        new []{batman, greenArrow, wonderWoman },
                        new PageSize(PositiveInteger.From(10)),
                        new PageIndex(PositiveInteger.From(1)),
                        new Order<Hero>(nameof(Hero.Name)),
                        (Expression<Func<Page<Hero>, bool>>)(page => page.Count == 1
                                                                     && page.Size == 10
                                                                     && page.Total == 3
                                                                     && page.Entries.Exactly(3)
                                                                     && page.Entries.Once(h => h.Id == batman.Id && h.Acolytes.Exactly(0))
                                                                     && page.Entries.Once(h => h.Id == greenArrow.Id && h.Acolytes.Exactly(0))
                                                                     && page.Entries.Once(h => h.Id == wonderWoman.Id && h.Acolytes.Exactly(0)))
                    };

                    yield return new object[]
                    {
                        new []{batman, greenArrow, wonderWoman },
                        new PageSize(PositiveInteger.From(1)),
                        new PageIndex(PositiveInteger.From(1)),
                        new Order<Hero>(nameof(Hero.Name)),
                        (Expression<Func<Page<Hero>, bool>>)(page => page.Count == 3
                                                                     && page.Size == 1
                                                                     && page.Total == 3
                                                                     && page.Entries.Once()
                                                                     && page.Entries.Once(h => h.Id == batman.Id && h.Acolytes.Exactly(0)))
                    };

                    yield return new object[]
                    {
                        new []{batman, greenArrow, wonderWoman },
                        new PageSize(PositiveInteger.From(1)),
                        new PageIndex(PositiveInteger.From(2)),
                        new Order<Hero>(nameof(Hero.Name)),
                        (Expression<Func<Page<Hero>, bool>>)(page => page.Count == 3
                                                                     && page.Size == 1
                                                                     && page.Total == 3
                                                                     && page.Entries.Once()
                                                                     && page.Entries.Once(h => h.Id == greenArrow.Id && h.Acolytes.Exactly(0)))
                    };

                    yield return new object[]
                    {
                        new []{batman, greenArrow, wonderWoman },
                        new PageSize(PositiveInteger.From(1)),
                        new PageIndex(PositiveInteger.From(3)),
                        new Order<Hero>(nameof(Hero.Name)),
                        (Expression<Func<Page<Hero>, bool>>)(page => page.Count == 3
                                                                     && page.Size == 1
                                                                     && page.Total == 3
                                                                     && page.Entries.Once()
                                                                     && page.Entries.Once(h => h.Id == wonderWoman.Id && h.Acolytes.Exactly(0)))
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(ReadPagesWithNoIncludesCases))]
        public async Task Given_hero_exists_and_has_an_acolyte_When_calling_ReadPage_without_including_acolytes_Then_result_should_not_have_acolyte(IEnumerable<Hero> heroes,
                                                                                                                                                    PageSize pageSize,
                                                                                                                                                    PageIndex pageIndex,
                                                                                                                                                    IOrder<Hero> orderBy,
                                                                                                                                                    Expression<Func<Page<Hero>, bool>> pageExpectation)
        {
            // Arrange
            SqliteDbContext.Heroes.AddRange(heroes);
            await SqliteDbContext.SaveChangesAsync().ConfigureAwait(false);

            DbContextOptionsBuilder<SqliteDbContext> optionsBuilder = new DbContextOptionsBuilder<SqliteDbContext>();
            optionsBuilder.UseSqlite(DatabaseFixture.Connection);
            SqliteDbContext context = new SqliteDbContext(optionsBuilder.Options);

            EntityFrameworkRepository<Hero, SqliteDbContext> repository = new EntityFrameworkRepository<Hero, SqliteDbContext>(context);

            // Act
            Page<Hero> page = await repository.ReadPage(pageSize: pageSize,
                                                        page: pageIndex,
                                                        orderBy: orderBy,
                                                        cancellationToken: default)
                                              .ConfigureAwait(false);

            // Assert
            page.Should().Match(pageExpectation);
        }

        public static IEnumerable<object[]> ReadPagesWithIncludesCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<Hero>(),
                    new PageSize(PositiveInteger.From(10)),
                    new PageIndex(PositiveInteger.From(1)),
                    new[] { IncludeClause<Hero>.Create(h => h.Acolytes) },
                    new Order<Hero>(nameof(Hero.Name)),
                    (Expression<Func<Page<Hero>, bool>>)(page => page.Count == 1
                                                                 && page.Size == 10
                                                                 && page.Total == 0
                                                                 && page.Entries.Exactly(0) )
                };

                {
                    Hero hero = new Hero(Guid.NewGuid(), Faker.Person.FullName);
                    Acolyte acolyte = new Acolyte(Guid.NewGuid(), Faker.Person.FullName);
                    Weapon weapon = new Weapon(Guid.NewGuid(), "Bow", 1);
                    acolyte.Take(weapon);
                    hero.Enrolls(acolyte);

                    yield return new object[]
                    {
                        new []{hero},
                        new PageSize(PositiveInteger.From(10)),
                        new PageIndex(PositiveInteger.From(1)),
                        new[] { IncludeClause<Hero>.Create(h => h.Acolytes) },
                        new Order<Hero>(nameof(Hero.Name)),
                        (Expression<Func<Page<Hero>, bool>>)(page => page.Count == 1
                                                                     && page.Size == 10
                                                                     && page.Total == 1
                                                                     && page.Entries.Once(h => h.Id == hero.Id
                                                                                               && h.Acolytes.Exactly(hero.Acolytes.Count())))
                    };
                }

                {
                    Hero batman = new Hero(Guid.NewGuid(), "Batman");
                    Acolyte robin = new Acolyte(Guid.NewGuid(), "Robin");
                    Weapon longStick = new Weapon(Guid.NewGuid(), "Long stick", 2);
                    robin.Take(longStick);
                    batman.Enrolls(robin);

                    Hero greenArrow = new Hero(Guid.NewGuid(), "Green Arrow");
                    Acolyte redArrow = new Acolyte(Guid.NewGuid(), "Red Arrow");
                    Weapon bow = new Weapon(Guid.NewGuid(), "Bow & arrow", 2);
                    redArrow.Take(bow);
                    greenArrow.Enrolls(robin);

                    Hero wonderWoman = new Hero(Guid.NewGuid(), "Wonder Woman");

                    yield return new object[]
                    {
                        new []{batman, greenArrow, wonderWoman },
                        new PageSize(PositiveInteger.From(10)),
                        new PageIndex(PositiveInteger.From(1)),
                        new Order<Hero>(nameof(Hero.Name)),
                        new[] { IncludeClause<Hero>.Create(h => h.Acolytes) },
                        (Expression<Func<Page<Hero>, bool>>)(page => page.Count == 1
                                                                     && page.Size == 10
                                                                     && page.Total == 3
                                                                     && page.Entries.Exactly(3)
                                                                     && page.Entries.Once(h => h.Id == batman.Id && h.Acolytes.Exactly(0))
                                                                     && page.Entries.Once(h => h.Id == greenArrow.Id && h.Acolytes.Exactly(0))
                                                                     && page.Entries.Once(h => h.Id == wonderWoman.Id && h.Acolytes.Exactly(0)))
                    };

                    yield return new object[]
                    {
                        new []{batman, greenArrow, wonderWoman },
                        new PageSize(PositiveInteger.From(1)),
                        new PageIndex(PositiveInteger.From(1)),
                        new[] { IncludeClause<Hero>.Create(h => h.Acolytes) },
                        new Order<Hero>(nameof(Hero.Name)),
                        (Expression<Func<Page<Hero>, bool>>)(page => page.Count == 3
                                                                     && page.Size == PageSize.One
                                                                     && page.Total == 3
                                                                     && page.Entries.Once()
                                                                     && page.Entries.Once(h => h.Id == batman.Id
                                                                                               && h.Acolytes.Once()
                                                                                               && h.Acolytes.Once(acolyte => acolyte.Id == robin.Id)))
                    };

                    yield return new object[]
                    {
                        new []{batman, greenArrow, wonderWoman },
                        new PageSize(PositiveInteger.From(1)),
                        new PageIndex(PositiveInteger.From(2)),
                        new[] { IncludeClause<Hero>.Create(h => h.Acolytes) },
                        new Order<Hero>(nameof(Hero.Name)),
                        (Expression<Func<Page<Hero>, bool>>)(page => page.Count == 3
                                                                     && page.Size == 1
                                                                     && page.Total == 3
                                                                     && page.Entries.Once()
                                                                     && page.Entries.Once(h => h.Id == greenArrow.Id
                                                                                               && h.Acolytes.Once()
                                                                                               && h.Acolytes.Once(acolyte => acolyte.Id == redArrow.Id)))
                    };

                    yield return new object[]
                    {
                        new []{batman, greenArrow, wonderWoman },
                        new PageSize(PositiveInteger.From(1)),
                        new PageIndex(PositiveInteger.From(3)),
                        new[] { IncludeClause<Hero>.Create(h => h.Acolytes) },
                        new Order<Hero>(nameof(Hero.Name)),
                        (Expression<Func<Page<Hero>, bool>>)(page => page.Count == 3
                                                                     && page.Size == 1
                                                                     && page.Total == 3
                                                                     && page.Entries.Once()
                                                                     && page.Entries.Once(h => h.Id == wonderWoman.Id && h.Acolytes.Exactly(0)))
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(ReadPagesWithNoIncludesCases))]
        public async Task Given_hero_exists_and_has_an_acolyte_When_calling_ReadPage_without_including_acolytes_Then_result_should_have_acolytes(IEnumerable<Hero> heroes,
                                                                                                                                                 PageSize pageSize,
                                                                                                                                                 PageIndex pageIndex,
                                                                                                                                                 IOrder<Hero> orderBy,
                                                                                                                                                 Expression<Func<Page<Hero>, bool>> pageExpectation)
        {
            // Arrange
            SqliteDbContext.Heroes.AddRange(heroes);
            await SqliteDbContext.SaveChangesAsync().ConfigureAwait(false);

            DbContextOptionsBuilder<SqliteDbContext> optionsBuilder = new DbContextOptionsBuilder<SqliteDbContext>();
            optionsBuilder.UseSqlite(DatabaseFixture.Connection);
            SqliteDbContext context = new SqliteDbContext(optionsBuilder.Options);

            EntityFrameworkRepository<Hero, SqliteDbContext> repository = new EntityFrameworkRepository<Hero, SqliteDbContext>(context);

            // Act
            Page<Hero> page = await repository.ReadPage(pageSize,
                                                        pageIndex,
                                                        orderBy,
                                                        cancellationToken: default)
                                              .ConfigureAwait(false);

            // Assert
            page.Should().Match(pageExpectation);
        }

        [Fact]
        public async Task Given_hero_exists_and_has_an_acolyte_When_calling_ReadPageAsync_with_including_acolytes_Then_result_should_not_have_acolyte()
        {
            // Arrange
            PageSize pageSize = new PageSize(PositiveInteger.From(10));
            Hero hero = new Hero(Guid.NewGuid(), Faker.Person.FullName);
            Acolyte acolyte = new Acolyte(Guid.NewGuid(), Faker.Person.FullName);
            Weapon weapon = new Weapon(Guid.NewGuid(), "Bow", 1);

            acolyte.Take(weapon);
            hero.Enrolls(acolyte);

            SqliteDbContext.Heroes.Add(hero);
            await SqliteDbContext.SaveChangesAsync().ConfigureAwait(false);

            DbContextOptionsBuilder<SqliteDbContext> optionsBuilder = new DbContextOptionsBuilder<SqliteDbContext>();
            optionsBuilder.UseSqlite(DatabaseFixture.Connection);
            SqliteDbContext context = new SqliteDbContext(optionsBuilder.Options);
            IRepository<Hero> repository = new EntityFrameworkRepository<Hero, SqliteDbContext>(context);

            // Act
            Page<Hero> page = await repository.ReadPage(pageSize,
                                                             new PageIndex(PositiveInteger.From(1)),
                                                             new[]
                                                             {
                                                                 IncludeClause<Hero>.Create(h => h.Acolytes)
                                                             },
                                                             orderBy: new Order<Hero>(nameof(Hero.Id)),
                                                             cancellationToken: default)
                                              .ConfigureAwait(false);

            // Assert
            page.Count.Should().Be(PositiveInteger.One);
            page.Size.Value.Should().Be(pageSize.Value);
            page.Entries.Should()
                        .HaveCount(1).And
                        .Contain(item => item.Id == hero.Id)
                        .Which
                        .Acolytes.Should()
                                 .HaveSameCount(hero.Acolytes).And
                                 .Contain(ac => ac.Id == acolyte.Id);
        }
    }
}
