/* using System.Threading;
using System.Threading.Tasks;
using Conduit.Users.Application.Users.Dtos;

namespace Conduit.Users.Application.Users.Repositories;

public interface IUsersQueryRepository
{
    Task<LoginDto?> FindLoginByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UserDto> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<UserDto> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
 */
