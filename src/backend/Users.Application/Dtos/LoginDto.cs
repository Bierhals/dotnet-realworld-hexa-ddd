namespace Conduit.Users.Application.Users.Dtos;

public record LoginDto
{
    public required string Email { get; init; }
    public required string HashedPassword { get; init; }
}
