using System.Collections.Generic;
using Conduit.Shared.Application.Cqrs;
using Conduit.Tags.Core.Application.Commands.RemoveArticleTags;
using Conduit.Tags.Core.Application.Commands.UpdateArticleTags;
using Conduit.Tags.Core.Application.Queries.TagCatalog;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Tags.Core.Application;

public static class TagsApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddTagsApplication(this IServiceCollection services)
    {
        services.AddCqrsMediator();

        services.AddScoped<TagReferenceCounter>();
        services.AddScoped<ICommandHandler<UpdateArticleTagsCommand>, UpdateArticleTagsHandler>();
        services.AddScoped<ICommandHandler<RemoveArticleTagsCommand>, RemoveArticleTagsHandler>();

        services.AddScoped<IQueryHandler<TagCatalogQuery, IReadOnlyCollection<string>>, TagCatalogHandler>();

        return services;
    }
}
