using System;
using System.Threading.Tasks;

namespace Conduit.Domain.User;

public interface IUserRepository
{
    Task AddAsync(User user);
    Task<User> GetByIdAsync(UserId id);
}
