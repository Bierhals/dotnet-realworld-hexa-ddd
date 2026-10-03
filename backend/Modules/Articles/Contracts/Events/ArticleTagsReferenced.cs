using System;
using System.Collections.Generic;

namespace Conduit.Articles.Contracts.Events;

/// <summary>
/// An article started using these tags. Consumers that keep a tag catalog count one more use of
/// each name.
/// </summary>
public sealed record ArticleTagsReferenced(Guid ArticleId, IReadOnlyList<string> TagNames);
