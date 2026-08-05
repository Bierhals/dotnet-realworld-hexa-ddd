namespace Conduit.Host.WebApi.Domain;

/// <summary>
/// Links an article to a tag name. The tag catalog itself is owned by the Tags module, so this
/// join entity only stores the name - there is no navigation into another module's data.
/// </summary>
public class ArticleTag
{
    public int ArticleId { get; init; }
    public Article? Article { get; init; }

    public string? TagId { get; init; }
}
