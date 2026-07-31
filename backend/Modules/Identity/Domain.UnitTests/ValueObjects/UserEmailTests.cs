using Conduit.Identity.Domain.ValueObjects;
using Shouldly;

namespace Conduit.Identity.Domain.UnitTests.ValueObjects;

public class UserEmailTests
{
    [Fact]
    public void A_well_formed_email_address_is_valid()
    {
        var result = UserEmail.Create("jake@jake.jake");

        result.IsError.ShouldBeFalse();
        result.Value.Value.ShouldBe("jake@jake.jake");
    }

    [Fact]
    public void An_email_address_is_normalized_to_trimmed_lowercase()
    {
        var result = UserEmail.Create("  Jake@Jake.JAKE  ");

        result.Value.Value.ShouldBe("jake@jake.jake");
    }

    [Fact]
    public void An_email_address_without_an_at_sign_is_invalid()
    {
        var result = UserEmail.Create("not-an-email");

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("User.InvalidEmail");
    }

    [Fact]
    public void Email_addresses_with_the_same_normalized_value_are_equal()
    {
        var first = UserEmail.Create("Jake@Jake.Jake").Value;
        var second = UserEmail.Create("jake@jake.jake").Value;

        first.ShouldBe(second);
    }
}
