using System.Net.Mail;
using Conduit.Shared.Domain;
using ErrorOr;

namespace Conduit.Identity.Domain.Rules;

public sealed class UserHasValidEmail : IBusinessRule
{
    private readonly string _email;
    public UserHasValidEmail(string email) => _email = email;

    public ErrorOr<Success> Check()
    {
        var mailIsValid = MailAddress.TryCreate(_email, out _);

        return !mailIsValid
            ? Error.Validation("User.InvalidEmail", "The user must have a valid email address.")
            : Result.Success;
    }
}