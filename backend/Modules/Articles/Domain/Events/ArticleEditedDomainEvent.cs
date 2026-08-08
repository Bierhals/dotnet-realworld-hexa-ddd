using System;
using Conduit.Shared.Domain;

namespace Conduit.Articles.Domain.Events;

public sealed record ArticleEditedDomainEvent(Guid ArticleId, string Slug) : DomainEvent;
