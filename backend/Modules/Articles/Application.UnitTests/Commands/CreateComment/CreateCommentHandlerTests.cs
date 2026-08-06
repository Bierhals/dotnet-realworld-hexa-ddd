using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application.Commands.CreateComment;
using Conduit.Articles.Application.UnitTests.TestDoubles;
using ErrorOr;
using Shouldly;

namespace Conduit.Articles.Application.UnitTests.Commands.CreateComment;

public class CreateCommentHandlerTests
{
    private readonly FakeArticlesRepository _articles = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeCommentNumberGenerator _commentNumbers = new();
    private readonly FakeProfileReader _profiles = new();

    private Task<ErrorOr<CommentReadModel>> Comment(string slug, string? author = "bob", string body = "nice one") =>
        new CreateCommentHandler(_articles, _unitOfWork, _commentNumbers, _profiles, new StubCurrentUserAccessor(author))
            .Handle(new CreateCommentCommand { Slug = slug, Body = body }, CancellationToken.None);

    [Fact]
    public async Task A_comment_is_numbered_by_the_database_and_returned_with_its_author()
    {
        _articles.Seed();
        _profiles.Seed("bob", bio: "I comment");

        var result = await Comment("how-to-train-your-dragon");

        result.IsError.ShouldBeFalse();
        result.Value.Id.ShouldBe(1);
        result.Value.Body.ShouldBe("nice one");
        result.Value.Author.Username.ShouldBe("bob");
        result.Value.Author.Bio.ShouldBe("I comment");
        _unitOfWork.SaveCount.ShouldBe(1);
    }

    [Fact]
    public async Task Every_comment_gets_the_next_number_from_the_sequence()
    {
        _articles.Seed();
        _articles.Seed("Another dragon story", "alice");

        var first = await Comment("how-to-train-your-dragon");
        var second = await Comment("how-to-train-your-dragon");
        var third = await Comment("another-dragon-story");

        first.Value.Id.ShouldBe(1);
        second.Value.Id.ShouldBe(2);
        third.Value.Id.ShouldBe(3);
    }

    [Fact]
    public async Task An_author_without_a_profile_still_gets_a_readable_comment()
    {
        _articles.Seed();

        var result = await Comment("how-to-train-your-dragon");

        result.Value.Author.Username.ShouldBe("bob");
        result.Value.Author.Bio.ShouldBeNull();
    }

    [Fact]
    public async Task An_empty_comment_is_rejected()
    {
        _articles.Seed();

        var result = await Comment("how-to-train-your-dragon", body: "   ");

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Validation);
        _unitOfWork.SaveCount.ShouldBe(0);
    }

    [Fact]
    public async Task Commenting_on_an_article_that_does_not_exist_reports_it_as_missing()
    {
        var result = await Comment("no-such-article");

        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
    }

    [Fact]
    public async Task Commenting_requires_being_signed_in()
    {
        _articles.Seed();

        var result = await Comment("how-to-train-your-dragon", author: null);

        result.FirstError.Type.ShouldBe(ErrorType.Unauthorized);
    }
}
