using Conduit.Shared.Domain;

namespace Conduit.Tags.Core.Domain.Events;

public sealed record TagAddedToCatalogDomainEvent(string TagName) : DomainEvent;
