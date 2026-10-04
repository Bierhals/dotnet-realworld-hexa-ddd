using System;
using System.Collections.Generic;

namespace Conduit.Articles.Domain.Events;

/// <summary>
/// What an article looked like when an event about it was raised. The events carry the state
/// itself and not what changed, so that whoever reacts to them never has to ask the article back
/// and can apply a redelivered event without harm. The body is left out on purpose: nobody who
/// reacts to an article has needed it, and it is by far the largest part.
/// </summary>
/// <param name="Revision">
/// Counts the changes of the article, starting at 1. It lets a receiver tell a stale event from a
/// current one, because delivery is neither exactly-once nor ordered.
/// </param>
public sealed record ArticleState(
    Guid ArticleId,
    int Revision,
    string Slug,
    string Title,
    string Description,
    string Author,
    IReadOnlyList<string> Tags,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
