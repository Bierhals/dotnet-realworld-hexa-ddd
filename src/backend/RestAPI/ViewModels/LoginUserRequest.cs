using System;
using System.ComponentModel.DataAnnotations;

using Conduit.RestAPI.ViewModels;

namespace Conduit.RestAPI.ViewModels;

public class LoginUserRequest 
{
    [Required]
    public LoginUser User { get; set; } = default;
}
