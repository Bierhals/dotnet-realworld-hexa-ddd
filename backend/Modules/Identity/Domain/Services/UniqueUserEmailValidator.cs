using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Domain.ValueObjects;
using ErrorOr;

namespace Conduit.Identity.Domain.Services;

public sealed class UniqueUserEmailValidator
{
    private readonly IUsersRepository _usersRepository;
    public UniqueUserEmailValidator(IUsersRepository users) => _usersRepository = users;

    public async Task<ErrorOr<Success>> IsUniqueAsync(UserEmail email, UserId? excludingUserId = null, CancellationToken ct = default)
    {
        var emailExists = excludingUserId is null
            ? await _usersRepository.ExistsByEmailAsync(email, ct)
            : await _usersRepository.ExistsByEmailAsync(email, excludingUserId.Value, ct);

        return emailExists
            ? Error.Validation("User.NotUniqueEmail", "The user must have a unique email address.")
            : Result.Success;
    }
}