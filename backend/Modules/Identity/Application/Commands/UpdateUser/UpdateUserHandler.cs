using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Domain;
using Conduit.Identity.Domain.Services;
using Conduit.Identity.Domain.ValueObjects;
using Conduit.Shared.Application;
using Conduit.Shared.Application.Cqrs;
using Conduit.Shared.Application.Optional;
using ErrorOr;

namespace Conduit.Identity.Application.Commands.UpdateUser;

public sealed class UpdateUserHandler(ICurrentUserAccessor currentUserAccessor, IUnitOfWork unitOfWork, IUsersRepository usersRepository, UniqueUserEmailValidator uniqueEmailValidator, UniqueUsernameValidator uniqueUsernameValidator, IPasswordHasher passwordHasher) : ICommandHandler<UpdateUserCommand>
{
    public async Task<ErrorOr<Success>> Handle(UpdateUserCommand message, CancellationToken cancellationToken)
    {
        return await GetCurrentUserAsync(cancellationToken)
            .ThenAsync(user => ApplyUsernameAsync(user, message.Username, cancellationToken))
            .ThenAsync(user => ApplyEmailAsync(user, message.Email, cancellationToken))
            .Then(user => ApplyPassword(user, message.Password))
            .Then(user => ApplyBio(user, message.Bio))
            .Then(user => ApplyImage(user, message.Image))
            .ThenDoAsync(user => unitOfWork.SaveChangesAsync(cancellationToken))
            .Then(_ => Result.Success);
    }

    private Task<ErrorOr<User>> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var currentUsername = currentUserAccessor.GetCurrentUsername() ?? throw new UnauthorizedAccessException("No authenticated user.");

        return Username.Create(currentUsername)
            .ThenAsync(username => usersRepository.GetByUsernameAsync(username, cancellationToken));
    }

    private async Task<ErrorOr<User>> ApplyUsernameAsync(User user, Optional<string> username, CancellationToken cancellationToken)
    {
        if (!username.IsSpecified)
        {
            return user;
        }

        return await Username.Create(username.Value)
            .ThenEnsureAsync(async newUsername => await uniqueUsernameValidator.IsUniqueAsync(newUsername, user.Id, cancellationToken).Then(_ => newUsername))
            .Then(newUsername => user.ChangeUsername(newUsername).Then(_ => user));
    }

    private async Task<ErrorOr<User>> ApplyEmailAsync(User user, Optional<string> email, CancellationToken cancellationToken)
    {
        if (!email.IsSpecified)
        {
            return user;
        }

        return await UserEmail.Create(email.Value)
            .ThenEnsureAsync(async newEmail => await uniqueEmailValidator.IsUniqueAsync(newEmail, user.Id, cancellationToken).Then(_ => newEmail))
            .Then(newEmail => user.ChangeEmail(newEmail).Then(_ => user));
    }

    private ErrorOr<User> ApplyPassword(User user, Optional<string> password)
    {
        if (password.IsSpecified)
        {
            user.ChangePassword(passwordHasher.Hash(password.Value));
        }

        return user;
    }

    private static ErrorOr<User> ApplyBio(User user, Optional<string?> bio)
    {
        if (bio.IsSpecified)
        {
            user.ChangeBio(bio.Value);
        }

        return user;
    }

    private static ErrorOr<User> ApplyImage(User user, Optional<string?> image)
    {
        if (!image.IsSpecified)
        {
            return user;
        }

        if (image.Value is null)
        {
            user.ChangeImage(null);
            return user;
        }

        return UserImage.Create(image.Value)
            .ThenDo(userImage => user.ChangeImage(userImage))
            .Then(_ => user);
    }
}
