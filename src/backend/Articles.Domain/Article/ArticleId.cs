using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace Conduit.Users.Domain.Article;

public class ArticleId : ComparableValueObject
{
    public string Value { get; }

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
    private ArticleId()
    {
        //for ef only
    }
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.

    private ArticleId(string id)
    {
        Value = id;
    }

    public static ArticleId Create()
    {
        return new ArticleId(Guid.NewGuid().ToString());
    }

    public static ArticleId Create(string id)
    {
        return new ArticleId(id);
    }

    protected override IEnumerable<IComparable> GetComparableEqualityComponents()
    {
        yield return Value;
    }
}
