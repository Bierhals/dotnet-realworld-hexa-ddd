using Conduit.Users.Application.Dtos;

namespace Conduit.Users.Application.Services;

public interface IAuthenticatedUserService
{
    AuthenticatedUserDto? GetAuthenticatedUser();
}
