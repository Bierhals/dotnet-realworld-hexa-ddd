using System.Threading;
using System.Threading.Tasks;
using Conduit.Application.Users.Commands.Dtos;
using Conduit.Domain;
using Conduit.Domain.User;
using MediatR;

namespace Conduit.Application.Users.Commands.RegisterNewUser;

public class RegisterNewUserHandler : IRequestHandler<RegisterNewUserCommand, UserDto>
{
    readonly IUsersCounter _usersCounter;
    readonly IUserRepository _userRepository;
    readonly IPasswordHasher _passwordHasher;

    public RegisterNewUserHandler(IUsersCounter usersCounter, IPasswordHasher passwordHasher, IUserRepository userRepository)
    {
        _userRepository = userRepository;
        _usersCounter = usersCounter;
        _passwordHasher = passwordHasher;
    }
    public async Task<UserDto> Handle(RegisterNewUserCommand request, CancellationToken cancellationToken)
    {
        User newUser = User.RegisterNewUser(request.Email, request.Username, request.Password, _usersCounter, _passwordHasher);

        await _userRepository.AddAsync(newUser, cancellationToken);

        return new UserDto
        {
            Email = newUser.Id!.Value,
            Username = newUser.Username,
            Bio = string.Empty,
            Image = string.Empty,
            Token = string.Empty
        };
    }
}
