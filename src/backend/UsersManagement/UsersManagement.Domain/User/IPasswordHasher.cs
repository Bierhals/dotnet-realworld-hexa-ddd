namespace Conduit.Users.Domain.User;

public interface IPasswordHasher
{
    string HashPassword(string clearTextPassword);
    bool VerifyPassword(string clearTextPassword, string hashedPassword);
}
