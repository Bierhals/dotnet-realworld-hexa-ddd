using Conduit.Shared.Domain;

namespace Conduit.Identity.Domain.Events;

public record UserBioChangedDomainEvent : DomainEvent
{
    public string Username { get; }
    public string? Bio { get; }

    public UserBioChangedDomainEvent(string username, string? bio) : base()
    {
        Username = username;
        Bio = bio;
    }
}
