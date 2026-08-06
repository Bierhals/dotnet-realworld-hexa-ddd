using Conduit.Shared.Application.Cqrs;

namespace Conduit.Articles.Application.Commands.UnfavoriteArticle;

public sealed record UnfavoriteArticleCommand : ICommand
{
    public required string Slug { get; init; }
}
