using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Application.Users.Services;

public interface IAuthenticationService
{
    Task<bool> ValidateLoginAsync(string email, string password, CancellationToken cancellationToken);
    string GenerateJwtToken(string email);
}
