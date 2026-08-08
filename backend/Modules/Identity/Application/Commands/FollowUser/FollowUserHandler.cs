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
    public Task<ErrorOr<Success>> Handle(FollowUserCommand message, CancellationToken cancellationToken)
    {
        var currentUsername = currentUserAccessor.GetCurrentUsername()
            ?? throw new UnauthorizedAccessException("No authenticated user.");

        return Username.Create(currentUsername)
            .ThenAsync(username => usersRepository.GetByUsernameAsync(username, cancellationToken))
            .ThenAsync(async follower =>
            {
                var target = await Username.Create(message.Username)
                    .ThenAsync(username => usersRepository.GetByUsernameAsync(username, cancellationToken));

                return await target.ThenAsync(async t =>
                {
                    if (await userFollowsRepository.ExistsAsync(t.Id, follower.Id, cancellationToken))
                    {
                        return Result.Success;
                    }

                    return await UserFollow.Create(t.Id, follower.Id)
                        .ThenDoAsync(userFollow => userFollowsRepository.AddAsync(userFollow, cancellationToken))
                        .ThenDoAsync(_ => unitOfWork.SaveChangesAsync(cancellationToken))
                        .Then(_ => Result.Success);
                });
            });
    }
}
