using System.Collections.Generic;

namespace Conduit.Host.WebApi.Features.Tags;

public class TagsEnvelope
{
    public List<string> Tags { get; set; } = new();
}
