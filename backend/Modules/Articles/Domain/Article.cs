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
/// An article and the tags it carries. Comments and favorites are their own aggregates: they have
/// their own identity and their own rules, and refer back to the article by id.
/// </summary>
public sealed class Article : AggregateRoot<ArticleId>
{
    private readonly List<TagName> _tags = [];

    public ArticleSlug Slug { get; private set; }
    public ArticleTitle Title { get; private set; }
    public ArticleDescription Description { get; private set; }
    public ArticleBody Body { get; private set; }
    public Username Author { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public int Revision { get; private set; }

    public IReadOnlyCollection<TagName> Tags => _tags.AsReadOnly();

#pragma warning disable CS8618 // Non-nullable properties are populated by EF Core when materializing.
    private Article() { } // for EF Core
#pragma warning restore CS8618

    private Article(
        ArticleId id,
        Username author,
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
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;
        Revision = 1;
    }

    public static Article Publish(
        Username author,
        ArticleTitle title,
        ArticleDescription description,
        ArticleBody body,
        IReadOnlyCollection<TagName> tagNames,
        DateTime nowUtc)
    {
        var article = new Article(ArticleId.New(), author, title, description, body, nowUtc);

        article._tags.AddRange(tagNames.Distinct());

        article.AddDomainEvent(new ArticlePublishedDomainEvent(article.State()));

        return article;
    }

    /// <summary>
    /// Applies the fields that were supplied - a <c>null</c> means "leave unchanged". An edit that
    /// changes nothing leaves the article, its revision and its events untouched.
    /// </summary>
    public ErrorOr<Success> Edit(
        Username editor,
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

        var tagsChanged = tagNames is not null && ApplyTags(tagNames);

        if (!changed && !tagsChanged)
        {
            return Result.Success;
        }

        UpdatedAtUtc = nowUtc;
        Revision++;
        AddDomainEvent(new ArticleEditedDomainEvent(State()));

        return Result.Success;
    }

    /// <summary>
    /// Checks that the requester may delete the article and announces the deletion. Removing the
    /// aggregate from the store stays with the caller.
    /// </summary>
    public ErrorOr<Success> Delete(Username requester) =>
        new OnlyTheAuthorCanChangeTheArticle(Author, requester).Check()
            .Then(_ =>
            {
                Revision++;
                AddDomainEvent(new ArticleDeletedDomainEvent(Id.Value, Revision));

                return Result.Success;
            });

    private ArticleState State() => new(
        Id.Value,
        Revision,
        Slug.Value,
        Title.Value,
        Description.Value,
        Author.Value,
        [.. _tags.Select(tagName => tagName.Value)],
        CreatedAtUtc,
        UpdatedAtUtc);

    private bool ApplyTags(IReadOnlyCollection<TagName> tagNames)
    {
        var wanted = tagNames.Distinct().ToList();

        var added = wanted.Where(tagName => !_tags.Contains(tagName)).ToList();
        var removed = _tags.Where(tagName => !wanted.Contains(tagName)).ToList();

        _tags.RemoveAll(removed.Contains);
        _tags.AddRange(added);

        return added.Count > 0 || removed.Count > 0;
    }
}
