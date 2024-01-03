using Conduit.RestAPI.ViewModels;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Conduit.RestAPI.Controllers;

[ApiController]
[Route("profiles")]
[Tags("Profile")]
[Consumes("application/json")]
[Produces("application/json")]
public class ProfilesController : ControllerBase
{
    /// <summary>
    /// Get a profile
    /// </summary>
    /// <remarks>
    /// Get a profile of a user of the system. Auth is optional<br/><a href="https://realworld-docs.netlify.app/docs/specs/backend-specs/endpoints/#get-profile">Conduit spec for Get Profile</a>
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
    public IActionResult GetProfileByUsername([FromQuery] string username)
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

    /// <summary>
    /// Follow a user
    /// </summary>
    /// <remarks>
    /// Follow a user by username<br/><a href="https://realworld-docs.netlify.app/docs/specs/backend-specs/endpoints/#follow-user">Conduit Spec for follow user endpoint</a>
    /// </remarks>
    /// <param name="username">Username of the profile you want to follow</param>
    /// <returns></returns>
    /// <response code="200">Returns the followed user's detailed profile information</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="422">User not found</response>
    [HttpPost("{username}/follow")]
    [ProducesResponseType<ProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public IActionResult FollowUserByUsername([FromQuery] string username)
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

    /// <summary>
    /// Unfollow a user
    /// </summary>
    /// <remarks>
    /// Unfollow a user by username<br/><a href="https://realworld-docs.netlify.app/docs/specs/backend-specs/endpoints#unfollow-user">Conduit Spec for unfollow user endpoint</a>
    /// </remarks>
    /// <param name="username">Username of the profile you want to unfollow</param>
    /// <returns></returns>
    /// <response code="200">Returns the unfollowed user's detailed profile information</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="422">User not found</response>
    [HttpDelete("{username}/follow")]
    [ProducesResponseType<ProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public IActionResult UnfollowUserByUsername([FromQuery] string username)
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