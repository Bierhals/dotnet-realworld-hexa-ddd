using Conduit.RestAPI.ViewModels;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

namespace Conduit.RestAPI.Controllers;

[ApiController]
[Route("profiles")]
[Tags("Profile")]
[Consumes("application/json")]
[Produces("application/json")]
public class ProfilesController : ControllerBase
{
    /// <summary>
    /// Get a profile of a user of the system. Auth is optional
    /// </summary>
    /// <remarks>
    /// <a href="https://realworld-docs.netlify.app/docs/specs/backend-specs/endpoints/#get-profile">Conduit spec for Get Profile</a>
    /// </remarks>
    /// <param name="username">Username of the profile to get</param>
    /// <returns></returns>
    /// <response code="200">Return the public profile</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="422">The get profile request was invalid</response>
    [HttpGet("{username}")]
    [ProducesResponseType<ProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetProfileByUsername(string username)
    {
        return Ok(
            new ProfileResponse
            {
                Profile = new Profile
                {
                    Username = "name",
                    Bio = "Test Bio",
                    Image = "Test",
                    Following = false
                }
            });
    }
}