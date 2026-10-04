using System;
using Conduit.Shared.Domain;

namespace Conduit.Articles.Domain.Events;

/// <summary>
/// The revision is the one the deletion itself created, so it is newer than any state the article
/// was ever announced with.
/// </summary>
public sealed record ArticleDeletedDomainEvent(Guid ArticleId, int Revision) : DomainEvent;
