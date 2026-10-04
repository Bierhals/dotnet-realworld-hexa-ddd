using System;
using System.Collections.Generic;
using Conduit.Shared.Application.Cqrs;

namespace Conduit.Tags.Core.Application.Commands.UpdateArticleTags;

/// <summary>
/// An article was published or edited and now carries exactly these tags.
/// </summary>
public sealed record UpdateArticleTagsCommand : ICommand
{
    public required Guid ArticleId { get; init; }

    public required int Revision { get; init; }

    public required IReadOnlyCollection<string> TagNames { get; init; }
}
