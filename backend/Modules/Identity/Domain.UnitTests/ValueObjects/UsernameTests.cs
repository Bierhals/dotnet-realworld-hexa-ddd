using Conduit.Identity.Domain.Rules;
using Conduit.Identity.Domain.ValueObjects;
using Shouldly;

namespace Conduit.Identity.Domain.UnitTests.ValueObjects;

public class UsernameTests
{
    [Fact]
    public void A_username_with_only_letters_numbers_dots_underscores_and_hyphens_is_valid()
    {
        var result = Username.Create("jake_99.the-hacker");

        result.IsError.ShouldBeFalse();
        result.Value.Value.ShouldBe("jake_99.the-hacker");
    }

    [Fact]
    public void Surrounding_whitespace_is_trimmed_from_a_username()
    {
        var result = Username.Create("  jake  ");

        result.Value.Value.ShouldBe("jake");
    }

    [Fact]
    public void A_username_containing_spaces_is_invalid()
    {
        var result = Username.Create("jake the hacker");

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("User.InvalidUsername");
    }

    [Fact]
    public void A_blank_username_is_invalid()
    {
        var result = Username.Create("   ");

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("User.InvalidUsername");
    }

    [Fact]
    public void A_username_longer_than_the_maximum_length_is_invalid()
    {
        var result = Username.Create(new string('a', UsernameLengthIsInRange.MaximumLength + 1));

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("User.UsernameTooLong");
    }

    [Fact]
    public void Usernames_with_the_same_value_are_equal()
    {
        var first = Username.Create("jake").Value;
        var second = Username.Create("jake").Value;

        first.ShouldBe(second);
    }
}
