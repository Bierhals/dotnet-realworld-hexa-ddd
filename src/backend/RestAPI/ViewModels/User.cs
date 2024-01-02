using System;
using System.ComponentModel.DataAnnotations;

namespace Conduit.RestAPI;

public class User
{
    public User(string email, string token, string username, string bio, string image)
    {
        Email = email;
        Token = token;
        Username = username;
        Bio = bio;
        Image = image;
    }

    [Required]
    public string Email { get; init; }
    [Required]
    public string Token { get; init; }
    [Required]
    public string Username { get; init; }
    [Required]
    public string Bio { get; init; }
    [Required]
    public string Image { get; init; }
}
