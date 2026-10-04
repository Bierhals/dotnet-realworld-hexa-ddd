using Conduit.Shared.Domain;

namespace Conduit.Articles.Domain.Events;

public sealed record ArticlePublishedDomainEvent(ArticleState Article) : DomainEvent;
