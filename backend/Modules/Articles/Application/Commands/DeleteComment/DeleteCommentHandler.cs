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

        var article = await articlesRepository.GetBySlugAsync(
            ArticleSlug.Rehydrate(command.Slug),
            cancellationToken);
        if (article is null)
        {
            return Error.NotFound("Article.NotFound", "The article does not exist.");
        }

        var deletion = article.DeleteComment(CommentId.From(command.CommentId), requester.Value);
        if (deletion.IsError)
        {
            return deletion.Errors;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
