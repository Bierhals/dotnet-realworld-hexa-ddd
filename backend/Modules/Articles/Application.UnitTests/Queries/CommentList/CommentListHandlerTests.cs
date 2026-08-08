using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application.Queries.CommentList;
using Conduit.Articles.Application.UnitTests.TestDoubles;
using ErrorOr;
using Shouldly;

namespace Conduit.Articles.Application.UnitTests.Queries.CommentList;

public class CommentListHandlerTests
{
    private readonly FakeArticlesReadRepository _articles = new();
    private readonly FakeProfileReader _profiles = new();

    private Task<ErrorOr<IReadOnlyCollection<CommentReadModel>>> Comments(string slug, string? viewer = null) =>
        new CommentListHandler(_articles, _profiles, new StubCurrentUserAccessor(viewer))
            .Handle(new CommentListQuery { Slug = slug }, CancellationToken.None);

    [Fact]
    public async Task Comments_are_returned_with_their_authors_resolved()
    {
        _articles.Seed("how-to-train-your-dragon", "alice");
        _articles.SeedComment("how-to-train-your-dragon", 1, "bob", "first");
        _articles.SeedComment("how-to-train-your-dragon", 2, "carol", "second");
        _profiles.Seed("bob", bio: "I comment");
        _profiles.Seed("carol");

        var result = await Comments("how-to-train-your-dragon");

        result.IsError.ShouldBeFalse();
        result.Value.Count.ShouldBe(2);
        result.Value.First().Author.Bio.ShouldBe("I comment");
    }

    [Fact]
    public async Task An_article_nobody_commented_on_returns_an_empty_list()
    {
        _articles.Seed("how-to-train-your-dragon", "alice");

        var result = await Comments("how-to-train-your-dragon");

        result.IsError.ShouldBeFalse();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task Comments_on_an_article_that_does_not_exist_are_reported_as_missing()
    {
        var result = await Comments("no-such-article");

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
    }

    [Fact]
    public async Task A_reader_sees_whether_they_follow_a_commenter()
    {
        _articles.Seed("how-to-train-your-dragon", "alice");
        _articles.SeedComment("how-to-train-your-dragon", 1, "bob");
        _profiles.Seed("bob");
        _profiles.Follows("carol", "bob");

        var result = await Comments("how-to-train-your-dragon", viewer: "carol");

        result.Value.ShouldHaveSingleItem().Author.Following.ShouldBeTrue();
    }
}
