using Conduit.Identity.Domain.Events;
using Shouldly;

namespace Conduit.Identity.Domain.UnitTests;

public class UserFollowTests
{
    [Fact]
    public void A_user_can_follow_another_user()
    {
        var followedUserId = UserId.New();
        var followerUserId = UserId.New();

        var result = UserFollow.Create(followedUserId, followerUserId);

        result.IsError.ShouldBeFalse();
        result.Value.FollowedUserId.ShouldBe(followedUserId);
        result.Value.FollowerUserId.ShouldBe(followerUserId);
    }

    [Fact]
    public void Following_another_user_raises_a_user_followed_event()
    {
        var followedUserId = UserId.New();
        var followerUserId = UserId.New();

        var result = UserFollow.Create(followedUserId, followerUserId);

        var raisedEvent = result.Value.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<UserFollowedDomainEvent>();
        raisedEvent.FollowedUserId.ShouldBe(followedUserId.Value);
        raisedEvent.FollowerUserId.ShouldBe(followerUserId.Value);
    }

    [Fact]
    public void A_user_cannot_follow_themselves()
    {
        var userId = UserId.New();

        var result = UserFollow.Create(userId, userId);

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("UserFollow.SelfFollow");
    }

    [Fact]
    public void Unfollowing_raises_a_user_unfollowed_event()
    {
        var followedUserId = UserId.New();
        var followerUserId = UserId.New();
        var userFollow = UserFollow.Create(followedUserId, followerUserId).Value;
        userFollow.ClearDomainEvents();

        userFollow.Unfollow();

        var raisedEvent = userFollow.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<UserUnfollowedDomainEvent>();
        raisedEvent.FollowedUserId.ShouldBe(followedUserId.Value);
        raisedEvent.FollowerUserId.ShouldBe(followerUserId.Value);
    }
}
