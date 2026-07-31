using Conduit.Shared.Application.Cqrs;

namespace Conduit.Identity.Application.Queries.CurrentUser;

public sealed record CurrentUserQuery() : IQuery<User>;
