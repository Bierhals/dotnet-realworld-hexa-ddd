using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Domain.ValueObjects;
using ErrorOr;

namespace Conduit.Identity.Domain.Services;

public sealed class UniqueUsernameValidator
{
    private readonly IUsersRepository _usersRepository;
    public UniqueUsernameValidator(IUsersRepository users) => _usersRepository = users;

    public async Task<ErrorOr<Success>> IsUniqueAsync(Username username, UserId? excludingUserId = null, CancellationToken ct = default)
    {
        var usernameExists = excludingUserId is null
            ? await _usersRepository.ExistsByUsernameAsync(username, ct)
            : await _usersRepository.ExistsByUsernameAsync(username, excludingUserId.Value, ct);

        return usernameExists
            ? Error.Validation("User.NotUniqueUsername", "The user must have a unique username.")
            : Result.Success;
    }
}
