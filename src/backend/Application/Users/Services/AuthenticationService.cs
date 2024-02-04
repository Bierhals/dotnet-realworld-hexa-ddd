using System.Threading;
using System.Threading.Tasks;
using Conduit.Application.Users.Dtos;
using Conduit.Application.Users.Repositories;
using Conduit.Domain.User;

namespace Conduit.Application.Users.Services;

public class AuthenticationService : IAuthenticationService
{
    readonly IUsersQueryRepository _usersQueryRepository;
    readonly IPasswordHasher _passwordHasher;

    public AuthenticationService(
        IUsersQueryRepository usersQueryRepository,
        IPasswordHasher passwordHasher)
    {
        _usersQueryRepository = usersQueryRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<bool> ValidateLoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        LoginDto? persistedLogin = await _usersQueryRepository.FindLoginByEmailAsync(email, cancellationToken);
        if (persistedLogin == null)
        {
            return false;
        }

        return _passwordHasher.VerifyPassword(password, persistedLogin.HashedPassword);
    }
}
