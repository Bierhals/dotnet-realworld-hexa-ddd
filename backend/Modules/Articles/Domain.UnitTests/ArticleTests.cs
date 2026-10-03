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
        article.DomainEvents.OfType<ArticlePublishedDomainEvent>().Single().Author.ShouldBe("alice");
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
    public void A_published_article_announces_the_tags_it_starts_using()
    {
        var article = AnArticle("alice", "dragons", "training");

        var changed = article.DomainEvents.OfType<ArticleTagsChangedDomainEvent>().Single();
        changed.Added.ShouldBe(["dragons", "training"], ignoreOrder: true);
        changed.Removed.ShouldBeEmpty();
    }

    [Fact]
    public void A_published_article_without_tags_announces_no_tag_change()
    {
        var article = AnArticle();

        article.DomainEvents.OfType<ArticleTagsChangedDomainEvent>().ShouldBeEmpty();
    }

    [Fact]
    public void Editing_the_tags_announces_which_ones_were_added_and_which_were_given_up()
    {
        var article = AnArticle("alice", "dragons", "training");
        article.ClearDomainEvents();

        var result = article.Edit(User("alice"), null, null, null, [Tag("dragons"), Tag("flying")], EditedAt);

        result.IsError.ShouldBeFalse();
        var changed = article.DomainEvents.OfType<ArticleTagsChangedDomainEvent>().Single();
        changed.Added.ShouldHaveSingleItem().ShouldBe("flying");
        changed.Removed.ShouldHaveSingleItem().ShouldBe("training");
        article.Tags.Select(tag => tag.Value).ShouldBe(["dragons", "flying"], ignoreOrder: true);
    }

    [Fact]
    public void Leaving_the_tags_out_of_an_edit_keeps_them_and_announces_nothing()
    {
        var article = AnArticle("alice", "dragons");
        article.ClearDomainEvents();

        article.Edit(User("alice"), null, null, ArticleBody.Create("Believe harder").Value, null, EditedAt);

        article.DomainEvents.OfType<ArticleTagsChangedDomainEvent>().ShouldBeEmpty();
        article.Tags.ShouldHaveSingleItem().Value.ShouldBe("dragons");
    }

    [Fact]
    public void Resubmitting_the_same_tags_announces_nothing()
    {
        var article = AnArticle("alice", "dragons");
        article.ClearDomainEvents();

        article.Edit(User("alice"), null, null, null, [Tag("dragons")], EditedAt);

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
    public void A_deleted_article_announces_that_it_gives_up_all_its_tags()
    {
        var article = AnArticle("alice", "dragons", "training");
        article.ClearDomainEvents();

        article.Delete(User("alice"));

        var changed = article.DomainEvents.OfType<ArticleTagsChangedDomainEvent>().Single();
        changed.Added.ShouldBeEmpty();
        changed.Removed.ShouldBe(["dragons", "training"], ignoreOrder: true);
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
