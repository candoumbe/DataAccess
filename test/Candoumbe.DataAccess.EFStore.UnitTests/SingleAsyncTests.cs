namespace Candoumbe.DataAccess.Tests.Repositories
{
    using Bogus;

    using Candoumbe.DataAccess.EFStore;
    using Candoumbe.DataAccess.EFStore.UnitTests;
    using Candoumbe.DataAccess.EFStore.UnitTests.Entities;
    using Candoumbe.DataAccess.Repositories;

    using FluentAssertions;

    using Microsoft.EntityFrameworkCore;

    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    public class SingleAsyncTests : EntityFrameworkRepositoryTestsBase, IClassFixture<SqliteDatabaseFixture>, IAsyncLifetime
    {
        public SingleAsyncTests(SqliteDatabaseFixture databaseFixture) : base(databaseFixture)
        {
        }

        [Fact]
        public async Task Given_hero_exists_and_has_an_acolyte_When_calling_SingleAsync_without_including_acolytes_Then_result_should_not_have_acolyte()
        {
            // Arrange
            Hero hero = new Hero(Guid.NewGuid(), Faker.Person.FullName);
            Acolyte acolyte = new Acolyte(Guid.NewGuid(), Faker.Person.FullName);

            hero.Enrolls(acolyte);

            SqliteDbContext.Heroes.Add(hero);
            await SqliteDbContext.SaveChangesAsync().ConfigureAwait(false);

            DbContextOptionsBuilder<SqliteDbContext> optionsBuilder = new DbContextOptionsBuilder<SqliteDbContext>();
            optionsBuilder.UseSqlite(DatabaseFixture.Connection);
            SqliteDbContext context = new SqliteDbContext(optionsBuilder.Options);
            EntityFrameworkRepository<Hero, SqliteDbContext> repository = new EntityFrameworkRepository<Hero, SqliteDbContext>(context);

            // Act
            Hero actual = await repository.SingleAsync(x => x.Id == hero.Id, default).ConfigureAwait(false);

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

            SqliteDbContext.Heroes.Add(hero);
            await SqliteDbContext.SaveChangesAsync().ConfigureAwait(false);

            DbContextOptionsBuilder<SqliteDbContext> optionsBuilder = new DbContextOptionsBuilder<SqliteDbContext>();
            optionsBuilder.UseSqlite(DatabaseFixture.Connection);
            SqliteDbContext context = new SqliteDbContext(optionsBuilder.Options);
            EntityFrameworkRepository<Hero, SqliteDbContext> repository = new EntityFrameworkRepository<Hero, SqliteDbContext>(context);

            // Act
            Hero actual = await repository.SingleAsync(x => x.Id == hero.Id,
                                                       new[] { IncludeClause<Hero>.Create(x => x.Acolytes.Where(item => item.Name == acolyte.Name)) },
                                                      default).ConfigureAwait(false);

            // Assert
            actual.Acolytes.Should()
                           .HaveSameCount(hero.Acolytes, $"'{nameof(Hero.Acolytes)}' was explicitely included").And
                           .OnlyContain(acolyteActual => acolyteActual.Name == acolyte.Name);

        }
    }
}