using System.Collections.Generic;
using Conduit.Shared.Application.Cqrs;

namespace Conduit.Articles.Application.Commands.EditArticle;

/// <summary>
/// Every field except the slug is optional; a <c>null</c> leaves that part of the article as it is.
/// Returns the article's slug, which changes along with its title.
/// </summary>
public sealed record EditArticleCommand : ICommand<string>
{
    public required string Slug { get; init; }

    public string? Title { get; init; }

    public string? Description { get; init; }

    public string? Body { get; init; }

    public IReadOnlyCollection<string>? TagList { get; init; }
}
