using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application.Queries.ArticleList;
using Conduit.Articles.Application.UnitTests.TestDoubles;
using ErrorOr;
using Shouldly;

namespace Conduit.Articles.Application.UnitTests.Queries.ArticleList;

public class ArticleListHandlerTests
{
    private readonly FakeArticlesReadRepository _articles = new();
    private readonly FakeProfileReader _profiles = new();

    private Task<ErrorOr<ArticleListReadModel>> List(
        string? tag = null,
        string? author = null,
        string? favoritedBy = null,
        int limit = 20,
        int offset = 0,
        string? viewer = null) =>
        new ArticleListHandler(_articles, _profiles, new StubCurrentUserAccessor(viewer))
            .Handle(
                new ArticleListQuery { Tag = tag, Author = author, FavoritedBy = favoritedBy, Limit = limit, Offset = offset },
                CancellationToken.None);

    [Fact]
    public async Task Listing_articles_resolves_every_author_in_one_lookup()
    {
        _articles.Seed("first", "alice");
        _articles.Seed("second", "bob");
        _profiles.Seed("alice", bio: "I write");
        _profiles.Seed("bob");

        var result = await List();

        result.IsError.ShouldBeFalse();
        result.Value.Articles.Count.ShouldBe(2);
        result.Value.Articles.First(article => article.Slug == "first").Author.Bio.ShouldBe("I write");
    }

    [Fact]
    public async Task Filtering_by_tag_author_and_favorites_is_passed_on_to_storage()
    {
        _articles.Seed("first", "alice", ["dragons"], favoritedBy: ["bob"]);
        _articles.Seed("second", "carol");
        _profiles.Seed("alice");

        var result = await List(tag: "dragons", author: "alice", favoritedBy: "bob");

        result.Value.Articles.ShouldHaveSingleItem().Slug.ShouldBe("first");
        _articles.LastFilter!.Tag.ShouldBe("dragons");
        _articles.LastFilter.Author.ShouldBe("alice");
        _articles.LastFilter.FavoritedBy.ShouldBe("bob");
    }

    [Fact]
    public async Task The_total_count_covers_the_whole_filter_not_just_the_current_page()
    {
        _articles.Seed("first", "alice");
        _articles.Seed("second", "alice");
        _articles.Seed("third", "alice");
        _profiles.Seed("alice");

        var result = await List(limit: 2);

        result.Value.Articles.Count.ShouldBe(2);
        result.Value.ArticlesCount.ShouldBe(3);
    }

    [Fact]
    public async Task A_filter_that_matches_nothing_returns_an_empty_list()
    {
        _articles.Seed("first", "alice", ["dragons"]);

        var result = await List(tag: "unicorns");

        result.IsError.ShouldBeFalse();
        result.Value.Articles.ShouldBeEmpty();
        result.Value.ArticlesCount.ShouldBe(0);
    }
}
