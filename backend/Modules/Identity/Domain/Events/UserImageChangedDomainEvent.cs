using Conduit.Shared.Domain;

namespace Conduit.Identity.Domain.Events;

public record UserImageChangedDomainEvent : DomainEvent
{
    public string Username { get; }
    public string? Image { get; }

    public UserImageChangedDomainEvent(string username, string? image) : base()
    {
        Username = username;
        Image = image;
    }
}
