using System;
namespace Conduit.RestAPI.ViewModels;

public record UserResponse
{
    public required User User { get; init; }
}