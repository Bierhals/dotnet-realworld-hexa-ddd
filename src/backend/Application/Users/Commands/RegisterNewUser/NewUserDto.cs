namespace Conduit.Application.Users.Commands.RegisterNewUser;

public record NewUserDto
{
    public required string Email { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
}
