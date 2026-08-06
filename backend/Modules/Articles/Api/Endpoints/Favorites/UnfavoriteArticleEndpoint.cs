using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Api.Endpoints.Articles;
using Conduit.Articles.Application.Commands.UnfavoriteArticle;
using Conduit.Shared.Application.Cqrs;
using Conduit.Shared.Infrastructure.ApiEndpoints;
using Conduit.Shared.Infrastructure.ErrorHandling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Articles.Api.Endpoints.Favorites;

internal sealed class UnfavoriteArticleEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("", HandleAsync)
            .RequireAuthorization()
            .WithSummary("Unfavorite an article")
            .WithDescription("Unfavorite an article. Auth is required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#unfavorite-article\">Conduit Spec for unfavorite article endpoint</a>")
            .Produces<ArticleEnvelope>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<Results<Ok<ArticleEnvelope>, ProblemHttpResult>> HandleAsync(
        [Required][Description("Slug of the article that you want to unfavorite")] string slug,
        ICqrsMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UnfavoriteArticleCommand { Slug = slug }, cancellationToken);
        if (result.IsError)
        {
            return result.Errors.ToProblemResult();
        }

        return await ArticleEnvelopeFactory.BuildAsync(slug, mediator, cancellationToken);
    }
}
