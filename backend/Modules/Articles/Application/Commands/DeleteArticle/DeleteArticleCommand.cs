using Conduit.Shared.Application.Cqrs;

namespace Conduit.Articles.Application.Commands.DeleteArticle;

public sealed record DeleteArticleCommand : ICommand
{
    public required string Slug { get; init; }
}
