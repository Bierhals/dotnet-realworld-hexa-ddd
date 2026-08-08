using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Articles.Application.Queries.CommentList;

public sealed class CommentListHandler(
    IArticlesReadRepository articlesReadRepository,
    IProfileReader profileReader,
    ICurrentUserAccessor currentUserAccessor)
    : IQueryHandler<CommentListQuery, IReadOnlyCollection<CommentReadModel>>
{
    public async Task<ErrorOr<IReadOnlyCollection<CommentReadModel>>> Handle(
        CommentListQuery query,
        CancellationToken cancellationToken)
    {
        var comments = await articlesReadRepository.GetCommentsAsync(query.Slug, cancellationToken);

        return await comments.ToErrorOr(Error.NotFound("Article.NotFound", "The article does not exist."))
            .ThenAsync(async c => await AuthorProfileResolver.ToReadModelsAsync(
                c,
                profileReader,
                currentUserAccessor.GetCurrentUsername(),
                cancellationToken));
    }
}
