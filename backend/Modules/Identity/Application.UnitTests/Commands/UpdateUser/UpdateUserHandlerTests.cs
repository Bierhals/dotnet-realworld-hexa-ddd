using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Application.Commands.UpdateUser;
using Conduit.Identity.Application.UnitTests.TestDoubles;
using Conduit.Identity.Domain;
using Conduit.Identity.Domain.ValueObjects;
using Conduit.Shared.Application.Optional;
using Shouldly;

namespace Conduit.Identity.Application.UnitTests.Commands.UpdateUser;

public class UpdateUserHandlerTests
{
    [Fact]
    public async Task Updating_only_the_bio_leaves_other_fields_untouched()
    {
        var user = RegisterUser(email: "jake@jake.jake", username: "jake", password: "hashed-password");
        var usersRepository = new FakeUsersRepository();
        usersRepository.Add(user);
        var handler = CreateHandler(usersRepository, currentUsername: "jake");
        var command = new UpdateUserCommand { Bio = new Optional<string?>("I work at statefarm") };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        user.Bio.ShouldBe("I work at statefarm");
        user.Username.Value.ShouldBe("jake");
        user.Email.Value.ShouldBe("jake@jake.jake");
        user.PasswordHash.ShouldBe("hashed-password");
    }

    [Fact]
    public async Task Updating_the_username_to_a_unique_value_succeeds()
    {
        var user = RegisterUser(email: "jake@jake.jake", username: "jake", password: "hashed-password");
        var usersRepository = new FakeUsersRepository();
        usersRepository.Add(user);
        var handler = CreateHandler(usersRepository, currentUsername: "jake");
        var command = new UpdateUserCommand { Username = new Optional<string>("jakey") };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        user.Username.Value.ShouldBe("jakey");
    }

    [Fact]
    public async Task Updating_the_username_to_one_already_taken_fails()
    {
        var user = RegisterUser(email: "jake@jake.jake", username: "jake", password: "hashed-password");
        var otherUser = RegisterUser(email: "john@jake.jake", username: "john", password: "hashed-password");
        var usersRepository = new FakeUsersRepository();
        usersRepository.Add(user);
        usersRepository.Add(otherUser);
        var handler = CreateHandler(usersRepository, currentUsername: "jake");
        var command = new UpdateUserCommand { Username = new Optional<string>("john") };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("User.NotUniqueUsername");
        user.Username.Value.ShouldBe("jake");
    }

    [Fact]
    public async Task Updating_the_email_to_one_already_taken_fails()
    {
        var user = RegisterUser(email: "jake@jake.jake", username: "jake", password: "hashed-password");
        var otherUser = RegisterUser(email: "john@jake.jake", username: "john", password: "hashed-password");
        var usersRepository = new FakeUsersRepository();
        usersRepository.Add(user);
        usersRepository.Add(otherUser);
        var handler = CreateHandler(usersRepository, currentUsername: "jake");
        var command = new UpdateUserCommand { Email = new Optional<string>("john@jake.jake") };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("User.NotUniqueEmail");
        user.Email.Value.ShouldBe("jake@jake.jake");
    }

    [Fact]
    public async Task Updating_the_password_stores_a_newly_hashed_value()
    {
        var user = RegisterUser(email: "jake@jake.jake", username: "jake", password: "hashed-password");
        var usersRepository = new FakeUsersRepository();
        usersRepository.Add(user);
        var handler = CreateHandler(usersRepository, currentUsername: "jake");
        var command = new UpdateUserCommand { Password = new Optional<string>("new-password") };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        user.PasswordHash.ShouldBe("hashed:new-password");
    }

    [Fact]
    public async Task Setting_the_image_to_null_clears_the_previous_image()
    {
        var user = RegisterUser(email: "jake@jake.jake", username: "jake", password: "hashed-password");
        user.ChangeImage(UserImage.Create("https://example.com/avatar.png").Value);
        var usersRepository = new FakeUsersRepository();
        usersRepository.Add(user);
        var handler = CreateHandler(usersRepository, currentUsername: "jake");
        var command = new UpdateUserCommand { Image = new Optional<string?>(null) };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        user.Image.ShouldBeNull();
    }

    [Fact]
    public async Task Setting_the_image_to_an_invalid_url_fails_validation()
    {
        var user = RegisterUser(email: "jake@jake.jake", username: "jake", password: "hashed-password");
        var usersRepository = new FakeUsersRepository();
        usersRepository.Add(user);
        var handler = CreateHandler(usersRepository, currentUsername: "jake");
        var command = new UpdateUserCommand { Image = new Optional<string?>("not-a-url") };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        user.Image.ShouldBeNull();
    }

    [Fact]
    public async Task Fields_left_unspecified_are_not_changed()
    {
        var user = RegisterUser(email: "jake@jake.jake", username: "jake", password: "hashed-password");
        var usersRepository = new FakeUsersRepository();
        usersRepository.Add(user);
        var handler = CreateHandler(usersRepository, currentUsername: "jake");
        var command = new UpdateUserCommand();

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        user.Username.Value.ShouldBe("jake");
        user.Email.Value.ShouldBe("jake@jake.jake");
        user.PasswordHash.ShouldBe("hashed-password");
        user.Bio.ShouldBeNull();
        user.Image.ShouldBeNull();
    }

    private static UpdateUserHandler CreateHandler(FakeUsersRepository usersRepository, string? currentUsername) =>
        new(new StubCurrentUserAccessor(currentUsername), new FakeUnitOfWork(), usersRepository, new(usersRepository), new(usersRepository), new FakePasswordHasher());

    private static User RegisterUser(string email, string username, string password) =>
        User.RegisterNewUser(UserEmail.Create(email).Value, Username.Create(username).Value, password);
}
