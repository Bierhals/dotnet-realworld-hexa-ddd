namespace Conduit.Domain.User;

public interface IUsersCounter
{
    uint CountUsersWithEmail(string email);
    uint CountUsersWithUsername(string username);
}
