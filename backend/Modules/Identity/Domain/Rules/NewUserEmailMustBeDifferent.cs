using System.Net.Mail;
using Conduit.Identity.Domain.ValueObjects;
using Conduit.Shared.Domain;
using ErrorOr;

namespace Conduit.Identity.Domain.Rules;

public sealed class NewUserEmailMustBeDifferent : IBusinessRule
{
    private readonly UserEmail _newEmail;
    private readonly UserEmail _currentEmail;

    public NewUserEmailMustBeDifferent(UserEmail newEmail, UserEmail currentEmail)
    {
        _newEmail = newEmail;
        _currentEmail = currentEmail;
    }

    public ErrorOr<Success> Check()
    {
        return _newEmail == _currentEmail
            ? Error.Validation("User.EmailNotChanged", "The new email must be different from the current email.")
            : Result.Success;
    }
}
