namespace Conduit.Shared.Application;

public interface ICurrentUserAccessor
{
    public string? GetCurrentUsername();
}
