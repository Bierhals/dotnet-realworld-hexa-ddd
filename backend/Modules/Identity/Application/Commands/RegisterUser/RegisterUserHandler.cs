using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Domain;
using Conduit.Identity.Domain.Services;
using Conduit.Identity.Domain.ValueObjects;
using Conduit.Shared.Application;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Identity.Application.Commands.RegisterUser;

public sealed class RegisterUserHandler(IUnitOfWork unitOfWork, IUsersRepository usersRepository, UniqueUserEmailValidator uniqueEmailValidator, UniqueUsernameValidator uniqueUsernameValidator, IPasswordHasher passwordHasher) : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<ErrorOr<Guid>> Handle(RegisterUserCommand message, CancellationToken cancellationToken)
    {
        var email = await UserEmail.Create(message.Email)
            .ThenEnsureAsync(async email => await uniqueEmailValidator.IsUniqueAsync(email, ct: cancellationToken).Then(_ => email));
        if (email.IsError)
        {
            return email.Errors;
        }

        var username = await Username.Create(message.Username)
            .ThenEnsureAsync(async username => await uniqueUsernameValidator.IsUniqueAsync(username, ct: cancellationToken).Then(_ => username));
        if (username.IsError)
        {
            return username.Errors;
        }

        var hashedPassword = passwordHasher.Hash(message.Password);

        return await User.RegisterNewUser(email.Value, username.Value, hashedPassword).ToErrorOr()
            .ThenDoAsync(async user => await usersRepository.AddAsync(user, cancellationToken))
            .ThenDoAsync(async user => await unitOfWork.CommitAsync(cancellationToken))
            .Then(user => user.Id.Value);
    }
}