namespace Conduit.Host.WebApi.Infrastructure;

public interface ICurrentUserAccessor
{
    public string? GetCurrentUsername();
}
