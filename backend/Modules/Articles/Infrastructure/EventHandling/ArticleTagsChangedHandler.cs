using Conduit.Articles.Contracts.Events;
using Conduit.Articles.Domain.Events;

namespace Conduit.Articles.Infrastructure.EventHandling;

/// <summary>
/// Translates the module's own domain event into the integration events other modules may react to,
/// so that nothing outside Articles ever sees a domain type. Wolverine publishes the non-null
/// members of the returned tuple through the outbox, in a transaction of their own.
/// </summary>
/// <remarks>
/// Referenced and released tags travel as separate messages on purpose: each is applied by its
/// consumer in one transaction, so a retry can never replay half of an edit.
/// </remarks>
public sealed class ArticleTagsChangedHandler
{
    public (ArticleTagsReferenced?, ArticleTagsReleased?) Handle(ArticleTagsChangedDomainEvent domainEvent) =>
    (
        domainEvent.Added.Count > 0 ? new ArticleTagsReferenced(domainEvent.ArticleId, domainEvent.Added) : null,
        domainEvent.Removed.Count > 0 ? new ArticleTagsReleased(domainEvent.ArticleId, domainEvent.Removed) : null
    );
}
