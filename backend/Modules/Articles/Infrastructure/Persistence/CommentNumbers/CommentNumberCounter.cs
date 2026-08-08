namespace Conduit.Articles.Infrastructure.Persistence.CommentNumbers;

/// <summary>
/// Stands in for a database sequence on providers that have none. A single row holds the number
/// that will be handed out next.
/// </summary>
internal sealed class CommentNumberCounter
{
    public const int SingletonId = 1;

    public int Id { get; set; }

    public int NextValue { get; set; }
}
