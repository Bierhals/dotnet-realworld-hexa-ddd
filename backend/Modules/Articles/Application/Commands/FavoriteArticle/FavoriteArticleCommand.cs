using Conduit.Shared.Application.Cqrs;

namespace Conduit.Articles.Application.Commands.FavoriteArticle;

public sealed record FavoriteArticleCommand : ICommand
{
    public required string Slug { get; init; }
}
