using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application.Queries.ArticleFeed;
using Conduit.Articles.Application.UnitTests.TestDoubles;
using ErrorOr;
using Shouldly;

namespace Conduit.Articles.Application.UnitTests.Queries.ArticleFeed;

public class ArticleFeedHandlerTests
{
    private readonly FakeArticlesReadRepository _articles = new();
    private readonly FakeProfileReader _profiles = new();

    private Task<ErrorOr<ArticleListReadModel>> Feed(string? reader = "bob", int limit = 20) =>
        new ArticleFeedHandler(_articles, _profiles, new StubCurrentUserAccessor(reader))
            .Handle(new ArticleFeedQuery { Limit = limit, Offset = 0 }, CancellationToken.None);

    [Fact]
    public async Task A_feed_only_contains_articles_by_the_authors_the_reader_follows()
    {
        _articles.Seed("by-alice", "alice");
        _articles.Seed("by-carol", "carol");
        _profiles.Seed("alice");
        _profiles.Seed("carol");
        _profiles.Follows("bob", "alice");

        var result = await Feed();

        result.IsError.ShouldBeFalse();
        result.Value.Articles.ShouldHaveSingleItem().Slug.ShouldBe("by-alice");
    }

    [Fact]
    public async Task A_reader_who_follows_nobody_gets_an_empty_feed()
    {
        _articles.Seed("by-alice", "alice");
        _profiles.Seed("alice");

        var result = await Feed();

        result.IsError.ShouldBeFalse();
        result.Value.Articles.ShouldBeEmpty();
        result.Value.ArticlesCount.ShouldBe(0);
    }

    [Fact]
    public async Task A_feed_requires_being_signed_in()
    {
        var result = await Feed(reader: null);

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Unauthorized);
    }
}
