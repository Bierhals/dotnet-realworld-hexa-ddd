using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application.Commands.DeleteArticle;
using Conduit.Shared.Application.Cqrs;
using Conduit.Shared.Infrastructure.ApiEndpoints;
using Conduit.Shared.Infrastructure.ErrorHandling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Articles.Api.Endpoints.Articles;

internal sealed class DeleteArticleEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("{slug}", HandleAsync)
            .RequireAuthorization()
            .WithSummary("Delete an article")
            .WithDescription("Delete an article. Auth is required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#delete-article\">Conduit Spec for delete article endpoint</a>")
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status403Forbidden)
            .ProducesValidationProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> HandleAsync(
        [Required][Description("The slug of the article to delete")] string slug,
        ICqrsMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteArticleCommand { Slug = slug }, cancellationToken);

        return result.IsError
            ? result.Errors.ToProblemResult()
            : TypedResults.NoContent();
    }
}
