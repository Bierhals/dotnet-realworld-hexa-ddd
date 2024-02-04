using Conduit.Application.Users.Dtos;
using Conduit.Domain.Common;
using CSharpFunctionalExtensions;
using MediatR;

namespace Conduit.Application;

public class CurrentUserQuery : IRequest<Result<UserDto, Error>>
{

}
