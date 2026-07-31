using System;
using Conduit.Shared.Domain;

namespace Conduit.Identity.Domain.Events;

public record UserUnfollowedDomainEvent : DomainEvent
{
    public Guid FollowedUserId { get; }
    public Guid FollowerUserId { get; }

    public UserUnfollowedDomainEvent(Guid followedUserId, Guid followerUserId) : base()
    {
        FollowedUserId = followedUserId;
        FollowerUserId = followerUserId;
    }
}
