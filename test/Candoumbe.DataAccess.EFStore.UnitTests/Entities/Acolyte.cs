namespace Candoumbe.DataAccess.EFStore.UnitTests.Entities;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Ultimately.Collections;

public class Acolyte
{
    public Guid Id { get; }

    public string Name { get; }

    public IEnumerable<Weapon> Weapons => _weapons.ToImmutableList();

    private readonly IList<Weapon> _weapons;

    public Acolyte(Guid id, string name)
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

    public void Throw(Guid weaponId)
    {
        _weapons.SingleOrNone(weapon => weapon.Id == weaponId)
            .MatchSome(weapon => _weapons.Remove(weapon));
    }
}

public class Weapon
{
    public Guid Id { get; }

    public int Level { get; }

    public string Name { get; }

    public Weapon(Guid id, string name, int level)
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