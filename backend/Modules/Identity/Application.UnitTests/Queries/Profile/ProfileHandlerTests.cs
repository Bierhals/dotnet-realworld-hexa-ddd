using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Application.Queries.CurrentUser;
using Conduit.Identity.Application.Queries.Profile;
using Conduit.Identity.Application.UnitTests.TestDoubles;
using Shouldly;

namespace Conduit.Identity.Application.UnitTests.Queries.Profile;

public class ProfileHandlerTests
{
    [Fact]
    public async Task Fetching_a_profile_as_an_anonymous_viewer_returns_not_following()
    {
        var usersReadRepository = new FakeUsersReadRepository();
        usersReadRepository.AddUser(new User { Username = "jake", Email = "jake@jake.jake", Token = "n/a" });
        var handler = new ProfileHandler(new StubCurrentUserAccessor(null), usersReadRepository);
        var query = new ProfileQuery { Username = "jake" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Username.ShouldBe("jake");
        result.Value.Following.ShouldBeFalse();
    }

    [Fact]
    public async Task Fetching_a_profile_as_an_authenticated_follower_returns_following()
    {
        var usersReadRepository = new FakeUsersReadRepository();
        usersReadRepository.AddUser(new User { Username = "jake", Email = "jake@jake.jake", Token = "n/a" });
        usersReadRepository.AddUser(new User { Username = "john", Email = "john@jake.jake", Token = "n/a" });
        usersReadRepository.AddFollow(followerUsername: "john", followedUsername: "jake");
        var handler = new ProfileHandler(new StubCurrentUserAccessor("john"), usersReadRepository);
        var query = new ProfileQuery { Username = "jake" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Following.ShouldBeTrue();
    }
}
