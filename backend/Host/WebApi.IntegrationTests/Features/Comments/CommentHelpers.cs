using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Conduit.Host.WebApi.Features.Comments;
using Conduit.Host.WebApi.Infrastructure.Errors;
using Conduit.Host.WebApi.IntegrationTests.Features.Users;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Host.WebApi.IntegrationTests.Features.Comments;

public static class CommentHelpers
{
    /// <summary>
    /// creates an article comment based on the given Create command.
    /// Creates a default user if parameter userName is empty.
    /// </summary>
    /// <param name="fixture"></param>
    /// <param name="command"></param>
    /// <param name="userName"></param>
    /// <returns></returns>
    public static async Task<Domain.Comment> CreateComment(
        SliceFixture fixture,
        Create.Command command,
        string userName
    )
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            userName = await UserHelpers.CreateDefaultUser(fixture);
        }

        var dbContext = fixture.GetDbContext();
        var currentAccessor = new StubCurrentUserAccessor(userName);

        var commentCreateHandler = new Create.Handler(
            dbContext,
            currentAccessor,
            fixture.ProfileQueryService
        );
        var created = await commentCreateHandler.Handle(
            command,
            new System.Threading.CancellationToken()
        );

        var dbArticleWithComments = await fixture.ExecuteDbContextAsync(db =>
            db.Articles.Include(a => a.Comments)
                .Where(a => a.Slug == command.Slug)
                .SingleOrDefaultAsync()
        );

        if (dbArticleWithComments is null)
        {
            throw new RestException(HttpStatusCode.NotFound, new { Article = Constants.NOT_FOUND });
        }

        var dbComment = dbArticleWithComments.Comments.FirstOrDefault(c =>
            c.ArticleId == dbArticleWithComments.ArticleId && c.AuthorUsername == userName
        );

        if (dbComment is null)
        {
            throw new RestException(HttpStatusCode.NotFound, new { Article = Constants.NOT_FOUND });
        }

        return dbComment;
    }
}
