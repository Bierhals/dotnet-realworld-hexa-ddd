using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Contracts.Events;
using Conduit.Tags.Contracts.Catalog;
using ErrorOr;

namespace Conduit.Tags.Core.Infrastructure.EventHandling;

/// <summary>
/// Keeps the tag catalog in step with the articles that use its tags. A driving adapter like an
/// endpoint: it translates another module's integration event into a call against this module's
/// own application layer.
/// </summary>
/// <remarks>
/// Delivery is at-least-once and the reference count is not idempotent by nature, so each handler
/// stays a single transaction and relies on Wolverine's durable inbox to drop a redelivery of a
/// message it already handled.
/// </remarks>
public sealed class ArticleTagsHandler(ITagCatalogService tagCatalog)
{
    public async Task Handle(ArticleTagsReferenced message, CancellationToken cancellationToken) =>
        ThrowIfFailed(await tagCatalog.ReferenceTagsAsync(message.TagNames, cancellationToken));

    public async Task Handle(ArticleTagsReleased message, CancellationToken cancellationToken) =>
        ThrowIfFailed(await tagCatalog.ReleaseTagsAsync(message.TagNames, cancellationToken));

    // The article module validated the names already, so a failure here is not something the sender
    // can fix: throwing hands the message to Wolverine's retry and dead-letter handling.
    private static void ThrowIfFailed(ErrorOr<Success> result)
    {
        if (result.IsError)
        {
            throw new InvalidOperationException(
                $"The tag catalog rejected an article's tags: {result.FirstError.Description}");
        }
    }
}
