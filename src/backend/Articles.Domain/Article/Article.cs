using System;
using System.Collections.Generic;
using Conduit.Users.Domain.Article.Events;
using Conduit.Users.Domain.Common;
using Conduit.Users.Domain.User;
using CSharpFunctionalExtensions;

namespace Conduit.Users.Domain.Article;

public class Article: AggregateRoot<ArticleId>
{
    readonly List<string> _tagList;

    public ArticleSlug Slug { get; private set; }
    public ArticleTitle Title { get; private set; }
    public ArticleDescription Description { get; private set; }
    public ArticleBody Body { get; private set; }
    public IReadOnlyList<string> TagList => _tagList;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public UserId AuthorId { get; private set; }

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
    private Article()
    {
        //for ef only

        _tagList = new();
    }
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.

    private Article(ArticleId id, UserId authorId, ArticleSlug slug, ArticleTitle title, ArticleDescription description, ArticleBody body, IEnumerable<string> tagList, DateTime createdAt, DateTime updatedAt) : base(id)
    {
        Slug = slug;
        AuthorId = authorId;
        Title = title;
        Description = description;
        Body = body;
        _tagList = [..tagList];
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static Result<Article, Error> CreateNewArticle(UserId authorId, ArticleSlug slug, ArticleTitle title, ArticleDescription description, ArticleBody body, IEnumerable<string> tagList, IArticleCounter articleCounter)
    {
        return CanCreateNewArticle(slug, title, articleCounter)
            .Match(
                onFailure: error => error,
                onSuccess: () =>
                {
                    DateTime createdAt = DateTime.UtcNow;
                    ArticleId id = ArticleId.Create();

                    Article newArticle = new(id, authorId, slug, title, description, body, tagList, createdAt, createdAt);

                    newArticle.AddDomainEvent(new NewArticleCreatedDomainEvent(slug.Value));

                    return Result.Success<Article, Error>(newArticle);
                });
    }

    static UnitResult<Error> CanCreateNewArticle(ArticleSlug slug, ArticleTitle title, IArticleCounter articleCounter)
    {
        return Result.Combine(
            ArticleRules.SlugMustBeUniqueRule(slug, articleCounter),
            ArticleRules.TitleMustBeUniqueRule(title, articleCounter));
    }
}
