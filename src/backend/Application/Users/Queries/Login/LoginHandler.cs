using System.Threading;
using System.Threading.Tasks;
using Conduit.Application.Users.Dtos;
using Conduit.Application.Users.Repositories;
using Conduit.Application.Users.Services;
using Conduit.Domain.Common;
using CSharpFunctionalExtensions;
using MediatR;

namespace Conduit.Application.Users.Queries.Login;

public class LoginHandler : IRequestHandler<LoginCommand, Result<UserDto, Error>>
{
    readonly IAuthenticationService _authenticationService;
    readonly IUsersQueryRepository _usersQueryRepository;

    public LoginHandler(
        IAuthenticationService authenticationService,
        IUsersQueryRepository usersQueryRepository)
    {
        _authenticationService = authenticationService;
        _usersQueryRepository = usersQueryRepository;
    }

    public async Task<Result<UserDto, Error>> Handle(LoginCommand request, CancellationToken cancellationToken = default)
    {
        string emailLowerCase = request.Email.ToLower();
        bool loginIsValid = await _authenticationService.ValidateLoginAsync(emailLowerCase, request.Password, cancellationToken);
        if (!loginIsValid)
        {
            return Result.Failure<UserDto, Error>(new Error(errorCode: "login.is.invalid", message: "Login is invalid"));
        }

        return await _usersQueryRepository.GetByEmailAsync(emailLowerCase, cancellationToken);
    }
}
