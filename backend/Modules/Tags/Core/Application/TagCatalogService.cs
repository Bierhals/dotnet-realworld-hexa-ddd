using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application.Cqrs;
using Conduit.Tags.Contracts.Catalog;
using Conduit.Tags.Core.Application.Commands.ReferenceTags;
using Conduit.Tags.Core.Application.Commands.ReleaseTags;
using ErrorOr;

namespace Conduit.Tags.Core.Application;

internal sealed class TagCatalogService(ICqrsMediator mediator) : ITagCatalogService
{
    public Task<ErrorOr<Success>> ReferenceTagsAsync(IReadOnlyCollection<string> tagNames, CancellationToken cancellationToken = default) =>
        mediator.Send(new ReferenceTagsCommand { TagNames = tagNames }, cancellationToken);

    public Task<ErrorOr<Success>> ReleaseTagsAsync(IReadOnlyCollection<string> tagNames, CancellationToken cancellationToken = default) =>
        mediator.Send(new ReleaseTagsCommand { TagNames = tagNames }, cancellationToken);
}
