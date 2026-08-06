using System;
using Conduit.Articles.Domain.Events;
using Conduit.Articles.Domain.Rules;
using Conduit.Articles.Domain.ValueObjects;
using Conduit.Shared.Domain;
using ErrorOr;

namespace Conduit.Articles.Domain;

/// <summary>
/// A comment written on an article. Its own aggregate: it has its own identity and its own rule
/// about who may remove it, and it refers to the article it was written on by id only.
/// </summary>
public sealed class Comment : AggregateRoot<CommentId>
{
    public ArticleId ArticleId { get; private set; }
    public AuthorUsername Author { get; private set; }
    public CommentBody Body { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

#pragma warning disable CS8618 // Non-nullable properties are populated by EF Core when materializing.
    private Comment() { } // for EF Core
#pragma warning restore CS8618

    private Comment(CommentId id, ArticleId articleId, AuthorUsername author, CommentBody body, DateTime createdAtUtc)
        : base(id)
    {
        ArticleId = articleId;
        Author = author;
        Body = body;
        CreatedAt = createdAtUtc;
        UpdatedAt = createdAtUtc;
    }

    public static Comment Post(
        CommentId id,
        ArticleId articleId,
        AuthorUsername author,
        CommentBody body,
        DateTime nowUtc)
    {
        var comment = new Comment(id, articleId, author, body, nowUtc);
        comment.AddDomainEvent(new CommentAddedDomainEvent(articleId.Value, id.Value, author.Value));

        return comment;
    }

    public bool BelongsTo(ArticleId articleId) => ArticleId == articleId;

    public ErrorOr<Success> EnsureCanBeDeletedBy(AuthorUsername requester) =>
        new OnlyTheAuthorCanDeleteTheComment(Author, requester).Check();

    /// <summary>
    /// Announces that this comment is gone. Called right before the repository removes it.
    /// </summary>
    public void Delete() => AddDomainEvent(new CommentDeletedDomainEvent(ArticleId.Value, Id.Value));
}
