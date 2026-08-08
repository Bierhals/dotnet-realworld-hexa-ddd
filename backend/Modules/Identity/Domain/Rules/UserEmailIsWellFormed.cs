using System.Net.Mail;
using Conduit.Shared.Domain;
using ErrorOr;

namespace Conduit.Identity.Domain.Rules;

public sealed class UserEmailIsWellFormed : IBusinessRule
{
    private readonly string _email;
    public UserEmailIsWellFormed(string email) => _email = email;

    public ErrorOr<Success> Check()
    {
        var emailIsWellFormed = MailAddress.TryCreate(_email, out _);

        return !emailIsWellFormed
            ? Error.Validation("User.InvalidEmail", "The user must have a valid email address.")
            : Result.Success;
    }
}
