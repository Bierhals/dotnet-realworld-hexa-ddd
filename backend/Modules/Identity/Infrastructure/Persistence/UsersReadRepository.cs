using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Application;
using Conduit.Identity.Application.Queries.CurrentUser;
using Conduit.Identity.Application.Queries.Profile;
using Conduit.Identity.Domain.ValueObjects;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Identity.Infrastructure.Persistence;

public sealed class UsersReadRepository(IdentityDbContext dbContext) : IUsersReadRepository
{
    public async Task<ErrorOr<User>> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        var usernameVo = Username.Rehydrate(username);
        var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == usernameVo, ct);
        if (user is null)
        {
            return Error.NotFound("User.NotFound", "No user was found for the given username.");
        }

        return new User
        {
            Username = user.Username.Value,
            Email = user.Email.Value,
            Bio = user.Bio,
            Image = user.Image?.Value,
            Token = string.Empty,
        };
    }

    public async Task<ErrorOr<Profile>> GetProfileAsync(string username, string? currentUsername, CancellationToken ct = default)
    {
        var usernameVo = Username.Rehydrate(username);
        var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == usernameVo, ct);
        if (user is null)
        {
            return Error.NotFound("User.NotFound", "No user was found for the given username.");
        }

        var following = false;
        if (currentUsername is not null)
        {
            var currentUsernameVo = Username.Rehydrate(currentUsername);
            var currentUser = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == currentUsernameVo, ct);
            if (currentUser is not null)
            {
                following = await dbContext.UserFollows.AsNoTracking()
                    .AnyAsync(f => f.FollowedUserId == user.Id && f.FollowerUserId == currentUser.Id, ct);
            }
        }

        return new Profile
        {
            Username = user.Username.Value,
            Bio = user.Bio,
            Image = user.Image?.Value,
            Following = following,
        };
    }
}
