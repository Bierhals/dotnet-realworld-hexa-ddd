using System.Linq;
using System.Security.Claims;
using Conduit.Shared.Application;
using Conduit.Shared.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace Conduit.Host.WebApi.Infrastructure;

public sealed class CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserAccessor, ICurrentUserSetter
{
    private string? _username;

    public void SetCurrentUsername(string username) => _username = username;

    public string? GetCurrentUsername() =>
        _username ??= httpContextAccessor
            .HttpContext?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
            ?.Value;
}
