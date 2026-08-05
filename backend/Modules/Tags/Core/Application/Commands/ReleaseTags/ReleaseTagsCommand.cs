using System.Collections.Generic;
using Conduit.Shared.Application.Cqrs;

namespace Conduit.Tags.Core.Application.Commands.ReleaseTags;

public sealed record ReleaseTagsCommand : ICommand
{
    public required IReadOnlyCollection<string> TagNames { get; init; }
}
