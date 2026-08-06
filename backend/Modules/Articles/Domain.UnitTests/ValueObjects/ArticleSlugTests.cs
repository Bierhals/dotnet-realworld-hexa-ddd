using Conduit.Articles.Domain.ValueObjects;
using Shouldly;

namespace Conduit.Articles.Domain.UnitTests.ValueObjects;

public class ArticleSlugTests
{
    private static string SlugFor(string title) =>
        ArticleSlug.FromTitle(ArticleTitle.Create(title).Value).Value;

    [Fact]
    public void A_title_becomes_a_lower_case_hyphenated_slug()
    {
        SlugFor("How To Train Your Dragon").ShouldBe("how-to-train-your-dragon");
    }

    [Fact]
    public void Characters_that_do_not_belong_in_a_url_are_dropped()
    {
        SlugFor("What?! Dragons & Co.").ShouldBe("what-dragons-co");
    }

    [Fact]
    public void Runs_of_whitespace_collapse_into_a_single_hyphen()
    {
        SlugFor("How   to    train").ShouldBe("how-to-train");
    }

    [Fact]
    public void A_long_title_is_cut_short()
    {
        var slug = SlugFor(new string('a', 60));

        slug.Length.ShouldBe(ArticleSlug.MaximumLength);
    }

    [Fact]
    public void Digits_and_existing_hyphens_survive()
    {
        SlugFor("Dragon-training 101").ShouldBe("dragon-training-101");
    }
}
