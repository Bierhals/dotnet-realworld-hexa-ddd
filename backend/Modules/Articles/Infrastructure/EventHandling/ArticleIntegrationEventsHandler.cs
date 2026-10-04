using Conduit.Articles.Contracts.Events;
using Conduit.Articles.Domain.Events;

namespace Conduit.Articles.Infrastructure.EventHandling;

/// <summary>
/// Translates the module's own domain events into the integration events other modules may react
/// to, so that nothing outside Articles ever sees a domain type. Wolverine publishes each returned
/// message through the outbox, in a transaction of its own.
/// </summary>
public sealed class ArticleIntegrationEventsHandler
{
    public ArticlePublished Handle(ArticlePublishedDomainEvent domainEvent) =>
        new(ToSnapshot(domainEvent.Article));

    public ArticleEdited Handle(ArticleEditedDomainEvent domainEvent) =>
        new(ToSnapshot(domainEvent.Article));

    public ArticleDeleted Handle(ArticleDeletedDomainEvent domainEvent) =>
        new(domainEvent.ArticleId, domainEvent.Revision);

    private static ArticleSnapshot ToSnapshot(ArticleState state) =>
        new(
            state.ArticleId,
            state.Revision,
            state.Slug,
            state.Title,
            state.Description,
            state.Author,
            state.Tags,
            state.CreatedAtUtc,
            state.UpdatedAtUtc);
}
