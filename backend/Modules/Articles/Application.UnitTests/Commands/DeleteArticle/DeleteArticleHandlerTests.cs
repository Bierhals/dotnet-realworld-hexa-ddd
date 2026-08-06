using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application.Commands.DeleteArticle;
using Conduit.Articles.Application.UnitTests.TestDoubles;
using Conduit.Articles.Domain;
using Conduit.Articles.Domain.ValueObjects;
using ErrorOr;
using Shouldly;

namespace Conduit.Articles.Application.UnitTests.Commands.DeleteArticle;

public class DeleteArticleHandlerTests
{
    private static readonly DateTime CommentedAt = new(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeArticlesRepository _articles = new();
    private readonly FakeCommentsRepository _comments = new();
    private readonly FakeArticleFavoritesRepository _favorites = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeTagCatalog _tagCatalog = new();

    private Task<ErrorOr<Success>> Delete(string slug, string? requester = "alice") =>
        new DeleteArticleHandler(_articles, _comments, _favorites, _unitOfWork, _tagCatalog, new StubCurrentUserAccessor(requester))
            .Handle(new DeleteArticleCommand { Slug = slug }, CancellationToken.None);

    [Fact]
    public async Task Deleting_your_own_article_removes_it()
    {
        _articles.Seed();

        var result = await Delete("how-to-train-your-dragon");

        result.IsError.ShouldBeFalse();
        _articles.Articles.ShouldBeEmpty();
        _unitOfWork.SaveCount.ShouldBe(1);
    }

    [Fact]
    public async Task A_deleted_article_gives_up_the_tags_it_used()
    {
        _articles.Seed("How to train your dragon", "alice", "dragons");
        _tagCatalog.Seed("dragons");

        await Delete("how-to-train-your-dragon");

        _tagCatalog.Tags.ShouldBeEmpty();
    }

    [Fact]
    public async Task A_tag_another_article_still_uses_stays_in_the_catalog()
    {
        _articles.Seed("How to train your dragon", "alice", "dragons");
        _tagCatalog.Seed("dragons");
        _tagCatalog.Seed("dragons");

        await Delete("how-to-train-your-dragon");

        _tagCatalog.ReferenceCountOf("dragons").ShouldBe(1);
    }

    [Fact]
    public async Task A_deleted_article_takes_its_comments_and_favorites_with_it()
    {
        var article = _articles.Seed();
        _comments.Seed(Comment.Post(
            CommentId.From(1),
            article.Id,
            AuthorUsername.Create("bob").Value,
            CommentBody.Create("nice one").Value,
            CommentedAt));
        _favorites.Seed(article.Id, "bob");

        var result = await Delete("how-to-train-your-dragon");

        result.IsError.ShouldBeFalse();
        _comments.Comments.ShouldBeEmpty();
        _favorites.Favorites.ShouldBeEmpty();
    }

    [Fact]
    public async Task Deleting_an_article_leaves_another_articles_comments_and_favorites_alone()
    {
        var article = _articles.Seed();
        var other = _articles.Seed("Another dragon story", "alice");
        _comments.Seed(Comment.Post(
            CommentId.From(1),
            other.Id,
            AuthorUsername.Create("bob").Value,
            CommentBody.Create("elsewhere").Value,
            CommentedAt));
        _favorites.Seed(other.Id, "bob");

        await Delete("how-to-train-your-dragon");

        _comments.Comments.ShouldHaveSingleItem();
        _favorites.CountFor(other.Id).ShouldBe(1);
    }

    [Fact]
    public async Task An_article_you_do_not_own_cannot_be_deleted()
    {
        _articles.Seed();

        var result = await Delete("how-to-train-your-dragon", requester: "bob");

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Forbidden);
        _articles.Articles.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task Deleting_an_article_that_does_not_exist_reports_it_as_missing()
    {
        var result = await Delete("no-such-article");

        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
    }
}
