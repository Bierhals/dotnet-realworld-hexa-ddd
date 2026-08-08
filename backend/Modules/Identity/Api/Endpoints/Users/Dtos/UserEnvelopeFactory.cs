using CurrentUserQueries = Conduit.Identity.Application.Queries.CurrentUser;

namespace Conduit.Identity.Api.Endpoints.Users.Dtos;

internal static class UserEnvelopeFactory
{
    public static UserEnvelope Create(CurrentUserQueries.User user) => new(new UserResponse
    {
        Username = user.Username,
        Email = user.Email,
        Bio = user.Bio,
        Image = user.Image,
        Token = user.Token,
    });
}
