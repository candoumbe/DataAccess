namespace Candoumbe.DataAccess.RavenDb.UnitTests.Entities;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Optional.Collections;

/// <summary>
/// A <see cref="Hero"/> never goes alone in any battle, hence its <see cref="Acolyte"/>.
/// </summary>
public class Acolyte
{
    public string Id { get; init; }

    public string Name { get; }

    public IReadOnlyList<Weapon> Weapons => _weapons.ToImmutableList();

    private readonly IList<Weapon> _weapons;

    /// <summary>
    /// Builds a new <see cref="Acolyte"/> isntance
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    public Acolyte(string id, string name)
    {
        Id = id;
        Name = name;
        _weapons = new List<Weapon>();
    }

    public void Take(Weapon weapon)
    {
        if (weapon is null)
        {
            throw new ArgumentNullException(nameof(weapon));
        }

        _weapons.Add(weapon);
    }

    public void Throw(string weaponId)
    {
        _weapons.SingleOrNone(weapon => weapon.Id == weaponId)
            .MatchSome(weapon => _weapons.Remove(weapon));
    }
}

public class Weapon
{
    public string Id { get;  }

    public int Level { get; }

    public string Name { get; }

    public Weapon(string id, string name, int level)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException($"'{nameof(name)}' ne peut pas avoir une valeur null ou être un espace blanc.", nameof(name));
        }

        if (level < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(level), level, $"'{nameof(level)}' cannot be negative or zero");
        }

        Id = id;
        Name = name;
        Level = level;
    }
}