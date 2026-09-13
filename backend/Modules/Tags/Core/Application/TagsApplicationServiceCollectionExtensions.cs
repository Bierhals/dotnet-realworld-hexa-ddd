using System.Collections.Generic;
using Conduit.Shared.Application.Cqrs;
using Conduit.Tags.Contracts.Catalog;
using Conduit.Tags.Core.Application.Commands.ReferenceTags;
using Conduit.Tags.Core.Application.Commands.ReleaseTags;
using Conduit.Tags.Core.Application.Queries.TagCatalog;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Tags.Core.Application;

public static class TagsApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddTagsApplication(this IServiceCollection services)
    {
        services.AddCqrsMediator();

        services.AddScoped<ICommandHandler<ReferenceTagsCommand>, ReferenceTagsHandler>();
        services.AddScoped<ICommandHandler<ReleaseTagsCommand>, ReleaseTagsHandler>();

        services.AddScoped<IQueryHandler<TagCatalogQuery, IReadOnlyCollection<string>>, TagCatalogHandler>();

        services.AddScoped<ITagCatalogService, TagCatalogService>();

        return services;
    }
}
