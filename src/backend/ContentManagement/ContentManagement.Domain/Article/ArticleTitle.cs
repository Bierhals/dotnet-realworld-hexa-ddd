using System;
using System.Collections.Generic;
using Conduit.Domain.Common;
using CSharpFunctionalExtensions;

namespace Conduit.Users.Domain.Article;

public class ArticleTitle : ValueObject
{
    public string Value { get; }

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
    private ArticleTitle()
    {
        //for ef only
    }
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.

    private ArticleTitle(string title)
    {
        Value = title;
    }

    public static Result<ArticleTitle, Error> Create(string title)
    {
        string titleLowerCase = title.ToLower();

        return new ArticleTitle(titleLowerCase);
    }

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Value;
    }
}
