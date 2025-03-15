namespace Candoumbe.DataAccess.Tests.Repositories
{
    using Bogus;

    using Candoumbe.DataAccess.EFStore;
    using Candoumbe.DataAccess.EFStore.UnitTests;
    using Candoumbe.DataAccess.EFStore.UnitTests.Entities;
    using Candoumbe.DataAccess.Repositories;

    using FluentAssertions;

    using Microsoft.EntityFrameworkCore;

    using Optional;

    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    public class FirstOrDefaultTests : EntityFrameworkRepositoryTestsBase, IClassFixture<SqliteDatabaseFixture>, IAsyncLifetime
    {
        public FirstOrDefaultTests(SqliteDatabaseFixture databaseFixture) : base(databaseFixture)
        {
        }

        [Fact]
        public async Task Given_hero_exists_and_has_an_acolyte_When_calling_FirstOrDefaultAsync_without_including_acolytes_Then_result_should_not_have_acolyte()
        {
            // Arrange
            Hero hero = new Hero(Guid.NewGuid(), Faker.Person.FullName);
            Acolyte acolyte = new Acolyte(Guid.NewGuid(), Faker.Person.FullName);

            hero.Enrolls(acolyte);

            SqliteDbContext.Heroes.Add(hero);
            await SqliteDbContext.SaveChangesAsync();

            DbContextOptionsBuilder<SqliteDbContext> optionsBuilder = new DbContextOptionsBuilder<SqliteDbContext>();
            optionsBuilder.UseSqlite(DatabaseFixture.Connection);
            SqliteDbContext context = new SqliteDbContext(optionsBuilder.Options);
            EntityFrameworkRepository<Hero, SqliteDbContext> repository = new EntityFrameworkRepository<Hero, SqliteDbContext>(context);

            // Act
            Option<Hero> maybeHero = await repository.FirstOrDefault(x => x.Id == hero.Id, cancellationToken: default);

            // Assert
            maybeHero.Match(
                hero => hero.Acolytes.Should()
                                     .BeEmpty("No instruction were defined to automatically include the acolytes property"),
#if NET7_0_OR_GREATER
                () => throw new UnreachableException($"'{nameof(EntityFrameworkRepository<Hero, SqliteDbContext>.FirstOrDefault)}' must return the entity when it exists")
#else
                () => throw new NotSupportedException($"'{nameof(EntityFrameworkRepository<Hero, SqliteDbContext>.FirstOrDefault)}' must return the entity when it exists")
#endif
            );
        }

        [Fact]
        public async Task Given_hero_exists_and_has_an_acolyte_When_calling_FirstOrDefaultAsync_with_including_acolytes_Then_result_should_not_have_acolyte()
        {
            // Arrange
            Hero hero = new Hero(Guid.NewGuid(), Faker.Person.FullName);
            Acolyte acolyte = new Acolyte(Guid.NewGuid(), Faker.Person.FullName);
            Weapon weapon = new Weapon(Guid.NewGuid(), "Bow", 1);
            acolyte.Take(weapon);
            hero.Enrolls(acolyte);

            SqliteDbContext.Heroes.Add(hero);
            await SqliteDbContext.SaveChangesAsync();

            DbContextOptionsBuilder<SqliteDbContext> optionsBuilder = new DbContextOptionsBuilder<SqliteDbContext>();
            optionsBuilder.UseSqlite(DatabaseFixture.Connection);
            SqliteDbContext context = new SqliteDbContext(optionsBuilder.Options);
            EntityFrameworkRepository<Hero, SqliteDbContext> repository = new EntityFrameworkRepository<Hero, SqliteDbContext>(context);

            // Act
            Option<Hero> maybeHero = await repository.FirstOrDefault(x => x.Id == hero.Id,
                                                                        new[] { IncludeClause<Hero>.Create(x => x.Acolytes.Where(item => item.Name == acolyte.Name)) },
                                                                        default);

            // Assert
            maybeHero.Match(
                hero => hero.Acolytes.Should()
                                     .HaveSameCount(hero.Acolytes, $"'{nameof(Hero.Acolytes)}' was explicitely included").And
                                     .OnlyContain(acolyteActual => acolyteActual.Name == acolyte.Name),
#if NET7_0_OR_GREATER
                () => throw new UnreachableException($"'{nameof(EntityFrameworkRepository<Hero, SqliteDbContext>.FirstOrDefault)}' must return the entity when it exists")
#else
                () => throw new NotSupportedException($"'{nameof(EntityFrameworkRepository<Hero, SqliteDbContext>.FirstOrDefault)}' must return the entity when it exists")
#endif
            );
        }
    }
}
