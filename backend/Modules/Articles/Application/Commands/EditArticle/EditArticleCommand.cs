using Conduit.Shared.Application.Cqrs;
using Conduit.Shared.Application.Optional;

namespace Conduit.Articles.Application.Commands.EditArticle;

/// <summary>
/// Every field except the slug is optional; an unspecified field leaves that part of the article
/// as it is. Returns the article's slug, which changes along with its title.
/// </summary>
public sealed record EditArticleCommand : ICommand<string>
{
    public required string Slug { get; init; }

    public Optional<string> Title { get; init; }

    public Optional<string> Description { get; init; }

    public Optional<string> Body { get; init; }

    public Optional<string[]> TagList { get; init; }
}
