using System.Threading;
using System.Threading.Tasks;
using Conduit.Application.Users.Dtos;
using Conduit.Domain.Common;
using CSharpFunctionalExtensions;
using MediatR;

namespace Conduit.Application;

public class CurrentUserHandler : IRequestHandler<CurrentUserQuery, Result<UserDto, Error>>
{
    public Task<Result<UserDto, Error>> Handle(CurrentUserQuery request, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}
