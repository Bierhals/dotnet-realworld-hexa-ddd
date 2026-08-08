using Conduit.Articles.Domain.ValueObjects;
using ErrorOr;
using Shouldly;

namespace Conduit.Articles.Domain.UnitTests.ValueObjects;

public class ArticleTextTests
{
    [Fact]
    public void A_title_is_trimmed_before_it_is_stored()
    {
        ArticleTitle.Create("  How to train  ").Value.Value.ShouldBe("How to train");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void An_article_needs_a_title(string title)
    {
        var result = ArticleTitle.Create(title);

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Validation);
    }

    [Fact]
    public void A_title_that_exceeds_the_column_length_is_rejected()
    {
        ArticleTitle.Create(new string('a', 256)).IsError.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void An_article_needs_a_description(string description)
    {
        ArticleDescription.Create(description).IsError.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void An_article_needs_a_body(string body)
    {
        ArticleBody.Create(body).IsError.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void A_comment_needs_a_body(string body)
    {
        CommentBody.Create(body).IsError.ShouldBeTrue();
    }

    [Fact]
    public void A_tag_name_is_trimmed_and_must_not_be_empty()
    {
        TagName.Create("  dragons ").Value.Value.ShouldBe("dragons");
        TagName.Create("   ").IsError.ShouldBeTrue();
        TagName.Create(new string('a', 65)).IsError.ShouldBeTrue();
    }

    [Fact]
    public void Two_tag_names_with_the_same_text_are_the_same_tag()
    {
        TagName.Create("dragons").Value.ShouldBe(TagName.Create("dragons").Value);
    }
}
