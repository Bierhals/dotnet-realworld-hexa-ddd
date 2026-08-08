using System;
using Conduit.Shared.Domain;
using ErrorOr;

namespace Conduit.Identity.Domain.Rules;

public sealed class UserImageUrlIsWellFormed : IBusinessRule
{
    private readonly string _url;
    public UserImageUrlIsWellFormed(string url) => _url = url;

    public ErrorOr<Success> Check()
    {
        var urlIsWellFormed = Uri.IsWellFormedUriString(_url, UriKind.Absolute);

        return !urlIsWellFormed
            ? Error.Validation("UserImage.InvalidImage", "The user image must have a valid URL.")
            : Result.Success;
    }
}
