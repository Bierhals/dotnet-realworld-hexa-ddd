using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Application.Commands.RegisterUser;
using Conduit.Identity.Application.UnitTests.TestDoubles;
using Conduit.Identity.Domain;
using Conduit.Identity.Domain.ValueObjects;
using Shouldly;

namespace Conduit.Identity.Application.UnitTests.Commands.RegisterUser;

public class RegisterUserHandlerTests
{
    [Fact]
    public async Task Registering_with_a_new_email_and_username_succeeds_and_returns_the_username()
    {
        var usersRepository = new FakeUsersRepository();
        var handler = CreateHandler(usersRepository);
        var command = new RegisterUserCommand { Username = "jake", Email = "jake@jake.jake", Password = "s3cret!" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe("jake");
        usersRepository.Users.ShouldHaveSingleItem().Username.Value.ShouldBe("jake");
    }

    [Fact]
    public async Task The_stored_users_password_is_hashed_not_stored_in_plain_text()
    {
        var usersRepository = new FakeUsersRepository();
        var handler = CreateHandler(usersRepository);
        var command = new RegisterUserCommand { Username = "jake", Email = "jake@jake.jake", Password = "s3cret!" };

        await handler.Handle(command, CancellationToken.None);

        usersRepository.Users.ShouldHaveSingleItem().PasswordHash.ShouldBe("hashed:s3cret!");
    }

    [Fact]
    public async Task Registering_fails_when_the_email_is_already_taken()
    {
        var usersRepository = new FakeUsersRepository();
        usersRepository.Add(RegisterUser(email: "jake@jake.jake", username: "existing"));
        var handler = CreateHandler(usersRepository);
        var command = new RegisterUserCommand { Username = "jake", Email = "jake@jake.jake", Password = "s3cret!" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("User.NotUniqueEmail");
    }

    [Fact]
    public async Task Registering_fails_when_the_username_is_already_taken()
    {
        var usersRepository = new FakeUsersRepository();
        usersRepository.Add(RegisterUser(email: "other@jake.jake", username: "jake"));
        var handler = CreateHandler(usersRepository);
        var command = new RegisterUserCommand { Username = "jake", Email = "jake@jake.jake", Password = "s3cret!" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("User.NotUniqueUsername");
    }

    [Fact]
    public async Task Registering_fails_when_the_email_is_malformed()
    {
        var usersRepository = new FakeUsersRepository();
        var handler = CreateHandler(usersRepository);
        var command = new RegisterUserCommand { Username = "jake", Email = "not-an-email", Password = "s3cret!" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        usersRepository.Users.ShouldBeEmpty();
    }

    private static RegisterUserHandler CreateHandler(FakeUsersRepository usersRepository) =>
        new(new FakeUnitOfWork(), usersRepository, new(usersRepository), new(usersRepository), new FakePasswordHasher());

    private static User RegisterUser(string email, string username) =>
        User.RegisterNewUser(UserEmail.Create(email).Value, Username.Create(username).Value, "hashed-password");
}
