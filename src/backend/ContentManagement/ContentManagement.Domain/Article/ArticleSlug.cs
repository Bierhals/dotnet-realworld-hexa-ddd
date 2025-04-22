/* using System;
using System.Collections.Generic;
using Conduit.Domain.Common;
using CSharpFunctionalExtensions;

namespace Conduit.Users.Domain.Article;

public class ArticleSlug : ValueObject
{
    public string Value { get; }

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
    private ArticleSlug()
    {
        //for ef only
    }
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.

    private ArticleSlug(string slug)
    {
        Value = slug;
    }

    public static Result<ArticleSlug, Error> Create(string slug)
    {
        return new ArticleSlug(slug);
    }

    public static Result<ArticleSlug, Error> CreateFromTitle(string title)
    {
        string slugLowerCase = title.ToLower();
        //TODO: nur a-zA-Z0-9 ansonsten -
        return new ArticleSlug(slugLowerCase);
    }

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Value;
    }
}
 */
