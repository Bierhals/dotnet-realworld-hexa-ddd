using System;
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
    private static readonly DateTime CommentedAt = new(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeArticlesRepository _articles = new();
    private readonly FakeCommentsRepository _comments = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private Task<ErrorOr<Success>> Delete(string slug, int commentId, string? requester = "bob") =>
        new DeleteCommentHandler(_articles, _comments, _unitOfWork, new StubCurrentUserAccessor(requester))
            .Handle(new DeleteCommentCommand { Slug = slug, CommentId = commentId }, CancellationToken.None);

    private Article AnArticleCommentedOnBy(string commenter, int commentNumber = 1)
    {
        var article = _articles.Seed();
        _comments.Seed(Comment.Post(
            CommentId.From(commentNumber),
            article.Id,
            Username.Create(commenter).Value,
            CommentBody.Create("nice one").Value,
            CommentedAt));

        return article;
    }

    [Fact]
    public async Task Deleting_your_own_comment_removes_it()
    {
        AnArticleCommentedOnBy("bob");

        var result = await Delete("how-to-train-your-dragon", 1);

        result.IsError.ShouldBeFalse();
        _comments.Comments.ShouldBeEmpty();
        _unitOfWork.SaveCount.ShouldBe(1);
    }

    [Fact]
    public async Task A_comment_you_did_not_write_cannot_be_deleted()
    {
        AnArticleCommentedOnBy("bob");

        var result = await Delete("how-to-train-your-dragon", 1, requester: "alice");

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Forbidden);
        _comments.Comments.ShouldHaveSingleItem();
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
    public async Task A_comment_written_on_another_article_cannot_be_deleted_through_this_one()
    {
        AnArticleCommentedOnBy("bob");
        var otherArticle = _articles.Seed("Another dragon story", "alice");
        _comments.Seed(Comment.Post(
            CommentId.From(2),
            otherArticle.Id,
            Username.Create("bob").Value,
            CommentBody.Create("elsewhere").Value,
            CommentedAt));

        var result = await Delete("how-to-train-your-dragon", 2);

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
        _comments.Comments.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Deleting_a_comment_on_an_article_that_does_not_exist_reports_it_as_missing()
    {
        var result = await Delete("no-such-article", 1);

        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
    }
}
