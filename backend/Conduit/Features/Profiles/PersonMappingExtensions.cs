using Conduit.Domain;

namespace Conduit.Features.Profiles;

public static class PersonMappingExtensions
{
    public static Profile MapToProfile(this Person person)
        => new()
        {
            Username = person.Username,
            Bio = person.Bio,
            Image = person.Image,
        };
}
