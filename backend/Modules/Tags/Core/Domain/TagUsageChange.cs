using System.Collections.Generic;

namespace Conduit.Tags.Core.Domain;

/// <summary>
/// The tags an article started and stopped using, as worked out against what the catalog already
/// knows about that article.
/// </summary>
public sealed record TagUsageChange(IReadOnlyCollection<TagName> Added, IReadOnlyCollection<TagName> Removed);
