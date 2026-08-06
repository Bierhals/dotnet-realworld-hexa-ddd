using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Api.Endpoints.Articles;
using Conduit.Articles.Application.Commands.FavoriteArticle;
using Conduit.Shared.Application.Cqrs;
using Conduit.Shared.Infrastructure.ApiEndpoints;
using Conduit.Shared.Infrastructure.ErrorHandling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Articles.Api.Endpoints.Favorites;

internal sealed class FavoriteArticleEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("", HandleAsync)
            .RequireAuthorization()
            .WithSummary("Favorite an article")
            .WithDescription("Favorite an article. Auth is required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#favorite-article\">Conduit Spec for favorite article endpoint</a>")
            .Produces<ArticleEnvelope>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<Results<Ok<ArticleEnvelope>, ProblemHttpResult>> HandleAsync(
        [Required][Description("Slug of the article that you want to favorite")] string slug,
        ICqrsMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new FavoriteArticleCommand { Slug = slug }, cancellationToken);
        if (result.IsError)
        {
            return result.Errors.ToProblemResult();
        }

        return await ArticleEnvelopeFactory.BuildAsync(slug, mediator, cancellationToken);
    }
}
