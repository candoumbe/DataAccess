namespace Candoumbe.DataAccess.RavenDb.UnitTests.Entities;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Optional.Collections;

public class Hero
{
    public string Id { get; init; }

    public string Name { get; init; }

    public IEnumerable<Acolyte> Acolytes => _acolytes.ToImmutableList();

    private readonly IList<Acolyte> _acolytes;

    public Hero(string id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException($"'{nameof(name)}' ne peut pas avoir une valeur null ou être un espace blanc.", nameof(name));
        }
        Id = id;
        Name = name;
        _acolytes = new List<Acolyte>();
    }

    public void Enrolls(Acolyte acolyte)
    {
        if (acolyte is null)
        {
            throw new ArgumentNullException(nameof(acolyte));
        }

        _acolytes.Add(acolyte);
    }

    public void Dismiss(string acolyteId)
    {
        _acolytes.SingleOrNone(acc => acc.Id == acolyteId)
            .MatchSome(acolyte => _acolytes.Remove(acolyte));
    }
}