using System.Linq;
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
        var currentUsernameVo = currentUsername is not null ? Username.Rehydrate(currentUsername) : null;

        var profile = await dbContext.Users.AsNoTracking()
            .Where(u => u.Username == usernameVo)
            .Select(u => new Profile
            {
                Username = u.Username.Value,
                Bio = u.Bio,
                Image = u.Image != null ? u.Image.Value : null,
                Following = currentUsernameVo != null && dbContext.UserFollows.Any(f =>
                    f.FollowedUserId == u.Id &&
                    dbContext.Users.Any(cu => cu.Id == f.FollowerUserId && cu.Username == currentUsernameVo)),
            })
            .FirstOrDefaultAsync(ct);

        if (profile is null)
        {
            return Error.NotFound("User.NotFound", "No user was found for the given username.");
        }

        return profile;
    }
}
