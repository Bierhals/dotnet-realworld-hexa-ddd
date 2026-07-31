using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Domain;
using Conduit.Identity.Domain.ValueObjects;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Identity.Infrastructure.Persistence;

public sealed class UsersRepository(IdentityDbContext dbContext) : IUsersRepository
{
    public async Task<ErrorOr<User>> GetByEmailAsync(UserEmail email, CancellationToken ct = default)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        return user is null
            ? Error.NotFound("User.NotFound", "No user was found for the given email address.")
            : user;
    }

    public async Task<ErrorOr<User>> GetByUsernameAsync(Username username, CancellationToken ct = default)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username, ct);
        return user is null
            ? Error.NotFound("User.NotFound", "No user was found for the given username.")
            : user;
    }

    public async Task AddAsync(User user, CancellationToken ct = default) =>
        await dbContext.Users.AddAsync(user, ct);

    public Task<bool> ExistsByEmailAsync(UserEmail email, CancellationToken ct = default) =>
        dbContext.Users.AnyAsync(u => u.Email == email, ct);

    public Task<bool> ExistsByEmailAsync(UserEmail email, UserId excludingUserId, CancellationToken ct = default) =>
        dbContext.Users.AnyAsync(u => u.Email == email && u.Id != excludingUserId, ct);

    public Task<bool> ExistsByUsernameAsync(Username username, CancellationToken ct = default) =>
        dbContext.Users.AnyAsync(u => u.Username == username, ct);

    public Task<bool> ExistsByUsernameAsync(Username username, UserId excludingUserId, CancellationToken ct = default) =>
        dbContext.Users.AnyAsync(u => u.Username == username && u.Id != excludingUserId, ct);
}
