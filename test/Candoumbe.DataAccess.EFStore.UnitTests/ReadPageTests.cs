using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Candoumbe.DataAccess.Abstractions;
using Candoumbe.DataAccess.EFStore.UnitTests.Entities;
using Candoumbe.DataAccess.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Candoumbe.DataAccess.EFStore.UnitTests;

[UnitTest]
public class ReadPageTests(SqliteDatabaseFixture databaseFixture, ITestOutputHelper outputHelper)
    : EntityFrameworkRepositoryTestsBase(databaseFixture), IClassFixture<SqliteDatabaseFixture>
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await SqliteStore.Database.EnsureCreatedAsync();
    }

    public override async Task DisposeAsync()
    {
        await SqliteStore.Database.EnsureDeletedAsync();
        await base.DisposeAsync();
    }

    public static TheoryData<IReadOnlyList<Hero>, PageSize, PageIndex, IOrderSpecification<Hero>, Page<Hero>>
        ReadPagesWithoutIncludesCases
    {
        get
        {
            TheoryData<IReadOnlyList<Hero>, PageSize, PageIndex, IOrderSpecification<Hero>, Page<Hero>> cases = new();

            cases.Add(
                [],
                PageSize.From(10),
                PageIndex.From(1),
                new SingleOrderSpecification<Hero>(h => h.Name),
                Page<Hero>.Empty(pageSize: PageSize.From(10))
            );

            {
                Hero hero = new(Guid.NewGuid(), Faker.Name.FullName());
                Acolyte acolyte = new(Guid.NewGuid(), Faker.Name.FullName());
                Weapon weapon = new(Guid.NewGuid(), "Bow", 1);
                acolyte.Take(weapon);
                hero.Enrolls(acolyte);

                cases.Add
                (
                    [hero],
                    PageSize.From(10),
                    PageIndex.From(1),
                    new SingleOrderSpecification<Hero>(h => h.Name),
                    new Page<Hero>([new Hero(hero.Id, hero.Name)], 1, PageSize.From(10))
                );
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

                cases.Add
                (
                    [batman, greenArrow, wonderWoman],
                    PageSize.From(2),
                    PageIndex.From(1),
                    new SingleOrderSpecification<Hero>(h => h.Name),
                    new Page<Hero>([
                            new Hero(batman.Id, batman.Name),
                            new Hero(greenArrow.Id, greenArrow.Name)
                        ],
                        3,
                        PageSize.From(2))
                );

                cases.Add
                (
                    [batman, greenArrow, wonderWoman],
                    PageSize.One,
                    PageIndex.From(1),
                    new SingleOrderSpecification<Hero>(h => h.Name),
                    new Page<Hero>([new Hero(batman.Id, batman.Name)], 3, PageSize.One)
                );

                cases.Add(
                    [batman, greenArrow, wonderWoman],
                    PageSize.One,
                    PageIndex.From(2),
                    new SingleOrderSpecification<Hero>(h => h.Name),
                    new Page<Hero>([new Hero(greenArrow.Id, greenArrow.Name)], 3, PageSize.One)
                );

                cases.Add(
                    [batman, greenArrow, wonderWoman],
                    PageSize.One,
                    PageIndex.From(3),
                    new SingleOrderSpecification<Hero>(h => h.Name),
                    new Page<Hero>([new Hero(wonderWoman.Id, wonderWoman.Name)], 3, PageSize.One)
                );
            }
            return cases;
        }
    }

    [Theory]
    [MemberData(nameof(ReadPagesWithoutIncludesCases))]
    public async Task
        Given_hero_exists_and_has_an_acolyte_When_calling_ReadPage_without_including_acolytes_Then_result_should_not_have_acolyte(
            IReadOnlyList<Hero> heroes,
            PageSize pageSize,
            PageIndex pageIndex,
            IOrderSpecification<Hero> orderBy,
            Page<Hero> expected)
    {
        // Arrange
        outputHelper.WriteLine($"Heroes in database : {heroes.Jsonify()}");
        outputHelper.WriteLine(
            $"Request : ({nameof(pageSize)}:{pageSize}, {nameof(pageIndex)}:{pageIndex}, {nameof(orderBy)}:{orderBy.Jsonify()}, {nameof(expected)}:{expected.Jsonify()})");

        SqliteStore.Heroes.AddRange(heroes);
        await SqliteStore.SaveChangesAsync();

        DbContextOptionsBuilder<SqliteStore> optionsBuilder = new();
        optionsBuilder.UseSqlite(DatabaseFixture.Connection);
        SqliteStore context = new(optionsBuilder.Options);

        EntityFrameworkRepository<Hero, SqliteStore> repository = new(context);

        // Act
        Page<Hero> actual = await repository.ReadPage(pageSize: pageSize, pageIndex: pageIndex, orderBy);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    public static
        TheoryData<IReadOnlyList<Hero>, PageSize, PageIndex, IReadOnlyList<IncludeClause<Hero>>, IOrderSpecification<Hero>, Page<Hero>> ReadPagesWithIncludesCases
    {
        get
        {
            TheoryData<IReadOnlyList<Hero>, PageSize, PageIndex, IReadOnlyList<IncludeClause<Hero>>, IOrderSpecification<Hero>, Page<Hero>> cases = new();

            cases.Add(
                [],
                PageSize.From(10),
                PageIndex.From(1),
                [IncludeClause<Hero>.Create(h => h.Acolytes)],
                new SingleOrderSpecification<Hero>(h => h.Name),
                Page<Hero>.Empty(PageSize.From(10)));

            {
                Hero hero = new(Guid.NewGuid(), Faker.Name.FullName());
                Acolyte acolyte = new(Guid.NewGuid(), Faker.Name.FullName());
                Weapon weapon = new(Guid.NewGuid(), "Bow", 1);
                acolyte.Take(weapon);
                hero.Enrolls(acolyte);

                cases.Add
                (
                    [hero],
                    PageSize.From(10),
                    PageIndex.From(1),
                    [],
                    new SingleOrderSpecification<Hero>(h => h.Name),
                    new Page<Hero>(
                                   [new Hero(hero.Id, hero.Name)],
                                   1,
                                   PageSize.From(10))
                );
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
                greenArrow.Enrolls(redArrow);

                Hero wonderWoman = new(Guid.NewGuid(), "Wonder Woman");

                cases.Add
                (
                    [batman, greenArrow, wonderWoman],
                    PageSize.From(10),
                    PageIndex.From(1),
                    [IncludeClause<Hero>.Create(h => h.Acolytes)],
                    new SingleOrderSpecification<Hero>(h => h.Name),
                    new Page<Hero>(new[] { batman, greenArrow, wonderWoman }
                                       .Select(static h =>
                                               {
                                                   Hero hero = new(h.Id, h.Name);
                                                   h.Acolytes.ForEach(acolyte => hero.Enrolls(new Acolyte(acolyte.Id, acolyte.Name)));
                                                   return hero;
                                               })
                                       .ToArray(),
                                   3,
                                   PageSize.From(10))
                );

                cases.Add
                (
                    [batman, greenArrow, wonderWoman],
                    PageSize.One,
                    PageIndex.From(1),
                    [IncludeClause<Hero>.Create(h => h.Acolytes)],
                    new SingleOrderSpecification<Hero>(h => h.Name),
                    new Page<Hero>(new[] { batman }.Select(static h =>
                                                           {
                                                               Hero hero = new(h.Id, h.Name);
                                                               h.Acolytes.ForEach(acolyte => hero.Enrolls(new Acolyte(acolyte.Id, acolyte.Name)));
                                                               return hero;
                                                           }).ToArray(),
                                   3,
                                   PageSize.From(1))
                );

                cases.Add
                (
                    [batman, greenArrow, wonderWoman],
                    PageSize.One,
                    PageIndex.From(2),
                    [IncludeClause<Hero>.Create(h => h.Acolytes)],
                    new SingleOrderSpecification<Hero>(h => h.Name),
                    new Page<Hero>(new[] { greenArrow }.Select(h =>
                                                               {
                                                                   Hero hero = new(h.Id, h.Name);
                                                                   h.Acolytes.ForEach(acolyte => hero.Enrolls(new Acolyte(acolyte.Id, acolyte.Name)));
                                                                   return hero;
                                                               }).ToArray(), 3, PageSize.One)
                );

                cases.Add
                (
                    [batman, greenArrow, wonderWoman],
                    PageSize.One,
                    PageIndex.From(3),
                    [IncludeClause<Hero>.Create(h => h.Acolytes)],
                    new SingleOrderSpecification<Hero>(h => h.Name),
                    new Page<Hero>([wonderWoman], 3, PageSize.One)
                );
            }

            return cases;
        }
    }

    [Theory]
    [MemberData(nameof(ReadPagesWithIncludesCases))]
    public async Task
        Given_hero_exists_and_has_an_acolyte_When_calling_ReadPage_with_includes_Then_result_should_have_acolytes(IReadOnlyList<Hero> heroes,
                                                                                                                  PageSize pageSize,
                                                                                                                  PageIndex pageIndex,
                                                                                                                  IReadOnlyList<IncludeClause<Hero>> includes,
                                                                                                                  IOrderSpecification<Hero> orderBy,
            Page<Hero> expected)
    {
        // Arrange
        outputHelper.WriteLine($"Heroes in database : {heroes.Jsonify()}");
        outputHelper.WriteLine(
            $"Request : ({nameof(pageSize)}:{pageSize}, {nameof(pageIndex)}:{pageIndex}, {nameof(expected)}:{expected.Jsonify()})");

        SqliteStore.Heroes.AddRange(heroes);
        await SqliteStore.SaveChangesAsync();

        DbContextOptionsBuilder<SqliteStore> optionsBuilder = new();
        optionsBuilder.UseSqlite(DatabaseFixture.Connection);
        SqliteStore context = new(optionsBuilder.Options);

        EntityFrameworkRepository<Hero, SqliteStore> repository = new(context);

        // Act
        Page<Hero> actual = await repository.ReadPage(pageSize, pageIndex, includedProperties: includes, orderBy);
        outputHelper.WriteLine($"Actual : {actual.Jsonify()}");

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task
        Given_hero_exists_and_has_an_acolyte_When_calling_ReadPageAsync_with_including_acolytes_Then_result_should_not_have_acolyte()
    {
        // Arrange
        PageSize pageSize = PageSize.From(10);
        Hero hero = new(Guid.NewGuid(), Faker.Person.FullName);
        Acolyte acolyte = new(Guid.NewGuid(), Faker.Person.FullName);
        Weapon weapon = new(Guid.NewGuid(), "Bow", 1);

        acolyte.Take(weapon);
        hero.Enrolls(acolyte);

        SqliteStore.Heroes.Add(hero);
        await SqliteStore.SaveChangesAsync();

        DbContextOptionsBuilder<SqliteStore> optionsBuilder = new();
        optionsBuilder.UseSqlite(DatabaseFixture.Connection);
        SqliteStore context = new(optionsBuilder.Options);
        IRepository<Hero> repository = new EntityFrameworkRepository<Hero, SqliteStore>(context);

        // Act
        Page<Hero> page = await repository.ReadPage(pageSize,
                                                    PageIndex.From(1),
                                                    [IncludeClause<Hero>.Create(h => h.Acolytes)],
                                                    orderBy: new SingleOrderSpecification<Hero>(h => h.Id));

        // Assert
        page.Count.Should().Be(1);
        page.Size.Value.Should().Be(pageSize);
        page.Entries.Should()
            .HaveCount(1).And
            .Contain(item => item.Id == hero.Id)
            .Which
            .Acolytes.Should()
            .HaveSameCount(hero.Acolytes).And
            .Contain(ac => ac.Id == acolyte.Id);
    }
}