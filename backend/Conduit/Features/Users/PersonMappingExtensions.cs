using Conduit.Domain;

namespace Conduit.Features.Users;

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
