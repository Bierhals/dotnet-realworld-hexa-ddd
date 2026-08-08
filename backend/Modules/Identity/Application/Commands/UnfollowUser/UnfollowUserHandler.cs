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
    public Task<ErrorOr<Success>> Handle(UnfollowUserCommand message, CancellationToken cancellationToken)
    {
        var currentUsername = currentUserAccessor.GetCurrentUsername()
            ?? throw new UnauthorizedAccessException("No authenticated user.");

        return Username.Create(currentUsername)
            .ThenAsync(username => usersRepository.GetByUsernameAsync(username, cancellationToken))
            .ThenAsync(async follower =>
            {
                var target = await Username.Create(message.Username)
                    .ThenAsync(username => usersRepository.GetByUsernameAsync(username, cancellationToken));

                return await target.ThenDoAsync(async t =>
                {
                    var userFollow = await userFollowsRepository.GetAsync(t.Id, follower.Id, cancellationToken);
                    if (userFollow is null)
                    {
                        return;
                    }

                    userFollow.Unfollow();
                    await userFollowsRepository.RemoveAsync(userFollow, cancellationToken);
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                }).Then(_ => Result.Success);
            });
    }
}
