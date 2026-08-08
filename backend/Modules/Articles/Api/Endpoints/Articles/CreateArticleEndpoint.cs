using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Api.Endpoints.Articles.Dtos;
using Conduit.Articles.Application.Commands.CreateArticle;
using Conduit.Articles.Application.Queries.ArticleDetails;
using Conduit.Shared.Application.Cqrs;
using Conduit.Shared.Infrastructure.ApiEndpoints;
using Conduit.Shared.Infrastructure.ErrorHandling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Conduit.Articles.Api.Endpoints.Articles;

internal sealed class CreateArticleEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("", HandleAsync)
            .RequireAuthorization()
            .WithSummary("Create an article")
            .WithDescription("Create an article. Auth is required<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#create-article\">Conduit Spec for create article endpoint</a>")
            .Produces<ArticleEnvelope>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status401Unauthorized)
            .ProducesValidationProblem(StatusCodes.Status409Conflict)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<Results<Created<ArticleEnvelope>, ProblemHttpResult>> HandleAsync(
        [Description("The article to create")] Request request,
        ICqrsMediator mediator,
        LinkGenerator linkGenerator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var command = new CreateArticleCommand
        {
            Title = request.Article.Title,
            Description = request.Article.Description,
            Body = request.Article.Body,
            TagList = request.Article.TagList,
        };

        var slug = await mediator.Send(command, cancellationToken);
        if (slug.IsError)
        {
            return slug.Errors.ToProblemResult();
        }

        var article = await mediator.Send(new ArticleDetailsQuery { Slug = slug.Value }, cancellationToken);
        if (article.IsError)
        {
            return article.Errors.ToProblemResult();
        }

        var location = linkGenerator.GetPathByName(httpContext, GetArticleEndpoint.Name, new { slug = slug.Value });

        return TypedResults.Created(location, new ArticleEnvelope(ArticleEnvelopeFactory.Create(article.Value)));
    }

    public sealed record Request
    {
        [Required]
        public required Data Article { get; init; }

        public sealed record Data
        {
            [Required]
            public required string Title { get; init; }

            [Required]
            public required string Description { get; init; }

            [Required]
            public required string Body { get; init; }

            public string[]? TagList { get; init; }
        }
    }
}
