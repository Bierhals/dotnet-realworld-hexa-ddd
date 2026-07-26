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
        return await usersReadRepository.GetByUsernameAsync(currentUserAccessor.GetCurrentUsername(), cancellationToken)
            .ThenDo(user => user.Token = tokenGenerator.CreateToken(user.Username));
    }
}
