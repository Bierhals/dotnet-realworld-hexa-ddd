using Conduit.Shared.Domain;
using Conduit.Domain.User.Events;
using Conduit.Domain.User.Rules;
using ErrorOr;

namespace Conduit.Users.Domain.User;

public class User : AggregateRoot<UserId>
{
    public UserEmail Email { get; protected set; }
    public Username Username { get; protected set; }
    public string HashedPassword { get; private set; }
    public string Bio { get; private set; }
    public string Image { get; private set; }

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
    private User()
    {
        //for ef only
    }
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.

    private User(UserId id, UserEmail userEmail, Username username, string hashedPassword, string bio, string image) : base(id)
    {
        Email = userEmail;
        Username = username;
        HashedPassword = hashedPassword;
        Bio = bio;
        Image = image;
    }

    public static ErrorOr<User> RegisterNewUser(UserEmail email, Username username, string clearTextPassword, IUsersCounter usersCounter, IPasswordHasher passwordHasher)
    {
        return CanRegisterNewUser(email, username, clearTextPassword, usersCounter)
            .Match(
                onFailure: error => error,
                onSuccess: () =>
                {
                    string hashedPassword = passwordHasher.HashPassword(clearTextPassword);
                    UserId id = UserId.Create();

                    User newUser = new(id, email, username, hashedPassword, string.Empty, string.Empty);

                    newUser.AddDomainEvent(new NewUserRegisteredDomainEvent(email.Value, username.Value));

                    return Result.Success<User, Error>(newUser);
                });
    }

    static UnitResult<Error> CanRegisterNewUser(UserEmail email, Username username, string clearTextPassword, IUsersCounter usersCounter)
    {
        return Result.Combine(
            UserRules.PasswordMustBeOfMinimumLengthRule(clearTextPassword.Length),
            UserRules.PasswordIsNotInBlacklistRule(clearTextPassword),
            UserRules.UsernameMustBeUniqueRule(username, usersCounter),
            UserRules.EmailMustBeUniqueRule(email, usersCounter));
    }

    public UnitResult<Error> ChangePassword(string newClearTextPassword, IPasswordHasher passwordHasher)
    {
        return CanChangePassword(newClearTextPassword, passwordHasher)
            .Match(
                onFailure: error => error,
                onSuccess: () =>
                {
                    HashedPassword = passwordHasher.HashPassword(newClearTextPassword);
                    AddDomainEvent(new PasswordChangedDomainEvent(Email.Value));

                    return UnitResult.Success<Error>();
                });
    }

    static UnitResult<Error> CanChangePassword(string newClearTextPassword, IPasswordHasher passwordHasher)
    {
        return Result.Combine(
            UserRules.PasswordMustBeOfMinimumLengthRule(newClearTextPassword.Length),
            UserRules.PasswordIsNotInBlacklistRule(newClearTextPassword));
    }

    public UnitResult<Error> ChangeUsername(Username newUsername, IUsersCounter usersCounter)
    {
        return CanChangeUsername(newUsername, usersCounter)
            .Match(
                onFailure: error => error,
                onSuccess: () =>
                {
                    string oldUsername = Username.Value;
                    Username = newUsername;

                    AddDomainEvent(new UsernameChangedDomainEvent(Email.Value, oldUsername, Username.Value));

                    return UnitResult.Success<Error>();
                });
    }

    static UnitResult<Error> CanChangeUsername(Username newUsername, IUsersCounter usersCounter)
    {
        return UserRules.UsernameMustBeUniqueRule(newUsername, usersCounter);
    }

    public UnitResult<Error> ChangeEMail(UserEmail newUserEMail, IUsersCounter usersCounter)
    {
        return CanChangeEMail(newUserEMail, usersCounter)
            .Match(
                onFailure: error => error,
                onSuccess: () =>
                {
                    string oldUserEMail = Email.Value;
                    Email = newUserEMail;

                    AddDomainEvent(new UserEMailChangedDomainEvent(oldUserEMail, Email.Value));

                    return UnitResult.Success<Error>();
                });
    }

    static UnitResult<Error> CanChangeEMail(UserEmail newUserEMail, IUsersCounter usersCounter)
    {
        return UserRules.EmailMustBeUniqueRule(newUserEMail, usersCounter);
    }

    public void ChangeImage(string image)
    {
        string oldImage = Image;
        Image = image;
        AddDomainEvent(new UserImageChangedDomainEvent(Email.Value, oldImage, image));
    }

    public void ChangeBio(string newBio)
    {
        string oldBio = newBio;
        Bio = newBio;
        AddDomainEvent(new UserBioChangedDomainEvent(Email.Value, oldBio, Bio));
    }
}
