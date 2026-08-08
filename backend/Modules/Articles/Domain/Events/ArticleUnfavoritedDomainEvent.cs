using System;
using Conduit.Shared.Domain;

namespace Conduit.Articles.Domain.Events;

public sealed record ArticleUnfavoritedDomainEvent(Guid ArticleId, string Username) : DomainEvent;
