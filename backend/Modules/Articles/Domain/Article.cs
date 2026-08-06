using System;
using System.Collections.Generic;
using System.Linq;
using Conduit.Articles.Domain.Events;
using Conduit.Articles.Domain.Rules;
using Conduit.Articles.Domain.ValueObjects;
using Conduit.Shared.Domain;
using ErrorOr;

namespace Conduit.Articles.Domain;

/// <summary>
/// An article together with everything that only exists as part of it: its comments, its favorites
/// and the tags it uses. Comments and favorites share the article's lifecycle and are therefore
/// reached exclusively through this aggregate root.
/// </summary>
public sealed class Article : AggregateRoot<ArticleId>
{
    private readonly List<Comment> _comments = [];
    private readonly List<ArticleTag> _tags = [];
    private readonly List<ArticleFavorite> _favorites = [];

    public ArticleSlug Slug { get; private set; }
    public ArticleTitle Title { get; private set; }
    public ArticleDescription Description { get; private set; }
    public ArticleBody Body { get; private set; }
    public AuthorUsername Author { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();

    public IReadOnlyCollection<TagName> TagNames => [.. _tags.Select(tag => tag.TagName)];

    public int FavoritesCount => _favorites.Count;

#pragma warning disable CS8618 // Non-nullable properties are populated by EF Core when materializing.
    private Article() { } // for EF Core
#pragma warning restore CS8618

    private Article(
        ArticleId id,
        AuthorUsername author,
        ArticleTitle title,
        ArticleDescription description,
        ArticleBody body,
        DateTime createdAtUtc) : base(id)
    {
        Author = author;
        Title = title;
        Description = description;
        Body = body;
        Slug = ArticleSlug.FromTitle(title);
        CreatedAt = createdAtUtc;
        UpdatedAt = createdAtUtc;
    }

    public static Article Publish(
        AuthorUsername author,
        ArticleTitle title,
        ArticleDescription description,
        ArticleBody body,
        IReadOnlyCollection<TagName> tagNames,
        DateTime nowUtc)
    {
        var article = new Article(ArticleId.New(), author, title, description, body, nowUtc);

        foreach (var tagName in tagNames.Distinct())
        {
            article._tags.Add(new ArticleTag(article.Id, tagName));
        }

        article.AddDomainEvent(new ArticlePublishedDomainEvent(article.Id.Value, article.Slug.Value, author.Value));

        return article;
    }

    /// <summary>
    /// Applies the fields that were supplied - a <c>null</c> means "leave unchanged" - and reports
    /// which tags the article started and stopped using, so the caller can update the tag catalog.
    /// </summary>
    public ErrorOr<TagChanges> Edit(
        AuthorUsername editor,
        ArticleTitle? title,
        ArticleDescription? description,
        ArticleBody? body,
        IReadOnlyCollection<TagName>? tagNames,
        DateTime nowUtc)
    {
        var check = new OnlyTheAuthorCanChangeTheArticle(Author, editor).Check();
        if (check.IsError)
        {
            return check.Errors;
        }

        var changed = false;

        if (title is not null && title != Title)
        {
            Title = title;
            Slug = ArticleSlug.FromTitle(title);
            changed = true;
        }

        if (description is not null && description != Description)
        {
            Description = description;
            changed = true;
        }

        if (body is not null && body != Body)
        {
            Body = body;
            changed = true;
        }

        var tagChanges = tagNames is null ? TagChanges.None : ApplyTags(tagNames);

        if (!changed && tagChanges.Added.Count == 0 && tagChanges.Removed.Count == 0)
        {
            return tagChanges;
        }

        UpdatedAt = nowUtc;
        AddDomainEvent(new ArticleEditedDomainEvent(Id.Value, Slug.Value));

        return tagChanges;
    }

    public ErrorOr<Success> EnsureCanBeDeletedBy(AuthorUsername requester) =>
        new OnlyTheAuthorCanChangeTheArticle(Author, requester).Check();

    /// <summary>
    /// Favoriting an article the account already favorited is a no-op, so that a repeated request
    /// does not double-count.
    /// </summary>
    public void Favorite(AuthorUsername username)
    {
        if (IsFavoritedBy(username))
        {
            return;
        }

        _favorites.Add(new ArticleFavorite(Id, username));
        AddDomainEvent(new ArticleFavoritedDomainEvent(Id.Value, username.Value));
    }

    public void Unfavorite(AuthorUsername username)
    {
        var favorite = _favorites.Find(x => x.Username == username);
        if (favorite is null)
        {
            return;
        }

        _favorites.Remove(favorite);
        AddDomainEvent(new ArticleUnfavoritedDomainEvent(Id.Value, username.Value));
    }

    public bool IsFavoritedBy(AuthorUsername username) => _favorites.Exists(x => x.Username == username);

    public Comment AddComment(CommentId id, AuthorUsername author, CommentBody body, DateTime nowUtc)
    {
        var comment = new Comment(id, Id, author, body, nowUtc);
        _comments.Add(comment);
        AddDomainEvent(new CommentAddedDomainEvent(Id.Value, id.Value, author.Value));

        return comment;
    }

    public ErrorOr<Success> DeleteComment(CommentId id, AuthorUsername requester)
    {
        var comment = _comments.Find(x => x.Id == id);
        if (comment is null)
        {
            return Error.NotFound("Comment.NotFound", "The comment does not exist.");
        }

        var check = new OnlyTheAuthorCanDeleteTheComment(comment.Author, requester).Check();
        if (check.IsError)
        {
            return check.Errors;
        }

        _comments.Remove(comment);
        AddDomainEvent(new CommentDeletedDomainEvent(Id.Value, id.Value));

        return Result.Success;
    }

    private TagChanges ApplyTags(IReadOnlyCollection<TagName> tagNames)
    {
        var wanted = tagNames.Distinct().ToList();
        var current = _tags.ConvertAll(tag => tag.TagName);

        var added = wanted.Where(tagName => !current.Contains(tagName)).ToList();
        var removed = current.Where(tagName => !wanted.Contains(tagName)).ToList();

        _tags.RemoveAll(tag => removed.Contains(tag.TagName));
        foreach (var tagName in added)
        {
            _tags.Add(new ArticleTag(Id, tagName));
        }

        return new TagChanges(added, removed);
    }
}
