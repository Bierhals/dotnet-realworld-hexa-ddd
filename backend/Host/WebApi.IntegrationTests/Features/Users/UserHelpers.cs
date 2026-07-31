using System.Threading.Tasks;

namespace Conduit.Host.WebApi.IntegrationTests.Features.Users;

public static class UserHelpers
{
    public static readonly string DefaultUserName = "username";

    /// <summary>
    /// seeds a default user's profile to be used in different tests
    /// </summary>
    /// <param name="fixture"></param>
    /// <returns></returns>
    public static Task<string> CreateDefaultUser(SliceFixture fixture)
    {
        fixture.ProfileQueryService.AddProfile(DefaultUserName);

        return Task.FromResult(DefaultUserName);
    }
}
