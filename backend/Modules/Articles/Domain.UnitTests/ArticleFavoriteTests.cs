using System.Linq;
using Conduit.Articles.Domain;
using Conduit.Articles.Domain.Events;
using Conduit.Articles.Domain.ValueObjects;
using Shouldly;

namespace Conduit.Articles.Domain.UnitTests;

public class ArticleFavoriteTests
{
    private static readonly ArticleId Article = ArticleId.New();

    private static Username User(string name) => Username.Create(name).Value;

    [Fact]
    public void Favoriting_an_article_records_who_favorited_which_article()
    {
        var favorite = ArticleFavorite.Create(Article, User("bob"));

        favorite.ArticleId.ShouldBe(Article);
        favorite.Username.Value.ShouldBe("bob");
        favorite.DomainEvents.OfType<ArticleFavoritedDomainEvent>().Single().Username.ShouldBe("bob");
    }

    [Fact]
    public void Every_favorite_gets_its_own_identity()
    {
        var first = ArticleFavorite.Create(Article, User("bob"));
        var second = ArticleFavorite.Create(Article, User("carol"));

        first.Id.ShouldNotBe(second.Id);
    }

    [Fact]
    public void Giving_up_a_favorite_announces_that_the_article_is_no_longer_favorited()
    {
        var favorite = ArticleFavorite.Create(Article, User("bob"));
        favorite.ClearDomainEvents();

        favorite.Remove();

        var unfavorited = favorite.DomainEvents.OfType<ArticleUnfavoritedDomainEvent>().Single();
        unfavorited.Username.ShouldBe("bob");
        unfavorited.ArticleId.ShouldBe(Article.Value);
    }
}
