using System.Net.Mail;
using Conduit.Identity.Domain.ValueObjects;
using Conduit.Shared.Domain;
using ErrorOr;

namespace Conduit.Identity.Domain.Rules;

public sealed class NewUsernameMustBeDifferent : IBusinessRule
{
    private readonly Username _newUsername;
    private readonly Username _currentUsername;

    public NewUsernameMustBeDifferent(Username newUsername, Username currentUsername)
    {
        _newUsername = newUsername;
        _currentUsername = currentUsername;
    }

    public ErrorOr<Success> Check()
    {
        return _newUsername == _currentUsername
            ? Error.Validation("User.UsernameNotChanged", "The new username must be different from the current username.")
            : Result.Success;
    }
}
