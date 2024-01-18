namespace Conduit.Domain.User.Rules;

public class UserEmailMustBeUniqueRule : IBusinessRule
{
    readonly IUsersCounter _usersCounter;
    readonly string _email;

    public UserEmailMustBeUniqueRule(string email, IUsersCounter usersCounter)
    {
        _email = email;
        _usersCounter = usersCounter;
    }

    public string Message => "Email must be unique";

    public bool IsBroken()
    {
        return _usersCounter.CountUsersWithEmail(_email) > 0;
    }
}
