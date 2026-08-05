using System.Collections.Generic;
using Conduit.Shared.Application.Cqrs;

namespace Conduit.Tags.Core.Application.Commands.ReferenceTags;

public sealed record ReferenceTagsCommand : ICommand
{
    public required IReadOnlyCollection<string> TagNames { get; init; }
}
