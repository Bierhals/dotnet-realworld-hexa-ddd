using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Conduit.Host.WebApi.Infrastructure;

public sealed class CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    : Conduit.Shared.Application.ICurrentUserAccessor, Conduit.Shared.Infrastructure.ICurrentUserSetter
{
    private string? _username;

    public void SetCurrentUsername(string username) => _username = username;

    public string? GetCurrentUsername() =>
        _username ??= httpContextAccessor
            .HttpContext?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
            ?.Value;
}
