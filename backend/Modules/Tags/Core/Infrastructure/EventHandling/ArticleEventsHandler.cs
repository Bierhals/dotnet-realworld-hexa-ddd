using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Contracts.Events;
using Conduit.Shared.Application.Cqrs;
using Conduit.Tags.Core.Application.Commands.RemoveArticleTags;
using Conduit.Tags.Core.Application.Commands.UpdateArticleTags;
using ErrorOr;
using Wolverine.Attributes;

namespace Conduit.Tags.Core.Infrastructure.EventHandling;

/// <summary>
/// Keeps the tag catalog in step with the articles that use its tags. A driving adapter like an
/// endpoint: it translates another module's integration events into calls against this module's
/// own application layer. It uses the module's command handlers directly.
/// </summary>
/// <remarks>
/// The command handlers commit through the module's unit of work, which owns the transaction, so
/// Wolverine's automatic transaction around this handler is switched off - it would fight over the
/// connection. Delivery is therefore at-least-once. That is harmless: the record of what each
/// article used is saved together with the catalog, and an event that is not newer than that
/// record changes nothing.
/// </remarks>
[NonTransactional]
public sealed class ArticleEventsHandler(
    ICommandHandler<UpdateArticleTagsCommand> updateArticleTags,
    ICommandHandler<RemoveArticleTagsCommand> removeArticleTags)
{
    public async Task Handle(ArticlePublished message, CancellationToken cancellationToken) =>
        await Update(message.Article, cancellationToken);

    public async Task Handle(ArticleEdited message, CancellationToken cancellationToken) =>
        await Update(message.Article, cancellationToken);

    public async Task Handle(ArticleDeleted message, CancellationToken cancellationToken) =>
        ThrowIfFailed(await removeArticleTags.Handle(
            new RemoveArticleTagsCommand { ArticleId = message.ArticleId, Revision = message.Revision },
            cancellationToken));

    private async Task Update(ArticleSnapshot article, CancellationToken cancellationToken) =>
        ThrowIfFailed(await updateArticleTags.Handle(
            new UpdateArticleTagsCommand
            {
                ArticleId = article.Id,
                Revision = article.Revision,
                TagNames = article.Tags,
            },
            cancellationToken));

    // The Articles module validated the names already, so a failure here is not something the
    // sender can fix: throwing hands the message to Wolverine's retry and dead-letter handling.
    private static void ThrowIfFailed(ErrorOr<Success> result)
    {
        if (result.IsError)
        {
            throw new InvalidOperationException(
                $"The tag catalog rejected the tags of an article: {result.FirstError.Description}");
        }
    }
}
