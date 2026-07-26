using Conduit.Identity.Domain.Events;
using Conduit.Identity.Domain.Rules;
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
    
    public static User RegisterNewUser(UserEmail email, Username username, string hashedPassword)
    {
        var user = new User(UserId.New(), email, username, hashedPassword, null, null);
        user.AddDomainEvent(new NewUserRegisteredDomainEvent(email.Value, username.Value));
        return user;
    }

    public void ChangePassword(string newHashedPassword)
    {
        PasswordHash = newHashedPassword;
        AddDomainEvent(new PasswordChangedDomainEvent(Username.Value));
    }

    public ErrorOr<Success> ChangeUsername(Username newUsername)
    {
        var usernameCheck = new NewUsernameMustBeDifferent(newUsername, Username).Check();
        if (usernameCheck.IsError)
        {
            return usernameCheck.Errors;
        }

        var previousUsername = Username.Value;
        Username = newUsername;
        AddDomainEvent(new UsernameChangedDomainEvent(previousUsername, Username.Value));
        return Result.Success;
    }

    public ErrorOr<Success> ChangeEmail(UserEmail newEmail)
    {
        var emailCheck = new NewUserEmailMustBeDifferent(newEmail, Email).Check();
        if (emailCheck.IsError)
        {
            return emailCheck.Errors;
        }

        Email = newEmail;
        AddDomainEvent(new EmailChangedDomainEvent(Username.Value, Email.Value));
        return Result.Success;
    }

    public void ChangeBio(string? bio)
    {
        Bio = string.IsNullOrWhiteSpace(bio) ? null : bio;
        AddDomainEvent(new UserBioChangedDomainEvent(Username.Value, Bio));
    }

    public void ChangeImage(UserImage? image)
    {
        Image = image;
        AddDomainEvent(new UserImageChangedDomainEvent(Username.Value, Image?.Value));
    }
}