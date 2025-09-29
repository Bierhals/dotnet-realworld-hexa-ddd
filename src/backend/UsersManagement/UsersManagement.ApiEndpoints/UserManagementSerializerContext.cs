using System.Text.Json.Serialization;
using Conduit.UsersManagement.ApiEndpoints.Models;
using Microsoft.AspNetCore.Mvc;

namespace Conduit.UsersManagement.ApiEndpoints;

[JsonSerializable(typeof(LoginUser))]
[JsonSerializable(typeof(LoginUserRequest))]
[JsonSerializable(typeof(UserResponse))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(NewUserRequest))]
[JsonSerializable(typeof(NewUser))]
[JsonSerializable(typeof(UpdateUser))]
[JsonSerializable(typeof(UpdateUserRequest))]
[JsonSerializable(typeof(ProfileResponse))]
[JsonSerializable(typeof(Profile))]
internal partial class UserManagementSerializerContext : JsonSerializerContext
{

}

