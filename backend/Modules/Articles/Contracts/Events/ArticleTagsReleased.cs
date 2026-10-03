using System;
using System.Collections.Generic;

namespace Conduit.Articles.Contracts.Events;

/// <summary>
/// An article stopped using these tags, either because an edit dropped them or because the article
/// itself is gone. Consumers that keep a tag catalog count one use less of each name.
/// </summary>
public sealed record ArticleTagsReleased(Guid ArticleId, IReadOnlyList<string> TagNames);
