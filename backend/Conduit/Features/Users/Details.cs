using System;
using System.Net;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using Conduit.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Conduit.Shared.RequestHandling;

namespace Conduit.Features.Users;

public class Details
{
    public record Query([Required] string Username) : IQuery<UserEnvelope>;

    public class Handler(
        ConduitContext context,
        IJwtTokenGenerator jwtTokenGenerator
    ) : IQueryHandler<Query, UserEnvelope>
    {
        public async Task<UserEnvelope> Handle(Query message, CancellationToken cancellationToken)
        {
            var person = await context
                .Persons.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Username == message.Username, cancellationToken);

            if (person == null)
            {
                throw new RestException(
                    HttpStatusCode.NotFound,
                    new { User = Constants.NOT_FOUND }
                );
            }

            var user = person.MapToUser();
            user.Token = jwtTokenGenerator.CreateToken(
                person.Username ?? throw new InvalidOperationException()
            );
            return new UserEnvelope(user);
        }
    }
}
