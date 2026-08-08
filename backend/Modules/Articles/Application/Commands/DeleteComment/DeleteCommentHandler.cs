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
    public Task<ErrorOr<Success>> Handle(DeleteCommentCommand command, CancellationToken cancellationToken) =>
        CurrentUser.Resolve(currentUserAccessor)
            .ThenAsync(async requester =>
            {
                var articleId = await articlesRepository.GetIdBySlugAsync(
                    ArticleSlug.Rehydrate(command.Slug),
                    cancellationToken);

                return await articleId.ToErrorOr(Error.NotFound("Article.NotFound", "The article does not exist."))
                    .ThenAsync(async id =>
                    {
                        var comment = await commentsRepository.GetAsync(CommentId.From(command.CommentId), cancellationToken);
                        var notFound = Error.NotFound("Comment.NotFound", "The comment does not exist.");

                        // Comment numbers are unique across all articles, so a comment that belongs to a different
                        // article is simply not found under this one.
                        return await comment.ToErrorOr(notFound)
                            .Then<Comment>(c => c.BelongsTo(id) ? c : notFound)
                            .Then(c => c.EnsureCanBeDeletedBy(requester).Then(_ => c))
                            .ThenDoAsync(async c =>
                            {
                                c.Delete();
                                commentsRepository.Remove(c);
                                await unitOfWork.SaveChangesAsync(cancellationToken);
                            })
                            .Then(_ => Result.Success);
                    });
            });
}
