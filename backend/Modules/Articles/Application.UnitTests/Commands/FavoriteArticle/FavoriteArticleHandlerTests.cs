using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application.Commands.FavoriteArticle;
using Conduit.Articles.Application.Commands.UnfavoriteArticle;
using Conduit.Articles.Application.UnitTests.TestDoubles;
using Conduit.Articles.Domain;
using ErrorOr;
using Shouldly;

namespace Conduit.Articles.Application.UnitTests.Commands.FavoriteArticle;

public class FavoriteArticleHandlerTests
{
    private readonly FakeArticlesRepository _articles = new();
    private readonly FakeArticleFavoritesRepository _favorites = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private Task<ErrorOr<Success>> Favorite(string slug, string? user = "bob") =>
        new FavoriteArticleHandler(_articles, _favorites, _unitOfWork, new StubCurrentUserAccessor(user))
            .Handle(new FavoriteArticleCommand { Slug = slug }, CancellationToken.None);

    private Task<ErrorOr<Success>> Unfavorite(string slug, string? user = "bob") =>
        new UnfavoriteArticleHandler(_articles, _favorites, _unitOfWork, new StubCurrentUserAccessor(user))
            .Handle(new UnfavoriteArticleCommand { Slug = slug }, CancellationToken.None);

    [Fact]
    public async Task Favoriting_an_article_records_it_for_that_reader_only()
    {
        var article = _articles.Seed();

        var result = await Favorite("how-to-train-your-dragon");

        result.IsError.ShouldBeFalse();
        _favorites.CountFor(article.Id).ShouldBe(1);
        _favorites.Favorites.ShouldHaveSingleItem().Username.Value.ShouldBe("bob");
        _unitOfWork.SaveCount.ShouldBe(1);
    }

    [Fact]
    public async Task Favoriting_an_article_twice_counts_once()
    {
        var article = _articles.Seed();

        await Favorite("how-to-train-your-dragon");
        await Favorite("how-to-train-your-dragon");

        _favorites.CountFor(article.Id).ShouldBe(1);
        _unitOfWork.SaveCount.ShouldBe(1);
    }

    [Fact]
    public async Task Two_readers_favoriting_the_same_article_both_count()
    {
        var article = _articles.Seed();

        await Favorite("how-to-train-your-dragon", user: "bob");
        await Favorite("how-to-train-your-dragon", user: "carol");

        _favorites.CountFor(article.Id).ShouldBe(2);
    }

    [Fact]
    public async Task Unfavoriting_gives_the_article_back()
    {
        var article = _articles.Seed();
        await Favorite("how-to-train-your-dragon");

        var result = await Unfavorite("how-to-train-your-dragon");

        result.IsError.ShouldBeFalse();
        _favorites.CountFor(article.Id).ShouldBe(0);
    }

    [Fact]
    public async Task Unfavoriting_only_gives_up_your_own_favorite()
    {
        var article = _articles.Seed();
        _favorites.Seed(article.Id, "carol");
        await Favorite("how-to-train-your-dragon", user: "bob");

        await Unfavorite("how-to-train-your-dragon", user: "bob");

        _favorites.CountFor(article.Id).ShouldBe(1);
        _favorites.Favorites.ShouldHaveSingleItem().Username.Value.ShouldBe("carol");
    }

    [Fact]
    public async Task Unfavoriting_an_article_you_never_favorited_still_succeeds()
    {
        var article = _articles.Seed();

        var result = await Unfavorite("how-to-train-your-dragon");

        result.IsError.ShouldBeFalse();
        _favorites.CountFor(article.Id).ShouldBe(0);
        _unitOfWork.SaveCount.ShouldBe(0);
    }

    [Fact]
    public async Task Favoriting_an_article_that_does_not_exist_reports_it_as_missing()
    {
        var result = await Favorite("no-such-article");

        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
    }

    [Fact]
    public async Task Favoriting_requires_being_signed_in()
    {
        _articles.Seed();

        var result = await Favorite("how-to-train-your-dragon", user: null);

        result.FirstError.Type.ShouldBe(ErrorType.Unauthorized);
    }
}
