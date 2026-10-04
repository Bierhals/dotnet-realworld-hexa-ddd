using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application.Cqrs;
using Conduit.Tags.Core.Domain;
using ErrorOr;

namespace Conduit.Tags.Core.Application.Commands.RemoveArticleTags;

public sealed class RemoveArticleTagsHandler(
    IArticleTagUsageRepository usageRepository,
    TagReferenceCounter tagReferenceCounter,
    IUnitOfWork unitOfWork) : ICommandHandler<RemoveArticleTagsCommand>
{
    public async Task<ErrorOr<Success>> Handle(RemoveArticleTagsCommand command, CancellationToken cancellationToken)
    {
        // The deletion may reach this module before anything else about the article did. The record
        // is kept either way, so that the events that were overtaken cannot bring the article back.
        var usage = await usageRepository.GetAsync(command.ArticleId, cancellationToken);
        if (usage is null)
        {
            usage = ArticleTagUsage.Start(command.ArticleId);
            usageRepository.Add(usage);
        }

        if (usage.Delete(command.Revision) is not { } change)
        {
            return Result.Success;
        }

        var applied = await tagReferenceCounter.ApplyAsync(change, cancellationToken);
        if (applied.IsError)
        {
            return applied.Errors;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
