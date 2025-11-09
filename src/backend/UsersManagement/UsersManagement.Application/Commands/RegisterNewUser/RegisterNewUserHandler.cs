using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.RequestHandling;
using Conduit.UsersManagement.Application.Dtos;
using ErrorOr;

namespace Conduit.UsersManagement.Application.Commands.RegisterNewUser;

public class RegisterNewUserHandler : ICommandHandler<RegisterNewUserCommand, ErrorOr<UserDto>>
{
    /*readonly IUnitOfWork _unitOfWork;
    readonly IUsersCounter _usersCounter;
    readonly IUsersRepository _userRepository;
    readonly IPasswordHasher _passwordHasher;
    readonly IAuthenticationService _authenticationService;*/

    public RegisterNewUserHandler(/*IUnitOfWork unitOfWork, IUsersCounter usersCounter, IPasswordHasher passwordHasher, IUsersRepository userRepository, IAuthenticationService authenticationService*/)
    {
        /*_userRepository = userRepository;
        _usersCounter = usersCounter;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _authenticationService = authenticationService;*/
    }

    public Task<ErrorOr<UserDto>> Handle(RegisterNewUserCommand request, CancellationToken cancellationToken = default)
    {
        /*Result<UserEmail, Error> email = UserEmail.Create(request.Email);
        Result<Username, Error> username = Username.Create(request.Username);

        return Task.FromResult(Result.Combine<Error>(email, username))
            .Bind(() => User.RegisterNewUser(email.Value, username.Value, request.Password, _usersCounter, _passwordHasher))
            .Map(async (newUser) =>
            {
                await _userRepository.AddAsync(newUser, cancellationToken);
                await _unitOfWork.CommitAsync();

                return new UserDto
                {
                    Id = newUser.Id.Value,
                    Email = newUser.Email.Value,
                    Username = newUser.Username.Value,
                    Bio = newUser.Bio,
                    Image = newUser.Image,
                    Token = _authenticationService.GenerateJwtToken(newUser.Id.Value)
                };
            });*/
        return Task.FromResult(new UserDto
        {
            Id = "Id",
            Email = "Email",
            Username = "Username",
            Bio = "Bio",
            Image = "Image",
            Token = "Token"
        }.ToErrorOr());
    }
}
