using Conduit.Shared.Domain;

namespace Conduit.Identity.Domain.Events;

public record NewUserRegisteredDomainEvent : DomainEvent
{
    public string Email { get; }
    public string Username { get; }

    public NewUserRegisteredDomainEvent(string email, string username) : base()
    {
        Email = email;
        Username = username;
    }
}
