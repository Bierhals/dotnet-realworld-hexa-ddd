using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application.Commands.EditArticle;
using Conduit.Shared.Application.Cqrs;
using Conduit.Shared.Infrastructure.ApiEndpoints;
using Conduit.Shared.Infrastructure.ErrorHandling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Articles.Api.Endpoints.Articles;

internal sealed class UpdateArticleEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("{slug}", HandleAsync)
            .RequireAuthorization()
            .WithSummary("Update an article")
            .WithDescription("Update an article. Auth is required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#update-article\">Conduit Spec for update article endpoint</a>")
            .Produces<ArticleEnvelope>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status403Forbidden)
            .ProducesValidationProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<Results<Ok<ArticleEnvelope>, ProblemHttpResult>> HandleAsync(
        [Required][Description("The slug of the article to update")] string slug,
        [Description("The article to update")] UpdateArticleRequest request,
        ICqrsMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new EditArticleCommand
        {
            Slug = slug,
            Title = request.Article.Title,
            Description = request.Article.Description,
            Body = request.Article.Body,
            TagList = request.Article.TagList,
        };

        var updatedSlug = await mediator.Send(command, cancellationToken);
        if (updatedSlug.IsError)
        {
            return updatedSlug.Errors.ToProblemResult();
        }

        return await ArticleEnvelopeFactory.BuildAsync(updatedSlug.Value, mediator, cancellationToken);
    }
}
