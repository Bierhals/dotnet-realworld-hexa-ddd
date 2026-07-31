using System;

namespace Conduit.Identity.Domain;

public readonly record struct UserId
{
    public Guid Value { get; init; }
    private UserId(Guid value) => Value = value;

    public static UserId New() => new(Guid.CreateVersion7());

    public static UserId Rehydrate(string value) => new(Guid.Parse(value));
}
