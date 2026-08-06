using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application.Commands.DeleteComment;
using Conduit.Articles.Application.UnitTests.TestDoubles;
using Conduit.Articles.Domain;
using Conduit.Articles.Domain.ValueObjects;
using ErrorOr;
using Shouldly;

namespace Conduit.Articles.Application.UnitTests.Commands.DeleteComment;

public class DeleteCommentHandlerTests
{
    private static readonly System.DateTime CommentedAt = new(2026, 1, 2, 12, 0, 0, System.DateTimeKind.Utc);

    private readonly FakeArticlesRepository _articles = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private Task<ErrorOr<Success>> Delete(string slug, int commentId, string? requester = "bob") =>
        new DeleteCommentHandler(_articles, _unitOfWork, new StubCurrentUserAccessor(requester))
            .Handle(new DeleteCommentCommand { Slug = slug, CommentId = commentId }, CancellationToken.None);

    private Article AnArticleCommentedOnBy(string commenter)
    {
        var article = _articles.Seed();
        article.AddComment(
            CommentId.From(1),
            AuthorUsername.Create(commenter).Value,
            CommentBody.Create("nice one").Value,
            CommentedAt);
        article.ClearDomainEvents();

        return article;
    }

    [Fact]
    public async Task Deleting_your_own_comment_removes_it()
    {
        var article = AnArticleCommentedOnBy("bob");

        var result = await Delete("how-to-train-your-dragon", 1);

        result.IsError.ShouldBeFalse();
        article.Comments.ShouldBeEmpty();
        _unitOfWork.SaveCount.ShouldBe(1);
    }

    [Fact]
    public async Task A_comment_you_did_not_write_cannot_be_deleted()
    {
        var article = AnArticleCommentedOnBy("bob");

        var result = await Delete("how-to-train-your-dragon", 1, requester: "alice");

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Forbidden);
        article.Comments.ShouldHaveSingleItem();
        _unitOfWork.SaveCount.ShouldBe(0);
    }

    [Fact]
    public async Task Deleting_a_comment_that_does_not_exist_reports_it_as_missing()
    {
        AnArticleCommentedOnBy("bob");

        var result = await Delete("how-to-train-your-dragon", 99);

        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
    }

    [Fact]
    public async Task Deleting_a_comment_on_an_article_that_does_not_exist_reports_it_as_missing()
    {
        var result = await Delete("no-such-article", 1);

        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
    }
}
