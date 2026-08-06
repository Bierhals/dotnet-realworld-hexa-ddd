using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application.Queries.ArticleDetails;
using Conduit.Articles.Application.UnitTests.TestDoubles;
using ErrorOr;
using Shouldly;

namespace Conduit.Articles.Application.UnitTests.Queries.ArticleDetails;

public class ArticleDetailsHandlerTests
{
    private readonly FakeArticlesReadRepository _articles = new();
    private readonly FakeProfileReader _profiles = new();

    private Task<ErrorOr<ArticleReadModel>> Details(string slug, string? viewer = null) =>
        new ArticleDetailsHandler(_articles, _profiles, new StubCurrentUserAccessor(viewer))
            .Handle(new ArticleDetailsQuery { Slug = slug }, CancellationToken.None);

    [Fact]
    public async Task An_article_is_returned_with_its_author_profile_filled_in()
    {
        _articles.Seed("how-to-train-your-dragon", "alice", ["dragons"]);
        _profiles.Seed("alice", bio: "I write", image: "alice.png");

        var result = await Details("how-to-train-your-dragon");

        result.IsError.ShouldBeFalse();
        result.Value.Slug.ShouldBe("how-to-train-your-dragon");
        result.Value.TagList.ShouldHaveSingleItem().ShouldBe("dragons");
        result.Value.Author.Username.ShouldBe("alice");
        result.Value.Author.Bio.ShouldBe("I write");
        result.Value.Author.Image.ShouldBe("alice.png");
    }

    [Fact]
    public async Task An_article_whose_author_has_no_profile_is_still_readable()
    {
        _articles.Seed("how-to-train-your-dragon", "ghost");

        var result = await Details("how-to-train-your-dragon");

        result.IsError.ShouldBeFalse();
        result.Value.Author.Username.ShouldBe("ghost");
        result.Value.Author.Bio.ShouldBeNull();
        result.Value.Author.Following.ShouldBeFalse();
    }

    [Fact]
    public async Task An_article_counts_as_favorited_only_for_the_reader_who_favorited_it()
    {
        _articles.Seed("how-to-train-your-dragon", "alice", favoritedBy: ["bob"]);
        _profiles.Seed("alice");

        (await Details("how-to-train-your-dragon", viewer: "bob")).Value.Favorited.ShouldBeTrue();
        (await Details("how-to-train-your-dragon", viewer: "carol")).Value.Favorited.ShouldBeFalse();
        (await Details("how-to-train-your-dragon")).Value.FavoritesCount.ShouldBe(1);
    }

    [Fact]
    public async Task A_reader_sees_whether_they_follow_the_author()
    {
        _articles.Seed("how-to-train-your-dragon", "alice");
        _profiles.Seed("alice");
        _profiles.Follows("bob", "alice");

        (await Details("how-to-train-your-dragon", viewer: "bob")).Value.Author.Following.ShouldBeTrue();
        (await Details("how-to-train-your-dragon", viewer: "carol")).Value.Author.Following.ShouldBeFalse();
    }

    [Fact]
    public async Task An_article_that_does_not_exist_is_reported_as_missing()
    {
        var result = await Details("no-such-article");

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
    }
}
