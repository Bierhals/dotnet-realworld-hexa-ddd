using Conduit.Shared.Domain;

namespace Conduit.Identity.Domain.Events;

public record UserProfileChangedDomainEvent : DomainEvent
{
    public string Username { get; }
    public string? Bio { get; }
    public string? Image { get; }

    public UserProfileChangedDomainEvent(string username, string? bio, string? image) : base()
    {
        Username = username;
        Bio = bio;
        Image = image;
    }
}