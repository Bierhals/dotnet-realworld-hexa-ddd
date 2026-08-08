using ErrorOr;

namespace Conduit.Shared.Application;

public static class ErrorOrExtensions
{
    public static ErrorOr<T> ToErrorOr<T>(this T? value, Error notFoundError)
        where T : class =>
        value is null ? notFoundError : value;

    public static ErrorOr<T> ToErrorOr<T>(this T? value, Error notFoundError)
        where T : struct =>
        value is null ? notFoundError : value.Value;
}
