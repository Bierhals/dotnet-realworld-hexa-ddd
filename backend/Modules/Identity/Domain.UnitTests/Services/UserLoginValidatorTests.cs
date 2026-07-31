using Conduit.Identity.Domain.Services;
using Conduit.Identity.Domain.UnitTests.TestDoubles;
using Conduit.Identity.Domain.ValueObjects;
using Shouldly;

namespace Conduit.Identity.Domain.UnitTests.Services;

public class UserLoginValidatorTests
{
    [Fact]
    public void A_login_with_the_correct_password_is_valid()
    {
        var hasher = new FakePasswordHasher();
        var user = RegisterUser(hasher, "correct-password");
        var validator = new UserLoginValidator(hasher);

        var result = validator.Validate(user, "correct-password");

        result.IsError.ShouldBeFalse();
    }

    [Fact]
    public void A_login_with_the_wrong_password_is_rejected()
    {
        var hasher = new FakePasswordHasher();
        var user = RegisterUser(hasher, "correct-password");
        var validator = new UserLoginValidator(hasher);

        var result = validator.Validate(user, "wrong-password");

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("User.InvalidCredentials");
    }

    private static User RegisterUser(FakePasswordHasher hasher, string plainPassword) =>
        User.RegisterNewUser(UserEmail.Create("jake@jake.jake").Value, Username.Create("jake").Value, hasher.Hash(plainPassword));
}
