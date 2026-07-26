namespace Conduit.Shared.Application.Optional;

public readonly struct Optional<T>
{
    public bool IsSpecified { get; }
    public T Value { get; }

    public Optional(T value)
    {
        Value = value;
        IsSpecified = true;
    }
}