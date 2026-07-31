using ErrorOr;

namespace Conduit.Identity.Domain.Services;

public sealed class UserLoginValidator
{
    private readonly IPasswordHasher _hasher;

    public UserLoginValidator(IPasswordHasher hasher)
    {
        _hasher = hasher;
    }

    public ErrorOr<Success> Validate(User user, string plainPassword)
    {
        var isValid = _hasher.Verify(plainPassword, user.PasswordHash);

        return isValid
            ? Result.Success
            : Error.Unauthorized("User.InvalidCredentials", "The provided credentials are invalid.");
    }
}
