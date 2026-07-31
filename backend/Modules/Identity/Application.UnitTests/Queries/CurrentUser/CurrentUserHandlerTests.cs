using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Application.Queries.CurrentUser;
using Conduit.Identity.Application.UnitTests.TestDoubles;
using Shouldly;

namespace Conduit.Identity.Application.UnitTests.Queries.CurrentUser;

public class CurrentUserHandlerTests
{
    [Fact]
    public async Task Fetching_the_current_user_returns_their_data_with_a_freshly_generated_token()
    {
        var usersReadRepository = new FakeUsersReadRepository();
        usersReadRepository.AddUser(new User
        {
            Username = "jake",
            Email = "jake@jake.jake",
            Bio = "I work at statefarm",
            Image = null,
            Token = "stale-token",
        });
        var handler = new CurrentUserHandler(new StubCurrentUserAccessor("jake"), usersReadRepository, new StubTokenGenerator());

        var result = await handler.Handle(new CurrentUserQuery(), CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.Username.ShouldBe("jake");
        result.Value.Email.ShouldBe("jake@jake.jake");
        result.Value.Bio.ShouldBe("I work at statefarm");
        result.Value.Token.ShouldBe("token-for-jake");
    }

    [Fact]
    public async Task Fetching_the_current_user_throws_when_there_is_no_current_user()
    {
        var handler = new CurrentUserHandler(new StubCurrentUserAccessor(null), new FakeUsersReadRepository(), new StubTokenGenerator());

        await Should.ThrowAsync<UnauthorizedAccessException>(() => handler.Handle(new CurrentUserQuery(), CancellationToken.None));
    }
}
