using Conduit.Shared.Domain;

namespace Conduit.Identity.Domain.Events;

public record PasswordChangedDomainEvent : DomainEvent
{
    public string Username { get; }

    public PasswordChangedDomainEvent(string username) : base()
    {
        Username = username;
    }
}
