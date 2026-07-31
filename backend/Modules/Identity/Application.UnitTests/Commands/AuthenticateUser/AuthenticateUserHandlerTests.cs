using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Application.Commands.AuthenticateUser;
using Conduit.Identity.Application.UnitTests.TestDoubles;
using Conduit.Identity.Domain;
using Conduit.Identity.Domain.Services;
using Conduit.Identity.Domain.ValueObjects;
using Shouldly;

namespace Conduit.Identity.Application.UnitTests.Commands.AuthenticateUser;

public class AuthenticateUserHandlerTests
{
    [Fact]
    public async Task Authenticating_with_correct_credentials_returns_the_username()
    {
        var usersRepository = new FakeUsersRepository();
        usersRepository.Add(RegisterUser(email: "jake@jake.jake", username: "jake", password: "s3cret!"));
        var handler = CreateHandler(usersRepository);
        var command = new AuthenticateUserCommand { Email = "jake@jake.jake", Password = "s3cret!" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe("jake");
    }

    [Fact]
    public async Task Authenticating_with_an_unknown_email_returns_invalid_credentials()
    {
        var usersRepository = new FakeUsersRepository();
        var handler = CreateHandler(usersRepository);
        var command = new AuthenticateUserCommand { Email = "unknown@jake.jake", Password = "s3cret!" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("User.InvalidCredentials");
    }

    [Fact]
    public async Task Authenticating_with_a_wrong_password_returns_invalid_credentials()
    {
        var usersRepository = new FakeUsersRepository();
        usersRepository.Add(RegisterUser(email: "jake@jake.jake", username: "jake", password: "s3cret!"));
        var handler = CreateHandler(usersRepository);
        var command = new AuthenticateUserCommand { Email = "jake@jake.jake", Password = "wrong-password" };

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsError.ShouldBeTrue();
        result.FirstError.Code.ShouldBe("User.InvalidCredentials");
    }

    private static AuthenticateUserHandler CreateHandler(FakeUsersRepository usersRepository) =>
        new(usersRepository, new UserLoginValidator(new FakePasswordHasher()));

    private static User RegisterUser(string email, string username, string password) =>
        User.RegisterNewUser(UserEmail.Create(email).Value, Username.Create(username).Value, new FakePasswordHasher().Hash(password));
}
