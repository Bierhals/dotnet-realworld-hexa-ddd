namespace Conduit.Domain;

public interface IPasswordHasher
{
    string HashPassword(string clearTextPassword);
    string VerifyPassword(string clearTextPassword, string hashedPassword);
}
