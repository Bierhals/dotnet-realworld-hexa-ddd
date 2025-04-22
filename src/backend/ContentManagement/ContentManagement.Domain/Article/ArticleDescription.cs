/* using System;
using System.Collections.Generic;
using Conduit.Users.Domain.Common;
using CSharpFunctionalExtensions;

namespace Conduit.Users.Domain.Article;

public class ArticleDescription : ValueObject
{
    public string Value { get; }

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
    private ArticleDescription()
    {
        //for ef only
    }
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.

    private ArticleDescription(string description)
    {
        Value = description;
    }

    public static Result<ArticleDescription, Error> Create(string description)
    {
        return new ArticleDescription(description);
    }

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Value;
    }
}
 */
