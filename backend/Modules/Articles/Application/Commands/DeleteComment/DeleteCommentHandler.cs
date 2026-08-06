using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Domain;
using Conduit.Articles.Domain.ValueObjects;
using Conduit.Shared.Application;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Articles.Application.Commands.DeleteComment;

public sealed class DeleteCommentHandler(
    IArticlesRepository articlesRepository,
    ICommentsRepository commentsRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserAccessor currentUserAccessor) : ICommandHandler<DeleteCommentCommand>
{
    public async Task<ErrorOr<Success>> Handle(DeleteCommentCommand command, CancellationToken cancellationToken)
    {
        var requester = CurrentAuthor.Resolve(currentUserAccessor);
        if (requester.IsError)
        {
            return requester.Errors;
        }

        var articleId = await articlesRepository.GetIdBySlugAsync(
            ArticleSlug.Rehydrate(command.Slug),
            cancellationToken);
        if (articleId is null)
        {
            return Error.NotFound("Article.NotFound", "The article does not exist.");
        }

        var comment = await commentsRepository.GetAsync(CommentId.From(command.CommentId), cancellationToken);

        // Comment numbers are unique across all articles, so a comment that belongs to a different
        // article is simply not found under this one.
        if (comment is null || !comment.BelongsTo(articleId.Value))
        {
            return Error.NotFound("Comment.NotFound", "The comment does not exist.");
        }

        var check = comment.EnsureCanBeDeletedBy(requester.Value);
        if (check.IsError)
        {
            return check.Errors;
        }

        comment.Delete();
        commentsRepository.Remove(comment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
