using System.Collections.Generic;

namespace Conduit.Tags.Core.Api.Endpoints.Tags;

public sealed record TagsEnvelope(IReadOnlyCollection<string> Tags);
