/* using System;
using System.Collections.Generic;
using Conduit.Domain.Common;
using CSharpFunctionalExtensions;

namespace Conduit.Users.Domain.Article;

public class ArticleBody : ValueObject
{
    public string Value { get; }

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
    private ArticleBody()
    {
        //for ef only
    }
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.

    private ArticleBody(string description)
    {
        Value = description;
    }

    public static Result<ArticleBody, Error> Create(string description)
    {
        return new ArticleBody(description);
    }

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Value;
    }
}
 */
