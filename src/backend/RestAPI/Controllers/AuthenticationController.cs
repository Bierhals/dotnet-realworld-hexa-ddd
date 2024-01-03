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
public class AuthenticationController : ControllerBase
{
    /// <summary>
    /// Login for existing user
    /// </summary>
    /// <remarks>
    /// <a href="https://realworld-docs.netlify.app/docs/specs/backend-specs/endpoints#authentication">Conduit Spec for login endpoint</a>
    /// </remarks>
    /// <returns></returns>
    /// <response code="200">Successful login, returns the User that is logged in</response>
    /// <response code="401">Unauthorized, likely because credentials are incorrect</response>
    [HttpPost("login")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Login([FromBody, SwaggerRequestBody(Description = "Credentials to use", Required = true)] LoginUserRequest request)
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
}