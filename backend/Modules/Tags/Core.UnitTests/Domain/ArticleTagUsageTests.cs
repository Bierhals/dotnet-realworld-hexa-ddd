using System;
using System.Linq;
using Conduit.Tags.Core.Domain;
using Shouldly;

namespace Conduit.Tags.Core.UnitTests.Domain;

public class ArticleTagUsageTests
{
    private static TagName[] Names(params string[] names) => [.. names.Select(TagName.Rehydrate)];

    private static ArticleTagUsage AUsage() => ArticleTagUsage.Start(Guid.NewGuid());

    [Fact]
    public void The_first_state_of_an_article_adds_all_its_tags()
    {
        var usage = AUsage();

        var change = usage.Update(1, Names("dragons", "training"));

        change.ShouldNotBeNull();
        change.Added.Select(name => name.Value).ShouldBe(["dragons", "training"]);
        change.Removed.ShouldBeEmpty();
        usage.Revision.ShouldBe(1);
    }

    [Fact]
    public void A_newer_state_reports_only_what_differs_from_the_known_one()
    {
        var usage = AUsage();
        usage.Update(1, Names("dragons", "training"));

        var change = usage.Update(2, Names("dragons", "flying"));

        change.ShouldNotBeNull();
        change.Added.ShouldHaveSingleItem().Value.ShouldBe("flying");
        change.Removed.ShouldHaveSingleItem().Value.ShouldBe("training");
        usage.Tags.Select(name => name.Value).ShouldBe(["dragons", "flying"], ignoreOrder: true);
    }

    [Fact]
    public void A_tag_listed_twice_counts_once()
    {
        var change = AUsage().Update(1, Names("dragons", "dragons"));

        change!.Added.ShouldHaveSingleItem();
    }

    [Fact]
    public void A_state_that_is_not_newer_changes_nothing()
    {
        var usage = AUsage();
        usage.Update(2, Names("dragons"));

        usage.Update(2, Names("dragons", "flying")).ShouldBeNull();
        usage.Update(1, Names("training")).ShouldBeNull();

        usage.Tags.ShouldHaveSingleItem().Value.ShouldBe("dragons");
        usage.Revision.ShouldBe(2);
    }

    [Fact]
    public void A_deletion_gives_up_all_tags_and_stays_on_record()
    {
        var usage = AUsage();
        usage.Update(1, Names("dragons", "training"));

        var change = usage.Delete(2);

        change.ShouldNotBeNull();
        change.Removed.Select(name => name.Value).ShouldBe(["dragons", "training"], ignoreOrder: true);
        usage.IsDeleted.ShouldBeTrue();
        usage.Tags.ShouldBeEmpty();
    }

    [Fact]
    public void An_older_state_arriving_after_the_deletion_does_not_bring_the_article_back()
    {
        var usage = AUsage();
        usage.Update(1, Names("dragons"));
        usage.Delete(3);

        usage.Update(2, Names("dragons", "flying")).ShouldBeNull();

        usage.Tags.ShouldBeEmpty();
    }

    [Fact]
    public void A_deletion_that_arrives_before_anything_else_still_blocks_the_older_states()
    {
        var usage = AUsage();

        var deletion = usage.Delete(3);
        var late = usage.Update(1, Names("dragons"));

        deletion.ShouldNotBeNull();
        deletion.Removed.ShouldBeEmpty();
        late.ShouldBeNull();
    }

    [Fact]
    public void A_deletion_that_is_repeated_changes_nothing()
    {
        var usage = AUsage();
        usage.Update(1, Names("dragons"));
        usage.Delete(2);

        usage.Delete(2).ShouldBeNull();
    }
}
