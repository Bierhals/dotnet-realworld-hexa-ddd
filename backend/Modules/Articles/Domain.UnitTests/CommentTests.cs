using System;
using System.Linq;
using Conduit.Articles.Domain;
using Conduit.Articles.Domain.Events;
using Conduit.Articles.Domain.ValueObjects;
using ErrorOr;
using Shouldly;

namespace Conduit.Articles.Domain.UnitTests;

public class CommentTests
{
    private static readonly DateTime PostedAt = new(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc);
    private static readonly ArticleId Article = ArticleId.New();

    private static Username User(string name) => Username.Create(name).Value;

    private static Comment AComment(string author = "bob", int number = 1) =>
        Comment.Post(
            CommentId.From(number),
            Article,
            User(author),
            CommentBody.Create("nice one").Value,
            PostedAt);

    [Fact]
    public void A_posted_comment_keeps_the_number_it_was_handed_and_names_the_article_it_belongs_to()
    {
        var comment = AComment(number: 7);

        comment.Id.Value.ShouldBe(7);
        comment.ArticleId.ShouldBe(Article);
        comment.Author.Value.ShouldBe("bob");
        comment.CreatedAt.ShouldBe(PostedAt);
        comment.UpdatedAt.ShouldBe(PostedAt);
        comment.DomainEvents.OfType<CommentAddedDomainEvent>().Single().CommentId.ShouldBe(7);
    }

    [Fact]
    public void A_comment_only_belongs_to_the_article_it_was_written_on()
    {
        var comment = AComment();

        comment.BelongsTo(Article).ShouldBeTrue();
        comment.BelongsTo(ArticleId.New()).ShouldBeFalse();
    }

    [Fact]
    public void A_comment_can_only_be_deleted_by_its_author()
    {
        var comment = AComment("bob");

        comment.EnsureCanBeDeletedBy(User("alice")).FirstError.Type.ShouldBe(ErrorType.Forbidden);
        comment.EnsureCanBeDeletedBy(User("bob")).IsError.ShouldBeFalse();
    }

    [Fact]
    public void A_deleted_comment_announces_that_it_is_gone()
    {
        var comment = AComment(number: 3);
        comment.ClearDomainEvents();

        comment.Delete();

        var deleted = comment.DomainEvents.OfType<CommentDeletedDomainEvent>().Single();
        deleted.CommentId.ShouldBe(3);
        deleted.ArticleId.ShouldBe(Article.Value);
    }
}
