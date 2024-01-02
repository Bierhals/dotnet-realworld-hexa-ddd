using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Conduit.RestAPI.ViewModels;

public class LoginUser
{
    [Required]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}
