using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Application.Commands.FollowUser;
using Conduit.Identity.Application.UnitTests.TestDoubles;
using Conduit.Identity.Domain;
using Conduit.Identity.Domain.ValueObjects;
using Shouldly;

namespace Conduit.Identity.Application.UnitTests.Commands.FollowUser;

public class FollowUserHandlerTests
{
    [Fact]
    public async Task Following_another_user_creates_a_follow_relationship()
    {
        var follower = RegisterUser("jake");
        var target = RegisterUser("john");
        var usersRepository = new FakeUsersRepository();
        usersRepository.Add(follower);
        usersRepository.Add(target);
        var userFollowsRepository = new FakeUserFollowsRepository();
        var handler = CreateHandler(usersRepository, userFollowsRepository, currentUsername: "jake");
        var command = new FollowUserCommand { Username = "john" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        var userFollow = userFollowsRepository.UserFollows.ShouldHaveSingleItem();
        userFollow.FollowedUserId.ShouldBe(target.Id);
        userFollow.FollowerUserId.ShouldBe(follower.Id);
    }

    [Fact]
    public async Task Following_a_user_that_is_already_followed_is_a_no_op_that_still_succeeds()
    {
        var follower = RegisterUser("jake");
        var target = RegisterUser("john");
        var usersRepository = new FakeUsersRepository();
        usersRepository.Add(follower);
        usersRepository.Add(target);
        var userFollowsRepository = new FakeUserFollowsRepository();
        await userFollowsRepository.AddAsync(UserFollow.Create(target.Id, follower.Id).Value, CancellationToken.None);
        var handler = CreateHandler(usersRepository, userFollowsRepository, currentUsername: "jake");
        var command = new FollowUserCommand { Username = "john" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        userFollowsRepository.UserFollows.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Following_throws_when_there_is_no_current_user()
    {
        var usersRepository = new FakeUsersRepository();
        var handler = CreateHandler(usersRepository, new FakeUserFollowsRepository(), currentUsername: null);
        var command = new FollowUserCommand { Username = "john" };

        await Should.ThrowAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Following_a_non_existent_target_user_returns_a_not_found_error()
    {
        var usersRepository = new FakeUsersRepository();
        usersRepository.Add(RegisterUser("jake"));
        var handler = CreateHandler(usersRepository, new FakeUserFollowsRepository(), currentUsername: "jake");
        var command = new FollowUserCommand { Username = "unknown" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorOr.ErrorType.NotFound);
    }

    private static FollowUserHandler CreateHandler(FakeUsersRepository usersRepository, FakeUserFollowsRepository userFollowsRepository, string? currentUsername) =>
        new(new StubCurrentUserAccessor(currentUsername), usersRepository, userFollowsRepository, new FakeUnitOfWork());

    private static User RegisterUser(string username) =>
        User.RegisterNewUser(UserEmail.Create($"{username}@jake.jake").Value, Username.Create(username).Value, "hashed-password");
}
