using Conduit.Shared.Application.Cqrs;

namespace Conduit.Application.Queries.CurrentUser;

public record CurrentUserQuery() : IQuery<User>;
