using Conduit.Identity.Domain.ValueObjects;
using Shouldly;

namespace Conduit.Identity.Domain.UnitTests.ValueObjects;

public class UserImageTests
{
    [Fact]
    public void An_absolute_url_is_a_valid_user_image()
    {
        var result = UserImage.Create("https://example.com/avatar.png");

        result.IsError.ShouldBeFalse();
        result.Value.Value.ShouldBe("https://example.com/avatar.png");
    }

    [Fact]
    public void Surrounding_whitespace_is_trimmed_from_a_user_image_url()
    {
        var result = UserImage.Create("  https://example.com/avatar.png  ");

        result.Value.Value.ShouldBe("https://example.com/avatar.png");
    }

    [Fact]
    public void A_relative_path_is_not_a_valid_user_image()
    {
        var result = UserImage.Create("/avatar.png");

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("UserImage.InvalidImage");
    }
}
