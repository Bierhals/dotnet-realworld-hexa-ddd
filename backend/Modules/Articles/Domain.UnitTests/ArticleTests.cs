using System;
using System.Collections.Generic;
using System.Linq;
using Conduit.Articles.Domain;
using Conduit.Articles.Domain.Events;
using Conduit.Articles.Domain.ValueObjects;
using ErrorOr;
using Shouldly;

namespace Conduit.Articles.Domain.UnitTests;

public class ArticleTests
{
    private static readonly DateTime PublishedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime EditedAt = new(2026, 2, 2, 12, 0, 0, DateTimeKind.Utc);

    private static Username User(string name) => Username.Create(name).Value;

    private static TagName Tag(string name) => TagName.Create(name).Value;

    private static Article AnArticle(string author = "alice", params string[] tags) =>
        Article.Publish(
            User(author),
            ArticleTitle.Create("How to train your dragon").Value,
            ArticleDescription.Create("Ever wonder how?").Value,
            ArticleBody.Create("You have to believe").Value,
            [.. tags.Select(Tag)],
            PublishedAt);

    [Fact]
    public void A_published_article_derives_its_slug_from_its_title()
    {
        var article = AnArticle();

        article.Slug.Value.ShouldBe("how-to-train-your-dragon");
        article.CreatedAtUtc.ShouldBe(PublishedAt);
        article.UpdatedAtUtc.ShouldBe(PublishedAt);
        article.DomainEvents.OfType<ArticlePublishedDomainEvent>().Single().Article.Author.ShouldBe("alice");
    }

    [Fact]
    public void Publishing_the_same_tag_twice_records_it_once()
    {
        var article = AnArticle("alice", "dragons", "dragons");

        article.Tags.ShouldHaveSingleItem().Value.ShouldBe("dragons");
    }

    [Fact]
    public void Renaming_an_article_moves_it_to_a_new_slug()
    {
        var article = AnArticle();

        var result = article.Edit(User("alice"), ArticleTitle.Create("How to tame your dragon").Value, null, null, null, EditedAt);

        result.IsError.ShouldBeFalse();
        article.Slug.Value.ShouldBe("how-to-tame-your-dragon");
        article.UpdatedAtUtc.ShouldBe(EditedAt);
    }

    [Fact]
    public void Fields_that_are_left_out_of_an_edit_keep_the_value_they_had()
    {
        var article = AnArticle();

        article.Edit(User("alice"), null, null, ArticleBody.Create("Believe harder").Value, null, EditedAt);

        article.Body.Value.ShouldBe("Believe harder");
        article.Title.Value.ShouldBe("How to train your dragon");
        article.Description.Value.ShouldBe("Ever wonder how?");
    }

    [Fact]
    public void An_edit_that_changes_nothing_leaves_the_article_untouched()
    {
        var article = AnArticle("alice", "dragons");
        article.ClearDomainEvents();

        var result = article.Edit(User("alice"), null, null, null, null, EditedAt);

        result.IsError.ShouldBeFalse();
        article.UpdatedAtUtc.ShouldBe(PublishedAt);
        article.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void A_published_article_announces_its_state_as_revision_one()
    {
        var article = AnArticle("alice", "dragons", "training");

        var state = article.DomainEvents.OfType<ArticlePublishedDomainEvent>().Single().Article;
        state.ArticleId.ShouldBe(article.Id.Value);
        state.Revision.ShouldBe(1);
        state.Slug.ShouldBe("how-to-train-your-dragon");
        state.Title.ShouldBe("How to train your dragon");
        state.Description.ShouldBe("Ever wonder how?");
        state.Author.ShouldBe("alice");
        state.Tags.ShouldBe(["dragons", "training"], ignoreOrder: true);
        state.CreatedAtUtc.ShouldBe(PublishedAt);
        state.UpdatedAtUtc.ShouldBe(PublishedAt);
    }

    [Fact]
    public void An_edit_announces_the_whole_new_state_under_the_next_revision()
    {
        var article = AnArticle("alice", "dragons", "training");
        article.ClearDomainEvents();

        article.Edit(User("alice"), ArticleTitle.Create("How to tame your dragon").Value, null, null, [Tag("dragons"), Tag("flying")], EditedAt);

        var state = article.DomainEvents.OfType<ArticleEditedDomainEvent>().Single().Article;
        article.Revision.ShouldBe(2);
        state.Revision.ShouldBe(2);
        state.Slug.ShouldBe("how-to-tame-your-dragon");
        state.Title.ShouldBe("How to tame your dragon");
        state.Description.ShouldBe("Ever wonder how?");
        state.Tags.ShouldBe(["dragons", "flying"], ignoreOrder: true);
        state.UpdatedAtUtc.ShouldBe(EditedAt);
        article.Tags.Select(tag => tag.Value).ShouldBe(["dragons", "flying"], ignoreOrder: true);
    }

    [Fact]
    public void An_edit_that_leaves_the_tags_out_keeps_them_in_the_announced_state()
    {
        var article = AnArticle("alice", "dragons");
        article.ClearDomainEvents();

        article.Edit(User("alice"), null, null, ArticleBody.Create("Believe harder").Value, null, EditedAt);

        article.DomainEvents.OfType<ArticleEditedDomainEvent>().Single().Article.Tags.ShouldBe(["dragons"]);
    }

    [Fact]
    public void An_edit_that_changes_nothing_does_not_move_the_revision()
    {
        var article = AnArticle("alice", "dragons");
        article.ClearDomainEvents();

        article.Edit(User("alice"), null, null, null, [Tag("dragons")], EditedAt);

        article.Revision.ShouldBe(1);
        article.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void An_article_can_only_be_edited_by_its_author()
    {
        var article = AnArticle("alice");

        var result = article.Edit(User("bob"), ArticleTitle.Create("Hijacked").Value, null, null, null, EditedAt);

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Forbidden);
        article.Title.Value.ShouldBe("How to train your dragon");
    }

    [Fact]
    public void An_article_can_only_be_deleted_by_its_author()
    {
        var article = AnArticle("alice");

        article.Delete(User("bob")).FirstError.Type.ShouldBe(ErrorType.Forbidden);
        article.Delete(User("alice")).IsError.ShouldBeFalse();
    }

    [Fact]
    public void A_deleted_article_announces_its_deletion_under_a_newer_revision()
    {
        var article = AnArticle("alice", "dragons", "training");
        article.ClearDomainEvents();

        article.Delete(User("alice"));

        var deleted = article.DomainEvents.OfType<ArticleDeletedDomainEvent>().Single();
        deleted.ArticleId.ShouldBe(article.Id.Value);
        deleted.Revision.ShouldBe(2);
    }

    [Fact]
    public void A_refused_delete_announces_nothing()
    {
        var article = AnArticle("alice", "dragons");
        article.ClearDomainEvents();

        article.Delete(User("bob"));

        article.DomainEvents.ShouldBeEmpty();
    }
}
