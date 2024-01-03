using Conduit.RestAPI.ViewModels;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

namespace Conduit.RestAPI.Controllers;

[ApiController]
[Route("users")]
[Tags("User and Authentication")]
[Consumes("application/json")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    /// <summary>
    /// Register a new user
    /// </summary>
    /// <remarks>
    /// <a href="https://realworld-docs.netlify.app/docs/specs/backend-specs/endpoints/#registration">Conduit Spec for registration endpoint</a>
    /// </remarks>
    /// <param name="request">Details of the new user to register</param>
    /// <returns></returns>
    /// <response code="201">Returns the newly registered User</response>
    /// <response code="422">The registration information was not valid (e.g. invalid email, weak password, etc)</response>
    [HttpPost]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public IActionResult CreateUser([FromBody, SwaggerRequestBody(Required = true)] NewUserRequest request)
    {
        return Ok(
            new UserResponse
            {
                User = new User
                {
                    Email = request.User.Email,
                    Username = request.User.Email,
                    Token = "Test Token",
                    Bio = "Test Bio",
                    Image = "Test"
                }
            });
    }

    /// <summary>
    /// Get current user
    /// </summary>
    /// <remarks>
    /// Gets the currently logged-in user<br/><a href="https://realworld-docs.netlify.app/docs/specs/backend-specs/endpoints#get-current-user">Conduit spec for Get Current User endpoint</a>
    /// </remarks>
    /// <returns></returns>
    /// <response code="200">Returns the currently logged in User</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="422">The given token was not valid (weird edge case, even possible?)</response>
    [HttpGet("/user")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public IActionResult GetCurrentUser()
    {
        return Ok(
            new UserResponse
            {
                User = new User
                {
                    Email = "@mail.com",
                    Username = "name",
                    Token = "Test Token",
                    Bio = "Test Bio",
                    Image = "Test"
                }
            });
    }

    /// <summary>
    /// Update current user
    /// </summary>
    /// <remarks>
    /// Updated user information for current user<br/><a href="https://realworld-docs.netlify.app/docs/specs/backend-specs/endpoints#update-user">Conduit spec for Update User</a>
    /// </remarks>
    /// <param name="request">User details to update. At least **one** field is required.</param>
    /// <returns></returns>
    /// <response code="200">Return the user after update</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="422">The update request was invalid</response>
    [HttpPut("/user")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public IActionResult UpdateCurrentUser([FromBody, SwaggerRequestBody(Required = true)] UpdateUserRequest request)
    {
        return Ok(
            new UserResponse
            {
                User = new User
                {
                    Email = "@mail.com",
                    Username = "name",
                    Token = "Test Token",
                    Bio = "Test Bio",
                    Image = "Test"
                }
            });
    }
}