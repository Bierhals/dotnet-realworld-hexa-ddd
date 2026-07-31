using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Domain.ValueObjects;
using ErrorOr;

namespace Conduit.Identity.Domain.UnitTests.TestDoubles;

internal sealed class FakeUsersRepository : IUsersRepository
{
    private readonly List<User> _users = [];

    public void Add(User user) => _users.Add(user);

    public Task<ErrorOr<User>> GetByEmailAsync(UserEmail email, CancellationToken ct = default)
    {
        var user = _users.SingleOrDefault(u => u.Email == email);
        return Task.FromResult<ErrorOr<User>>(user is null ? Error.NotFound("User.NotFound") : user);
    }

    public Task<ErrorOr<User>> GetByUsernameAsync(Username username, CancellationToken ct = default)
    {
        var user = _users.SingleOrDefault(u => u.Username == username);
        return Task.FromResult<ErrorOr<User>>(user is null ? Error.NotFound("User.NotFound") : user);
    }

    public Task AddAsync(User user, CancellationToken ct = default)
    {
        _users.Add(user);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByEmailAsync(UserEmail email, CancellationToken ct = default) =>
        Task.FromResult(_users.Any(u => u.Email == email));

    public Task<bool> ExistsByEmailAsync(UserEmail email, UserId excludingUserId, CancellationToken ct = default) =>
        Task.FromResult(_users.Any(u => u.Email == email && u.Id != excludingUserId));

    public Task<bool> ExistsByUsernameAsync(Username username, CancellationToken ct = default) =>
        Task.FromResult(_users.Any(u => u.Username == username));

    public Task<bool> ExistsByUsernameAsync(Username username, UserId excludingUserId, CancellationToken ct = default) =>
        Task.FromResult(_users.Any(u => u.Username == username && u.Id != excludingUserId));
}
