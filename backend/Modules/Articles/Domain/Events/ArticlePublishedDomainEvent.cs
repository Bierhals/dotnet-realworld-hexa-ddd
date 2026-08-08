using System;
using Conduit.Shared.Domain;

namespace Conduit.Articles.Domain.Events;

public sealed record ArticlePublishedDomainEvent(Guid ArticleId, string Slug, string Author) : DomainEvent;
