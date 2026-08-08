namespace Conduit.Articles.Domain;

/// <summary>
/// The public, numeric identity of a comment, as required by the RealWorld API. The numbers come
/// from a database sequence rather than from the aggregate, so that they stay unique across
/// articles without the aggregate having to query anything.
/// </summary>
public readonly record struct CommentId
{
    public int Value { get; init; }

    private CommentId(int value) => Value = value;

    public static CommentId From(int value) => new(value);
}
