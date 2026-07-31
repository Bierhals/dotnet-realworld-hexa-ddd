using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Application.Commands.UnfollowUser;
using Conduit.Identity.Application.UnitTests.TestDoubles;
using Conduit.Identity.Domain;
using Conduit.Identity.Domain.ValueObjects;
using Shouldly;

namespace Conduit.Identity.Application.UnitTests.Commands.UnfollowUser;

public class UnfollowUserHandlerTests
{
    [Fact]
    public async Task Unfollowing_a_followed_user_removes_the_relationship()
    {
        var follower = RegisterUser("jake");
        var target = RegisterUser("john");
        var usersRepository = new FakeUsersRepository();
        usersRepository.Add(follower);
        usersRepository.Add(target);
        var userFollowsRepository = new FakeUserFollowsRepository();
        await userFollowsRepository.AddAsync(UserFollow.Create(target.Id, follower.Id).Value, CancellationToken.None);
        var handler = CreateHandler(usersRepository, userFollowsRepository, currentUsername: "jake");
        var command = new UnfollowUserCommand { Username = "john" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        userFollowsRepository.UserFollows.ShouldBeEmpty();
    }

    [Fact]
    public async Task Unfollowing_a_user_that_is_not_followed_is_a_no_op_that_still_succeeds()
    {
        var usersRepository = new FakeUsersRepository();
        usersRepository.Add(RegisterUser("jake"));
        usersRepository.Add(RegisterUser("john"));
        var handler = CreateHandler(usersRepository, new FakeUserFollowsRepository(), currentUsername: "jake");
        var command = new UnfollowUserCommand { Username = "john" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeFalse();
    }

    [Fact]
    public async Task Unfollowing_throws_when_there_is_no_current_user()
    {
        var usersRepository = new FakeUsersRepository();
        var handler = CreateHandler(usersRepository, new FakeUserFollowsRepository(), currentUsername: null);
        var command = new UnfollowUserCommand { Username = "john" };

        await Should.ThrowAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
    }

    private static UnfollowUserHandler CreateHandler(FakeUsersRepository usersRepository, FakeUserFollowsRepository userFollowsRepository, string? currentUsername) =>
        new(new StubCurrentUserAccessor(currentUsername), usersRepository, userFollowsRepository, new FakeUnitOfWork());

    private static User RegisterUser(string username) =>
        User.RegisterNewUser(UserEmail.Create($"{username}@jake.jake").Value, Username.Create(username).Value, "hashed-password");
}
