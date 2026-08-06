using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application.Cqrs;
using Conduit.Shared.Infrastructure.ApiEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Articles.Api.Endpoints.Articles;

internal sealed class GetArticleEndpoint : IEndpoint
{
    public const string Name = nameof(GetArticleEndpoint);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("{slug}", HandleAsync)
            .AllowAnonymous()
            .WithName(Name)
            .WithSummary("Get an article")
            .WithDescription("Get an article. Auth not required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#get-article\">Conduit Spec for get article endpoint</a>")
            .Produces<ArticleEnvelope>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
    }

    private static Task<Results<Ok<ArticleEnvelope>, ProblemHttpResult>> HandleAsync(
        [Required][Description("Slug of the article to get")] string slug,
        ICqrsMediator mediator,
        CancellationToken cancellationToken) =>
        ArticleEnvelopeFactory.BuildAsync(slug, mediator, cancellationToken);
}
