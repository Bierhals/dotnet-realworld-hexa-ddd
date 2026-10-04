using System;
using System.Collections.Generic;
using System.Linq;
using Conduit.Shared.Domain;

namespace Conduit.Tags.Core.Domain;

/// <summary>
/// Which tags one article currently uses, as far as the Articles module has told this module. The
/// catalog counts how many articles use each tag, and it can only do that reliably if it knows
/// what each article used before: events about an article carry its state and not what changed,
/// and they may arrive twice or out of order.
/// </summary>
public sealed class ArticleTagUsage : AggregateRoot<Guid>
{
    private List<string> _tagNames = [];

    /// <summary>
    /// The revision of the article the usage is up to date with. Zero until the first event.
    /// </summary>
    public int Revision { get; private set; }

    public bool IsDeleted { get; private set; }

    public IReadOnlyCollection<TagName> Tags => [.. _tagNames.Select(TagName.Rehydrate)];

    private ArticleTagUsage() { } // for EF Core

    private ArticleTagUsage(Guid articleId) : base(articleId)
    {
    }

    public static ArticleTagUsage Start(Guid articleId) => new(articleId);

    /// <summary>
    /// Takes over the tags of a newer state of the article. Returns <c>null</c> when the state is
    /// not newer than the one already known, which is what a redelivered or late event looks like.
    /// </summary>
    public TagUsageChange? Update(int revision, IReadOnlyCollection<TagName> tagNames)
    {
        if (revision <= Revision)
        {
            return null;
        }

        var wanted = tagNames.Distinct().ToList();
        var current = Tags;

        var added = wanted.Where(tagName => !current.Contains(tagName)).ToList();
        var removed = current.Where(tagName => !wanted.Contains(tagName)).ToList();

        _tagNames = [.. wanted.Select(tagName => tagName.Value)];
        Revision = revision;

        return new TagUsageChange(added, removed);
    }

    /// <summary>
    /// Gives up every tag of the article for good. Returns <c>null</c> when the deletion is not
    /// newer than what is known. A deleted article stays on record so that an older event that
    /// shows up late cannot bring it back.
    /// </summary>
    public TagUsageChange? Delete(int revision)
    {
        if (revision <= Revision)
        {
            return null;
        }

        var removed = Tags;

        _tagNames = [];
        Revision = revision;
        IsDeleted = true;

        return new TagUsageChange([], removed);
    }
}
