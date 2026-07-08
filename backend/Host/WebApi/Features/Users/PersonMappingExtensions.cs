using Conduit.Host.WebApi.Domain;

namespace Conduit.Host.WebApi.Features.Users;

public static class PersonMappingExtensions
{
    public static User MapToUser(this Person person)
        => new()
        {
            Username = person.Username,
            Email = person.Email,
            Bio = person.Bio,
            Image = person.Image,
        };
}
