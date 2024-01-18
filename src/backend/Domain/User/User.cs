using Conduit.Domain.Common;
using Conduit.Domain.User.Events;
using Conduit.Domain.User.Rules;

namespace Conduit.Domain.User;

public class User : AggregateRoot<UserId>
{
    public string Username { get; private set;}
    public string Password { get; private set; }
    public string Bio { get; }
    public string Image { get; }

    User(UserId id, string username, string password, string bio, string image) : base(id)
    {
        Username = username;
        Password = password;
        Bio = bio;
        Image = image;
    }

    public static User RegisterNewUser(string email, string username, string clearTextPassword, IUsersCounter usersCounter, IPasswordHasher passwordHasher)
    {
        string emailLowercase = email.ToLower();
        UserId id = new (emailLowercase);
        string hashedPassword = passwordHasher.HashPassword(clearTextPassword);
        
        User newUser = new (id, username, hashedPassword, string.Empty, string.Empty);
        
        newUser.CheckRule(new UserEmailMustBeValidRule(email));
        newUser.CheckRule(new UsernameMustBeProvidedRule(username));
        newUser.CheckRule(new UsernameCanOnlyContainLettersAndNumbersRule(username));
        newUser.CheckRule(new UserPasswordIsToShortRule(clearTextPassword.Length));
        newUser.CheckRule(new UserPasswordIsBlacklistedRule(clearTextPassword));
        newUser.CheckRule(new UserEmailMustBeUniqueRule(email, usersCounter));
        newUser.CheckRule(new UsernameMustBeUniqueRule(username, usersCounter));

        newUser.AddDomainEvent(new NewUserRegisteredDomainEvent(emailLowercase, username));

        return newUser;
    }

    public void ChangePassword(string newClearTextPassword, IPasswordHasher passwordHasher)
    {
        CheckRule(new UserPasswordIsToShortRule(newClearTextPassword.Length));
        CheckRule(new UserPasswordIsBlacklistedRule(newClearTextPassword));

        Password = passwordHasher.HashPassword(newClearTextPassword);

        AddDomainEvent(new PasswordChangedDomainEvent(Username));
    }

    public void ChangeUsername(string newUsername, IUsersCounter usersCounter)
    {
        CheckRule(new UsernameMustBeProvidedRule(newUsername));
        CheckRule(new UsernameCanOnlyContainLettersAndNumbersRule(newUsername));
        CheckRule(new UsernameMustBeUniqueRule(newUsername, usersCounter));

        string oldUsername = Username;
        Username = newUsername;

        AddDomainEvent(new UsernameChangedDomainEvent(oldUsername, Username));
    }
}
