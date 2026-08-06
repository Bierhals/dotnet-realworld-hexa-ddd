using System.Text.RegularExpressions;
using Conduit.Shared.Domain;
using ErrorOr;

namespace Conduit.Identity.Domain.Rules;

public sealed partial class UsernameContainsOnlyAllowedCharacters : IBusinessRule
{
    private readonly string _username;
    public UsernameContainsOnlyAllowedCharacters(string username) => _username = username;

    public ErrorOr<Success> Check()
    {
        var containsOnlyAllowedCharacters = AllowedUsernameCharactersRegEx().IsMatch(_username);

        return !containsOnlyAllowedCharacters
            ? Error.Validation("User.InvalidUsername", "The user must have a valid username.")
            : Result.Success;
    }

    [GeneratedRegex("^[A-Za-z0-9_.-]+$")]
    private static partial Regex AllowedUsernameCharactersRegEx();
}
