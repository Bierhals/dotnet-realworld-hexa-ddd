using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Contracts.Events;
using Conduit.Shared.Application.Cqrs;
using Conduit.Tags.Core.Application.Commands.ReferenceTags;
using Conduit.Tags.Core.Application.Commands.ReleaseTags;
using ErrorOr;
using Wolverine.Attributes;

namespace Conduit.Tags.Core.Infrastructure.EventHandling;

/// <summary>
/// Keeps the tag catalog in step with the articles that use its tags. A driving adapter like an
/// endpoint: it translates another module's integration event into a call against this module's
/// own application layer. It uses the module's command handlers directly.
/// </summary>
/// <remarks>
/// The command handlers commit through the module's unit of work, which owns the transaction, so
/// Wolverine's automatic transaction around this handler is switched off - it would fight over the
/// connection. Delivery is therefore at-least-once, and the reference count is not idempotent by
/// nature: a crash between the commit and the acknowledgement counts a message twice.
/// </remarks>
[NonTransactional]
public sealed class ArticleTagsHandler(
    ICommandHandler<ReferenceTagsCommand> referenceTags,
    ICommandHandler<ReleaseTagsCommand> releaseTags)
{
    public async Task Handle(ArticleTagsReferenced message, CancellationToken cancellationToken) =>
        ThrowIfFailed(await referenceTags.Handle(new ReferenceTagsCommand { TagNames = message.TagNames }, cancellationToken));

    public async Task Handle(ArticleTagsReleased message, CancellationToken cancellationToken) =>
        ThrowIfFailed(await releaseTags.Handle(new ReleaseTagsCommand { TagNames = message.TagNames }, cancellationToken));

    // The article module validated the names already, so a failure here is not something the sender
    // can fix: throwing hands the message to Wolverine's retry and dead-letter handling.
    private static void ThrowIfFailed(ErrorOr<Success> result)
    {
        if (result.IsError)
        {
            throw new InvalidOperationException(
                $"The tag catalog rejected an article's tags: {result.FirstError.Description}");
        }
    }
}
