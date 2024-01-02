using System;
using System.ComponentModel.DataAnnotations;

namespace Conduit.RestAPI;

public class UserResponse
{
    public UserResponse(User user)
    {
        User = user;
    }

    [Required]
    public User User { get; init; }
}