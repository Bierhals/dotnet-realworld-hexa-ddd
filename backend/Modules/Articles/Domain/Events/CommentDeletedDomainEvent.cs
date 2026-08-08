using System;
using Conduit.Shared.Domain;

namespace Conduit.Articles.Domain.Events;

public sealed record CommentDeletedDomainEvent(Guid ArticleId, int CommentId) : DomainEvent;
