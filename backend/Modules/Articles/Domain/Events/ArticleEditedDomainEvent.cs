using Conduit.Shared.Domain;

namespace Conduit.Articles.Domain.Events;

public sealed record ArticleEditedDomainEvent(ArticleState Article) : DomainEvent;
