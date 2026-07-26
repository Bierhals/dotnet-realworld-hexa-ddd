using Conduit.Shared.Domain;
using ErrorOr;

namespace Conduit.Identity.Domain.Rules;

public sealed class UserCannotFollowThemselves : IBusinessRule
{
    private readonly UserId _followedUserId;
    private readonly UserId _followerUserId;

    public UserCannotFollowThemselves(UserId followedUserId, UserId followerUserId)
    {
        _followedUserId = followedUserId;
        _followerUserId = followerUserId;
    }

    public ErrorOr<Success> Check()
    {
        return _followedUserId == _followerUserId
            ? Error.Validation("UserFollow.SelfFollow", "A user cannot follow themselves.")
            : Result.Success;
    }
}
