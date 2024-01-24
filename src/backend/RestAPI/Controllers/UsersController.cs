using System.Threading;
using System.Threading.Tasks;
using Conduit.Application.Users.Commands.Dtos;
using Conduit.Application.Users.Commands.RegisterNewUser;
using Conduit.Domain.Common;
using Conduit.RestAPI.ViewModels;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
    readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <remarks>
    /// <a href="https://realworld-docs.netlify.app/docs/specs/backend-specs/endpoints/#registration">Conduit Spec for registration endpoint</a>
    /// </remarks>
    /// <param name="request">Details of the new user to register</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <response code="201">User</response>
    /// <response code="422">Unexpected error</response>
    [HttpPost]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<GenericErrorModel>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<Results<UnprocessableEntity<GenericErrorModel>, Created<UserResponse>>> CreateUser([FromBody, SwaggerRequestBody(Required = true)] NewUserRequest request, CancellationToken cancellationToken)
    {
        Result<UserDto, RuleError> registrationResult = await _mediator.Send(new RegisterNewUserCommand
        {
            Email = request.User.Email,
            Username = request.User.Username,
            Password = request.User.Password
        }, cancellationToken);

        if (registrationResult.IsFailure)
        {
            return TypedResults.UnprocessableEntity(
                new GenericErrorModel
                {
                    Errors = new Errors
                    {
                        Body = new[] { registrationResult.Error.Message }
                    }
                });
        }

        UserDto newUser = registrationResult.Value;
        
        return TypedResults.Created(
            (string?)null,
            new UserResponse
            {
                User = new User
                {
                    Email = newUser.Email,
                    Username = newUser.Username,
                    Token = newUser.Token,
                    Bio = newUser.Bio,
                    Image = newUser.Image
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
    /// <response code="200">User</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="422">Unexpected error</response>
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
    /// <response code="200">User</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="422">Unexpected error</response>
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
