using System.Collections.Generic;
using Conduit.Shared.Application.Cqrs;

namespace Conduit.Articles.Application.Commands.CreateArticle;

/// <summary>
/// Returns the slug of the new article, which is how the API addresses it from then on.
/// </summary>
public sealed record CreateArticleCommand : ICommand<string>
{
    public required string Title { get; init; }

    public required string Description { get; init; }

    public required string Body { get; init; }

    public IReadOnlyCollection<string>? TagList { get; init; }
}
