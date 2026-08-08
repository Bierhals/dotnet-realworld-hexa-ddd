using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Domain;
using Conduit.Identity.Domain.Services;
using Conduit.Identity.Domain.ValueObjects;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Identity.Application.Commands.AuthenticateUser;

public sealed class AuthenticateUserHandler(IUsersRepository usersRepository, UserLoginValidator loginValidator) : ICommandHandler<AuthenticateUserCommand, string>
{
    public Task<ErrorOr<string>> Handle(AuthenticateUserCommand message, CancellationToken cancellationToken) =>
        UserEmail.Create(message.Email)
            .ThenAsync(async email =>
            {
                var userResult = await usersRepository.GetByEmailAsync(email, cancellationToken);
                if (userResult.IsError)
                {
                    // Do not distinguish "no user for this email" from "wrong password" — both must look
                    // like invalid credentials to the caller, otherwise login becomes a user-enumeration oracle.
                    return Error.Unauthorized("User.InvalidCredentials", "The provided credentials are invalid.");
                }

                return loginValidator.Validate(userResult.Value, message.Password)
                    .Then(_ => userResult.Value.Username.Value);
            });
}
