using System;
using Conduit.Articles.Domain.ValueObjects;
using Conduit.Shared.Domain;

namespace Conduit.Articles.Domain;

/// <summary>
/// A comment on an article. Part of the <see cref="Article"/> aggregate - it is always created and
/// removed through the article it belongs to, never on its own.
/// </summary>
public sealed class Comment : Entity<CommentId>
{
    public ArticleId ArticleId { get; private set; }
    public AuthorUsername Author { get; private set; }
    public CommentBody Body { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

#pragma warning disable CS8618 // Non-nullable properties are populated by EF Core when materializing.
    // for EF Core. Entity<TId>'s parameterless constructor is internal to Conduit.Shared.Domain,
    // so the id has to be passed here; EF Core overwrites it when it materializes the entity.
    private Comment() : base(default) { }
#pragma warning restore CS8618

    internal Comment(CommentId id, ArticleId articleId, AuthorUsername author, CommentBody body, DateTime createdAtUtc)
        : base(id)
    {
        ArticleId = articleId;
        Author = author;
        Body = body;
        CreatedAt = createdAtUtc;
        UpdatedAt = createdAtUtc;
    }
}
