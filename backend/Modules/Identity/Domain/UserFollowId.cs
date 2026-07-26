using System;

namespace Conduit.Identity.Domain;

public readonly record struct UserFollowId
{
    public Guid Value { get; init; }
    private UserFollowId(Guid value) => Value = value;

    public static UserFollowId New() => new(Guid.CreateVersion7());

    public static UserFollowId Rehydrate(string value) => new(Guid.Parse(value));
}
