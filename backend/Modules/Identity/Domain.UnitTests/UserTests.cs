using Conduit.Identity.Domain.Events;
using Conduit.Identity.Domain.ValueObjects;
using Shouldly;

namespace Conduit.Identity.Domain.UnitTests;

public class UserTests
{
    [Fact]
    public void Registering_a_new_user_sets_its_profile_data()
    {
        var email = UserEmail.Create("jake@jake.jake").Value;
        var username = Username.Create("jake").Value;

        var user = User.RegisterNewUser(email, username, "hashed-password");

        user.Email.ShouldBe(email);
        user.Username.ShouldBe(username);
        user.PasswordHash.ShouldBe("hashed-password");
        user.Bio.ShouldBeNull();
        user.Image.ShouldBeNull();
    }

    [Fact]
    public void Registering_a_new_user_raises_a_new_user_registered_event()
    {
        var email = UserEmail.Create("jake@jake.jake").Value;
        var username = Username.Create("jake").Value;

        var user = User.RegisterNewUser(email, username, "hashed-password");

        var raisedEvent = user.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<NewUserRegisteredDomainEvent>();
        raisedEvent.Email.ShouldBe(email.Value);
        raisedEvent.Username.ShouldBe(username.Value);
    }

    [Fact]
    public void Changing_the_password_updates_the_password_hash_and_raises_an_event()
    {
        var user = RegisterUser();
        user.ClearDomainEvents();

        user.ChangePassword("new-hashed-password");

        user.PasswordHash.ShouldBe("new-hashed-password");
        var raisedEvent = user.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<PasswordChangedDomainEvent>();
        raisedEvent.Username.ShouldBe(user.Username.Value);
    }

    [Fact]
    public void Changing_to_a_different_username_updates_it_and_raises_an_event()
    {
        var user = RegisterUser(username: "jake");
        user.ClearDomainEvents();
        var newUsername = Username.Create("jakey").Value;

        var result = user.ChangeUsername(newUsername);

        result.IsError.ShouldBeFalse();
        user.Username.ShouldBe(newUsername);
        var raisedEvent = user.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<UsernameChangedDomainEvent>();
        raisedEvent.PreviousUsername.ShouldBe("jake");
        raisedEvent.Username.ShouldBe("jakey");
    }

    [Fact]
    public void Changing_the_username_to_the_current_one_is_rejected()
    {
        var user = RegisterUser(username: "jake");
        var sameUsername = Username.Create("jake").Value;

        var result = user.ChangeUsername(sameUsername);

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("User.UsernameNotChanged");
        user.Username.ShouldBe(sameUsername);
    }

    [Fact]
    public void Changing_to_a_different_email_updates_it_and_raises_an_event()
    {
        var user = RegisterUser(email: "jake@jake.jake");
        user.ClearDomainEvents();
        var newEmail = UserEmail.Create("jake@newmail.com").Value;

        var result = user.ChangeEmail(newEmail);

        result.IsError.ShouldBeFalse();
        user.Email.ShouldBe(newEmail);
        var raisedEvent = user.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<EmailChangedDomainEvent>();
        raisedEvent.Username.ShouldBe(user.Username.Value);
        raisedEvent.Email.ShouldBe("jake@newmail.com");
    }

    [Fact]
    public void Changing_the_email_to_the_current_one_is_rejected()
    {
        var user = RegisterUser(email: "jake@jake.jake");
        var sameEmail = UserEmail.Create("jake@jake.jake").Value;

        var result = user.ChangeEmail(sameEmail);

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("User.EmailNotChanged");
        user.Email.ShouldBe(sameEmail);
    }

    [Fact]
    public void Changing_the_bio_sets_the_new_value_and_raises_an_event()
    {
        var user = RegisterUser();
        user.ClearDomainEvents();

        user.ChangeBio("I work at statefarm");

        user.Bio.ShouldBe("I work at statefarm");
        var raisedEvent = user.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<UserBioChangedDomainEvent>();
        raisedEvent.Bio.ShouldBe("I work at statefarm");
    }

    [Fact]
    public void Changing_the_bio_to_blank_text_clears_it()
    {
        var user = RegisterUser();
        user.ChangeBio("I work at statefarm");

        user.ChangeBio("   ");

        user.Bio.ShouldBeNull();
    }

    [Fact]
    public void Changing_the_image_sets_the_new_value_and_raises_an_event()
    {
        var user = RegisterUser();
        user.ClearDomainEvents();
        var image = UserImage.Create("https://example.com/avatar.png").Value;

        user.ChangeImage(image);

        user.Image.ShouldBe(image);
        var raisedEvent = user.DomainEvents.ShouldHaveSingleItem().ShouldBeOfType<UserImageChangedDomainEvent>();
        raisedEvent.Image.ShouldBe(image.Value);
    }

    [Fact]
    public void Clearing_the_image_removes_the_previous_value()
    {
        var user = RegisterUser();
        user.ChangeImage(UserImage.Create("https://example.com/avatar.png").Value);

        user.ChangeImage(null);

        user.Image.ShouldBeNull();
    }

    private static User RegisterUser(string email = "jake@jake.jake", string username = "jake") =>
        User.RegisterNewUser(UserEmail.Create(email).Value, Username.Create(username).Value, "hashed-password");
}
