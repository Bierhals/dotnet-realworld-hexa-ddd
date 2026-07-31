using Conduit.Shared.Domain;

namespace Conduit.Identity.Domain.Events;

public record EmailChangedDomainEvent : DomainEvent
{
    public string Email { get; }
    public string Username { get; }

    public EmailChangedDomainEvent(string username, string email) : base()
    {
        Username = username;
        Email = email;
    }
}
