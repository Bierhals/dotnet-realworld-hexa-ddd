using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Domain;
using Conduit.Articles.Domain.ValueObjects;
using Conduit.Shared.Application;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Articles.Application.Commands.CreateComment;

public sealed class CreateCommentHandler(
    IArticlesRepository articlesRepository,
    ICommentsRepository commentsRepository,
    IUnitOfWork unitOfWork,
    ICommentNumberGenerator commentNumberGenerator,
    IProfileReader profileReader,
    ICurrentUserAccessor currentUserAccessor) : ICommandHandler<CreateCommentCommand, CommentReadModel>
{
    public async Task<ErrorOr<CommentReadModel>> Handle(
        CreateCommentCommand command,
        CancellationToken cancellationToken)
    {
        var author = CurrentAuthor.Resolve(currentUserAccessor);
        if (author.IsError)
        {
            return author.Errors;
        }

        var body = CommentBody.Create(command.Body);
        if (body.IsError)
        {
            return body.Errors;
        }

        // Only the article's identity is needed here - a comment is its own aggregate and never
        // goes through the article.
        var articleId = await articlesRepository.GetIdBySlugAsync(
            ArticleSlug.Rehydrate(command.Slug),
            cancellationToken);
        if (articleId is null)
        {
            return Error.NotFound("Article.NotFound", "The article does not exist.");
        }

        var commentId = await commentNumberGenerator.NextAsync(cancellationToken);
        var comment = Comment.Post(commentId, articleId.Value, author.Value, body.Value, DateTime.UtcNow);

        commentsRepository.Add(comment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var readModels = await AuthorProfileResolver.ToReadModelsAsync(
            [new CommentProjection(
                comment.Id.Value,
                comment.Body.Value,
                comment.CreatedAtUtc,
                comment.UpdatedAtUtc,
                comment.Author.Value)],
            profileReader,
            author.Value.Value,
            cancellationToken);

        return readModels.First();
    }
}
