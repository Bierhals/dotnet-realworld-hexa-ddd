using System.Threading.Tasks;
using Conduit.Host.WebApi.Domain;

namespace Conduit.Host.WebApi.IntegrationTests.Features.Users;

public static class UserHelpers
{
    public static readonly string DefaultUserName = "username";

    /// <summary>
    /// creates a default user to be used in different tests
    /// </summary>
    /// <param name="fixture"></param>
    /// <returns></returns>
    public static async Task<Person> CreateDefaultUser(SliceFixture fixture)
    {
        var person = new Person { Username = DefaultUserName, Email = "email" };

        await fixture.InsertAsync(person);

        return person;
    }
}
