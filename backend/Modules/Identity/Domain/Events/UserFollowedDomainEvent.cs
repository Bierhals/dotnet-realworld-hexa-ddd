using System;
using Conduit.Shared.Domain;

namespace Conduit.Identity.Domain.Events;

public record UserFollowedDomainEvent : DomainEvent
{
    public Guid FollowedUserId { get; }
    public Guid FollowerUserId { get; }

    public UserFollowedDomainEvent(Guid followedUserId, Guid followerUserId) : base()
    {
        FollowedUserId = followedUserId;
        FollowerUserId = followerUserId;
    }
}
