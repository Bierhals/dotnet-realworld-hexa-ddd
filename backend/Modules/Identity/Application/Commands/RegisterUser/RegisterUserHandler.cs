using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Domain;
using Conduit.Identity.Domain.Services;
using Conduit.Identity.Domain.ValueObjects;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Identity.Application.Commands.RegisterUser;

public sealed class RegisterUserHandler(IUnitOfWork unitOfWork, IUsersRepository usersRepository, UniqueUserEmailValidator uniqueEmailValidator, UniqueUsernameValidator uniqueUsernameValidator, IPasswordHasher passwordHasher) : ICommandHandler<RegisterUserCommand, string>
{
    public Task<ErrorOr<string>> Handle(RegisterUserCommand message, CancellationToken cancellationToken) =>
        ValidateEmailAsync(message.Email, cancellationToken)
            .ThenAsync(email => ValidateUsernameAsync(message.Username, cancellationToken)
                .ThenAsync(username => CreateUserAsync(email, username, message.Password, cancellationToken)));

    private Task<ErrorOr<UserEmail>> ValidateEmailAsync(string email, CancellationToken cancellationToken) =>
        UserEmail.Create(email)
            .ThenEnsureAsync(async newEmail => await uniqueEmailValidator.IsUniqueAsync(newEmail, ct: cancellationToken).Then(_ => newEmail));

    private Task<ErrorOr<Username>> ValidateUsernameAsync(string username, CancellationToken cancellationToken) =>
        Username.Create(username)
            .ThenEnsureAsync(async newUsername => await uniqueUsernameValidator.IsUniqueAsync(newUsername, ct: cancellationToken).Then(_ => newUsername));

    private Task<ErrorOr<string>> CreateUserAsync(UserEmail email, Username username, string password, CancellationToken cancellationToken)
    {
        var hashedPassword = passwordHasher.Hash(password);

        return User.RegisterNewUser(email, username, hashedPassword).ToErrorOr()
            .ThenDoAsync(user => usersRepository.AddAsync(user, cancellationToken))
            .ThenDoAsync(user => unitOfWork.SaveChangesAsync(cancellationToken))
            .Then(user => user.Username.Value);
    }
}