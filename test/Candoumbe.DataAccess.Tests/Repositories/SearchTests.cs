using System;
using Candoumbe.DataAccess.Repositories;
using FluentAssertions;
using Xunit;

namespace Candoumbe.DataAccess.UnitTests.Repositories;

public class SearchTests
{
    public class Person
    {
        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public DateTime BirthDate { get; set; }
    }

    public class SuperHero : Person
    {
        public string Nickname { get; set; }

        public int Height { get; set; }

        public Henchman Henchman { get; set; }
    }

    public class Henchman : SuperHero;

    /// <summary>
    /// Constructor builds a valid instance
    /// </summary>
    [Fact]
    public void CtorBuildValidInstance()
    {
        // Act
        Search<Henchman> search = new();

        // Assert
        search.Filter.Should()
            .BeNull();
        search.Order.Should()
            .BeNull();
    }
}