using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

using Conduit.RestAPI.ViewModels;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Conduit.RestAPI.Controllers;

[ApiController]
[Route("users")]
[Tags("User and Authentication")]
[Consumes("application/json")]
[Produces("application/json")]
public class AuthenticationController : ControllerBase
{
    /// <summary>
    /// Existing user login
    /// </summary>
    /// <remarks>
    /// Login for existing user
    /// </remarks>
    /// <returns></returns>
    /// <response code="200">Successful login, returns the User that is logged in</response>
    /// <response code="401">Unauthorized, likely because credentials are incorrect</response>
    [HttpPost]
    [Route("login")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Login([FromBody][Required] LoginUserRequest request)
    {
        return Ok(new UserResponse(new User(request.User.Email, "test", request.User.Email, "test", "Test")));
    }
}