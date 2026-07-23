using Conduit.Shared.Domain;

namespace Conduit.Identity.Domain.Events;

public record UsernameChangedDomainEvent : DomainEvent
{
    public string PreviousUsername { get; }
    public string Username { get; }

    public UsernameChangedDomainEvent(string previousUsername, string username) : base()
    {
        PreviousUsername = previousUsername;
        Username = username;
    }
}