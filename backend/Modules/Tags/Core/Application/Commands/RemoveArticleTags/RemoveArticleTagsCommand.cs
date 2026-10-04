using System;
using Conduit.Shared.Application.Cqrs;

namespace Conduit.Tags.Core.Application.Commands.RemoveArticleTags;

/// <summary>
/// An article was deleted, so it no longer uses any tag.
/// </summary>
public sealed record RemoveArticleTagsCommand : ICommand
{
    public required Guid ArticleId { get; init; }

    public required int Revision { get; init; }
}
