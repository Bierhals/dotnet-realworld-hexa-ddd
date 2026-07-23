using System.Text.RegularExpressions;
using Conduit.Shared.Domain;
using ErrorOr;

namespace Conduit.Identity.Domain.Rules;

public sealed partial class UserHasValidUsername : IBusinessRule
{
    private readonly string _username;
    public UserHasValidUsername(string username) => _username = username;

    public ErrorOr<Success> Check()
    {
        var usernameIsValid = ValidUsernameRegEx().IsMatch(_username);

        return !usernameIsValid
            ? Error.Validation("User.InvalidUsername", "The user must have a valid username.")
            : Result.Success;
    }

    [GeneratedRegex("^[A-Za-z0-9_.-]+$")]
    private static partial Regex ValidUsernameRegEx();
}