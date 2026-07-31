namespace Conduit.Host.WebApi.Domain;

public class ArticleFavorite
{
    public int ArticleId { get; init; }
    public Article? Article { get; init; }

    public string Username { get; init; } = null!;
}
