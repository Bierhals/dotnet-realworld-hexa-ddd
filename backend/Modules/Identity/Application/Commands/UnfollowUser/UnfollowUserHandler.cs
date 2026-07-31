using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Identity.Domain;
using Conduit.Identity.Domain.ValueObjects;
using Conduit.Shared.Application;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Identity.Application.Commands.UnfollowUser;

public sealed class UnfollowUserHandler(
    ICurrentUserAccessor currentUserAccessor,
    IUsersRepository usersRepository,
    IUserFollowsRepository userFollowsRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<UnfollowUserCommand>
{
    public async Task<ErrorOr<Success>> Handle(UnfollowUserCommand message, CancellationToken cancellationToken)
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

        var userFollow = await userFollowsRepository.GetAsync(target.Value.Id, follower.Value.Id, cancellationToken);
        if (userFollow is null)
        {
            return Result.Success;
        }

        userFollow.Unfollow();
        await userFollowsRepository.RemoveAsync(userFollow, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
