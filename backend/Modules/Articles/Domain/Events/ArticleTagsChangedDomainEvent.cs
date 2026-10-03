using System;
using System.Collections.Generic;
using Conduit.Shared.Domain;

namespace Conduit.Articles.Domain.Events;

/// <summary>
/// The tags an article started and stopped using - on publish, on edit, and on delete (which gives
/// up all of them). Raised only when at least one of the two lists is non-empty.
/// </summary>
public sealed record ArticleTagsChangedDomainEvent(
    Guid ArticleId,
    IReadOnlyList<string> Added,
    IReadOnlyList<string> Removed) : DomainEvent;
