using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Domain;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using Conduit.Infrastructure.Security;
using Conduit.Shared.RequestHandling;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Features.Users;

public class Create
{
    public record UserData(
        [Required] string Username,
        [Required] string Email,
        [Required] string Password
    );

    public record Command([Required] UserData User) : ICommand<UserEnvelope>;

    public class Handler(
        ConduitContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator
    ) : ICommandHandler<Command, UserEnvelope>
    {
        public async Task<UserEnvelope> Handle(Command message, CancellationToken cancellationToken)
        {
            if (
                await context
                    .Persons.Where(x => x.Username == message.User.Username)
                    .AnyAsync(cancellationToken)
            )
            {
                throw new RestException(
                    HttpStatusCode.BadRequest,
                    new { Username = Constants.IN_USE }
                );
            }

            if (
                await context
                    .Persons.Where(x => x.Email == message.User.Email)
                    .AnyAsync(cancellationToken)
            )
            {
                throw new RestException(
                    HttpStatusCode.BadRequest,
                    new { Email = Constants.IN_USE }
                );
            }

            var salt = Guid.NewGuid().ToByteArray();
            var person = new Person
            {
                Username = message.User.Username,
                Email = message.User.Email,
                Hash = await passwordHasher.Hash(
                    message.User.Password ?? throw new InvalidOperationException(),
                    salt
                ),
                Salt = salt,
            };

            await context.Persons.AddAsync(person, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            var user = person.MapToUser();
            user.Token = jwtTokenGenerator.CreateToken(
                person.Username ?? throw new InvalidOperationException()
            );
            return new UserEnvelope(user);
        }
    }
}
