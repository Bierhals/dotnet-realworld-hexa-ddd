using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace Conduit.RestAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    [HttpGet]
    public IActionResult Hello()
    {
        return Ok("Hello World");
    }
}