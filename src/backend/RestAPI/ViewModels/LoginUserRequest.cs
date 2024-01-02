using System;
using System.ComponentModel.DataAnnotations;

using Conduit.RestAPI.ViewModels;

namespace Conduit.RestAPI.ViewModels;

public class LoginUserRequest
{
    public LoginUserRequest(LoginUser user)
    {
        User = user;
    }

    [Required]
    public LoginUser User { get; init; }
}