using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Optional.Collections;

namespace Candoumbe.DataAccess.RavenDb.UnitTests;

public class Hero
{
    public string Id { get; init; }

    public string Name { get; init; }

    public IReadOnlyList<Acolyte> Acolytes
    {
        get => _acolytes.ToImmutableList();
        init => _acolytes = value?.ToList() ?? [];
    }

    private readonly List<Acolyte> _acolytes;

    public Hero(string id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException($"'{nameof(name)}' ne peut pas avoir une valeur null ou être un espace blanc.", nameof(name));
        }
        Id = id;
        Name = name;
        _acolytes = [];
    }

    public void Enrolls(Acolyte acolyte)
    {
        ArgumentNullException.ThrowIfNull(acolyte);

        _acolytes.Add(acolyte);
    }

    public void Dismiss(string acolyteId)
    {
        _acolytes.SingleOrNone(acc => acc.Id == acolyteId)
            .MatchSome(acolyte => _acolytes.Remove(acolyte));
    }
}