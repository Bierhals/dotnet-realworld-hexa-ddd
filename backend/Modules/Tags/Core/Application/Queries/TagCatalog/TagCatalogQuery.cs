using System.Collections.Generic;
using Conduit.Shared.Application.Cqrs;

namespace Conduit.Tags.Core.Application.Queries.TagCatalog;

public sealed record TagCatalogQuery : IQuery<IReadOnlyCollection<string>>;
