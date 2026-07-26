using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Domain;
using Conduit.Identity.Domain.ValueObjects;
using Conduit.Shared.Application;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Identity.Application.Commands.FollowUser;

public sealed class FollowUserHandler(
    ICurrentUserAccessor currentUserAccessor,
    IUsersRepository usersRepository,
    IUserFollowsRepository userFollowsRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<FollowUserCommand>
{
    public async Task<ErrorOr<Success>> Handle(FollowUserCommand message, CancellationToken cancellationToken)
    {
        var currentUsername = currentUserAccessor.GetCurrentUsername()
            ?? throw new UnauthorizedAccessException("No authenticated user.");

        var follower = await Username.Create(currentUsername)
            .ThenAsync(username => usersRepository.GetByUsernameAsync(username, cancellationToken));
        if (follower.IsError)
        {
            return follower.Errors;
        }

        var target = await Username.Create(message.Username)
            .ThenAsync(username => usersRepository.GetByUsernameAsync(username, cancellationToken));
        if (target.IsError)
        {
            return target.Errors;
        }

        if (await userFollowsRepository.ExistsAsync(target.Value.Id, follower.Value.Id, cancellationToken))
        {
            return Result.Success;
        }

        return await UserFollow.Create(target.Value.Id, follower.Value.Id)
            .ThenDoAsync(userFollow => userFollowsRepository.AddAsync(userFollow, cancellationToken))
            .ThenDoAsync(_ => unitOfWork.SaveChangesAsync(cancellationToken))
            .Then(_ => Result.Success);
    }
}