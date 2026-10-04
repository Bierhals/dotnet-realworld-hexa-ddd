using System;

namespace Conduit.Articles.Contracts.Events;

/// <summary>
/// The article is gone. The revision is newer than any state announced for it before.
/// </summary>
public sealed record ArticleDeleted(Guid ArticleId, int Revision);
