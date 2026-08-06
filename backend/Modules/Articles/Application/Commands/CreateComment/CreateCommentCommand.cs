using Conduit.Shared.Application.Cqrs;

namespace Conduit.Articles.Application.Commands.CreateComment;

public sealed record CreateCommentCommand : ICommand<CommentReadModel>
{
    public required string Slug { get; init; }

    public required string Body { get; init; }
}
