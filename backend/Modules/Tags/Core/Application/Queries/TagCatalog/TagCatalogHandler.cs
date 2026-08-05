using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Tags.Core.Application.Queries.TagCatalog;

public sealed class TagCatalogHandler(ITagsReadRepository tagsReadRepository)
    : IQueryHandler<TagCatalogQuery, IReadOnlyCollection<string>>
{
    public async Task<ErrorOr<IReadOnlyCollection<string>>> Handle(TagCatalogQuery query, CancellationToken cancellationToken) =>
        ErrorOrFactory.From(await tagsReadRepository.GetTagNamesAsync(cancellationToken));
}
