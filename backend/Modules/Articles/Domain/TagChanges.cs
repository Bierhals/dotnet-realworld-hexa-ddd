using System.Collections.Generic;
using Conduit.Articles.Domain.ValueObjects;

namespace Conduit.Articles.Domain;

/// <summary>
/// The tags an article started and stopped using during an edit. The aggregate reports them
/// instead of acting on them, so that the use case can announce them to the tag catalog - which
/// another module owns and which the aggregate must not reach into.
/// </summary>
public sealed record TagChanges(IReadOnlyCollection<TagName> Added, IReadOnlyCollection<TagName> Removed)
{
    public static TagChanges None { get; } = new([], []);
}
