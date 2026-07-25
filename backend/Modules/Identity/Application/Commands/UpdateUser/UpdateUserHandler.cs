using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Domain;
using Conduit.Identity.Domain.Services;
using Conduit.Identity.Domain.ValueObjects;
using Conduit.Shared.Application;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Identity.Application.Commands.UpdateUser;

public sealed class UpdateUserHandler(ICurrentUserAccessor currentUserAccessor, IUnitOfWork unitOfWork, IUsersRepository usersRepository, UniqueUserEmailValidator uniqueEmailValidator, UniqueUsernameValidator uniqueUsernameValidator, IPasswordHasher passwordHasher) : ICommandHandler<UpdateUserCommand>
{
    public async Task<ErrorOr<Success>> Handle(UpdateUserCommand message, CancellationToken cancellationToken)
    {
        var currentUser = await Username.Create(currentUserAccessor.GetCurrentUsername())
            .ThenAsync(async username => await usersRepository.GetByUsernameAsync(username, cancellationToken));
        if (currentUser.IsError)
        {
            return currentUser.Errors;
        }

        if (message.Username is not null)
        {
            var username = await Username.Create(message.Username)
                .ThenEnsureAsync(async username => await uniqueUsernameValidator.IsUniqueAsync(username, currentUser.Value.Id, cancellationToken).Then(_ => username));
            if (username.IsError)
            {
                return username.Errors;
            }
            currentUser.ThenEnsure(user => user.ChangeUsername(username.Value).Then(_ => user));
        }

        if (message.Email is not null)
        {
            var email = await UserEmail.Create(message.Email)
                .ThenEnsureAsync(async email => await uniqueEmailValidator.IsUniqueAsync(email, currentUser.Value.Id, cancellationToken).Then(_ => email));
            if (email.IsError)
            {
                return email.Errors;
            }
            currentUser.ThenEnsure(user => user.ChangeEmail(email.Value).Then(_ => user));
        }

        if (message.Password is not null)
        {
            var hashedPassword = passwordHasher.Hash(message.Password);
            currentUser.ThenEnsure(user => user.ChangePassword(hashedPassword).Then(_ => user));
        }

        currentUser.ThenEnsure(user => user.ChangeProfile(message.Bio, message.Image).Then(_ => user));

        if (currentUser.IsError)
        {
            return currentUser.Errors;
        }       
        return Result.Success;
    }
}