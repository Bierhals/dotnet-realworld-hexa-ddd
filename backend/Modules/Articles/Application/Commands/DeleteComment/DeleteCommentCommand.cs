using Conduit.Shared.Application.Cqrs;

namespace Conduit.Articles.Application.Commands.DeleteComment;

public sealed record DeleteCommentCommand : ICommand
{
    public required string Slug { get; init; }

    public required int CommentId { get; init; }
}
