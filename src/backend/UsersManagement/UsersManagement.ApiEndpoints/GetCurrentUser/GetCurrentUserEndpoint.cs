using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.UsersManagement.ApiEndpoints.GetCurrentUser;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Conduit.UsersManagement.ApiEndpoints.Users;

public class GetCurrentUserEndpoint : EndpointWithoutRequest<UserResponse>
{
    public override void Configure()
    {
        Get("/api/user");
        Description(b => b
            .Produces(401)
            .ProducesProblemFE(422) //shortcut for .Produces<ErrorResponse>(422)
            .ProducesProblemFE<InternalErrorResponse>(500));
        Summary(s => {
            s.Summary = "Get current user";
            s.Description = "Gets the currently logged-in user<br/><a href=\"https://realworld-docs.netlify.app/specifications/backend/endpoints#get-current-user\">Conduit spec for Get Current User endpoint</a>";
            s.Responses[422] = "Unprocessable Content";
        });
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendAsync(new()
        {
            User = new()
            {
                Email = "test@test.de",
                Token = "Token",
                Username = "Username",
                Bio = "Bio",
                Image = "Image"
            }
        });
    }
}
