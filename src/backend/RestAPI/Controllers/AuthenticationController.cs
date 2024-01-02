using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

using Conduit.RestAPI.ViewModels;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Conduit.RestAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("User and Authentication")]
[Consumes("application/json")]
[Produces("application/json")]
public class AuthenticationController : ControllerBase
{
    [HttpGet]
    public IActionResult Hello()
    {
        return Ok("Hello World");
    }

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
    public IActionResult Login([FromBody][Required] LoginUserRequest request)
    {
        return Ok("Hello World");
    }
}