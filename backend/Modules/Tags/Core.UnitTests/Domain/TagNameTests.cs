using Conduit.Tags.Core.Domain;
using ErrorOr;
using Shouldly;

namespace Conduit.Tags.Core.UnitTests.Domain;

public class TagNameTests
{
    [Fact]
    public void Surrounding_whitespace_is_removed_from_a_tag_name()
    {
        var name = TagName.Create("  dragons  ");

        name.IsError.ShouldBeFalse();
        name.Value.Value.ShouldBe("dragons");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void A_tag_name_without_any_visible_characters_is_rejected(string value)
    {
        var name = TagName.Create(value);

        name.IsError.ShouldBeTrue();
        name.FirstError.Type.ShouldBe(ErrorType.Validation);
    }

    [Fact]
    public void A_tag_name_longer_than_the_allowed_maximum_is_rejected()
    {
        var name = TagName.Create(new string('a', 65));

        name.IsError.ShouldBeTrue();
        name.FirstError.Type.ShouldBe(ErrorType.Validation);
    }

    [Fact]
    public void Two_tag_names_with_the_same_text_are_the_same_name()
    {
        var first = TagName.Create("dragons");
        var second = TagName.Create("dragons");

        first.Value.ShouldBe(second.Value);
    }
}
