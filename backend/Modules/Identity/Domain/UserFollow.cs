using Conduit.Identity.Domain.Events;
using Conduit.Identity.Domain.Rules;
using Conduit.Shared.Domain;
using ErrorOr;

namespace Conduit.Identity.Domain;

public sealed class UserFollow : AggregateRoot<UserFollowId>
{
    public UserId FollowedUserId { get; private set; }
    public UserId FollowerUserId { get; private set; }

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Fügen Sie ggf. den „erforderlichen“ Modifizierer hinzu, oder deklarieren Sie den Modifizierer als NULL-Werte zulassend.
    private UserFollow() { } // für EF Core
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Fügen Sie ggf. den „erforderlichen“ Modifizierer hinzu, oder deklarieren Sie den Modifizierer als NULL-Werte zulassend.

    private UserFollow(UserFollowId id, UserId followedUserId, UserId followerUserId) : base(id)
    {
        FollowedUserId = followedUserId;
        FollowerUserId = followerUserId;
    }

    public static ErrorOr<UserFollow> Create(UserId followedUserId, UserId followerUserId)
    {
        var check = new UserCannotFollowThemselves(followedUserId, followerUserId).Check();
        if (check.IsError)
        {
            return check.Errors;
        }

        var userFollow = new UserFollow(UserFollowId.New(), followedUserId, followerUserId);
        userFollow.AddDomainEvent(new UserFollowedDomainEvent(followedUserId.Value, followerUserId.Value));
        return userFollow;
    }

    public void Unfollow()
    {
        AddDomainEvent(new UserUnfollowedDomainEvent(FollowedUserId.Value, FollowerUserId.Value));
    }
}
