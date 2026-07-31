using System;
using Conduit.Shared.Domain;
using ErrorOr;

namespace Conduit.Identity.Domain.Rules;

public sealed class UserImageIsValidUrl : IBusinessRule
{
    private readonly string _url;
    public UserImageIsValidUrl(string url) => _url = url;

    public ErrorOr<Success> Check()
    {
        var urlIsValid = Uri.IsWellFormedUriString(_url, UriKind.Absolute);

        return !urlIsValid
            ? Error.Validation("UserImage.InvalidImage", "The user image must have a valid URL.")
            : Result.Success;
    }
}
