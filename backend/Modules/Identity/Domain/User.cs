using Conduit.Identity.Domain.Events;
using Conduit.Identity.Domain.ValueObjects;
using Conduit.Shared.Domain;
using ErrorOr;

namespace Conduit.Identity.Domain;

public sealed class User : AggregateRoot<UserId>
{
    public Username Username { get; private set; }
    public UserEmail Email { get; private set; }
    public string? Bio { get; private set; }
    public UserImage? Image { get; private set; }
    public string PasswordHash { get; private set; }

    #pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Fügen Sie ggf. den „erforderlichen“ Modifizierer hinzu, oder deklarieren Sie den Modifizierer als NULL-Werte zulassend.
    private User() { } // für EF Core
    #pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Fügen Sie ggf. den „erforderlichen“ Modifizierer hinzu, oder deklarieren Sie den Modifizierer als NULL-Werte zulassend.

    private User(UserId id, UserEmail userEmail, Username username, string hashedPassword, string? bio, UserImage? image) : base(id)
    {
        Email = userEmail;
        Username = username;
        PasswordHash = hashedPassword;
        Bio = bio;
        Image = image;
    }
    
    public static ErrorOr<User> RegisterNewUser(UserEmail email, Username username, string hashedPassword)
    {
        // TODO: Validate
        var user = new User(UserId.New(), email, username, hashedPassword, null, null);
        user.RaiseDomainEvent(new NewUserRegisteredDomainEvent(email.Value, username.Value));
        return user;
    }
}