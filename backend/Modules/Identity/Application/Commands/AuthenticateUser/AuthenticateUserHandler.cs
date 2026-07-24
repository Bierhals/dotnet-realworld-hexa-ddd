using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Domain;
using Conduit.Identity.Domain.Services;
using Conduit.Identity.Domain.ValueObjects;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Identity.Application.Commands.AuthenticateUser;

public sealed class AuthenticateUserHandler(IUsersRepository usersRepository, UserLoginValidator loginValidator) : ICommandHandler<AuthenticateUserCommand, Guid>
{
    public async Task<ErrorOr<Guid>> Handle(AuthenticateUserCommand message, CancellationToken cancellationToken)
    {
        return await UserEmail.Create(message.Email)
            .ThenAsync(async email => await usersRepository.GetByEmailAsync(email, cancellationToken))
            .ThenEnsure(user => loginValidator.Validate(user, message.Password).Then(_ => user))
            .Then(user => user.Id.Value);
    }
}