using System;
using System.Collections.Generic;

namespace Conduit.Articles.Contracts.Events;

/// <summary>
/// The public state of an article at the moment an event about it was raised. Receivers keep what
/// they need of it in a projection of their own instead of asking the Articles module later. The
/// body is not part of it.
/// </summary>
/// <param name="Revision">
/// Counts the changes of the article, starting at 1. Events are delivered at least once and in no
/// particular order, so a receiver ignores any event that is not newer than the state it already
/// holds.
/// </param>
public sealed record ArticleSnapshot(
    Guid Id,
    int Revision,
    string Slug,
    string Title,
    string Description,
    string Author,
    IReadOnlyList<string> Tags,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
