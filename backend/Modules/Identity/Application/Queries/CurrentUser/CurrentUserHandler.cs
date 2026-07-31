using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application;
using Conduit.Shared.Application.Cqrs;
using ErrorOr;

namespace Conduit.Identity.Application.Queries.CurrentUser;

public sealed class CurrentUserHandler(ICurrentUserAccessor currentUserAccessor, IUsersReadRepository usersReadRepository, ITokenGenerator tokenGenerator) : IQueryHandler<CurrentUserQuery, User>
{
    public async Task<ErrorOr<User>> Handle(CurrentUserQuery message, CancellationToken cancellationToken)
    {
        var currentUsername = currentUserAccessor.GetCurrentUsername() ?? throw new UnauthorizedAccessException("No authenticated user.");

        return await usersReadRepository.GetByUsernameAsync(currentUsername, cancellationToken)
            .ThenDo(user => user.Token = tokenGenerator.CreateToken(user.Username));
    }
}
