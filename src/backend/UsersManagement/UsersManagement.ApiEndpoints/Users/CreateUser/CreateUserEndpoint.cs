using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.ApiEndpoints;
using Conduit.Shared.RequestHandling;
using Conduit.UsersManagement.ApiEndpoints.Models;
using Conduit.UsersManagement.Application.Commands.RegisterNewUser;
using Conduit.UsersManagement.Application.Dtos;
using ErrorOr;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Conduit.UsersManagement.ApiEndpoints.CreateUser;

internal sealed class CreateUserEndpoint : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(string.Empty, HandleAsync)
            .WithSummary("Register a new user")
            .WithDescription("<a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints/#registration\">Conduit Spec for registration endpoint</a>");
    }

    private static async Task<Results<Created<UserResponse>, UnprocessableEntity<ValidationProblemDetails>>> HandleAsync([FromBody] NewUserRequest request, [FromServices] ICommandHandler<RegisterNewUserCommand, ErrorOr<UserDto>> commandHandler, CancellationToken ct)
    {
        var result = await commandHandler.Handle(new RegisterNewUserCommand { Email = request.User.Email, Password = request.User.Password, Username = request.User.Username }, ct);
        return result.Match(
            value => (Results<Created<UserResponse>, UnprocessableEntity<ValidationProblemDetails>>)TypedResults.Created(
                (string?)null,
                new UserResponse()
                {
                    User = new()
                    {
                        Email = value.Email,
                        Token = value.Token,
                        Username = value.Username,
                        Bio = value.Bio,
                        Image = value.Image
                    }
                }),
            errors => TypedResults.UnprocessableEntity(new ValidationProblemDetails())
        );

        /*return Task.FromResult<Results<Created<UserResponse>, UnprocessableEntity<ValidationProblemDetails>>>(
            TypedResults.Created((string?)null, new UserResponse()
            {
                User = new()
                {
                    Email = "test@test.de",
                    Token = "Token",
                    Username = "Username",
                    Bio = "Bio",
                    Image = "Image"
                }
            }));*/
    }
}
