using System.ComponentModel.DataAnnotations;

namespace Conduit.RestAPI.ViewModels;

public class LoginUser
{
    public LoginUser(string email, string password)
    {
        Email = email;
        Password = password;
    }

    [Required]
    public string Email { get; init; }
    [Required]
    public string Password { get; init; }
}