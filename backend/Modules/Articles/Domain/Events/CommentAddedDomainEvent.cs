using System;
using Conduit.Shared.Domain;

namespace Conduit.Articles.Domain.Events;

public sealed record CommentAddedDomainEvent(Guid ArticleId, int CommentId, string Author) : DomainEvent;
