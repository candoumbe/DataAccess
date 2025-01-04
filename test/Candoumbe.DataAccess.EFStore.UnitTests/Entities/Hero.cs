namespace Candoumbe.DataAccess.EFStore.UnitTests.Entities;

using Optional.Collections;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

public class Hero
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public IEnumerable<Acolyte> Acolytes => _acolytes.ToImmutableList();

    private readonly IList<Acolyte> _acolytes;

    public Hero(Guid id, string name)
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

    public void Dismiss(Guid acolyteId)
    {
        _acolytes.SingleOrNone(acc => acc.Id == acolyteId)
            .MatchSome(acolyte => _acolytes.Remove(acolyte));
    }
}