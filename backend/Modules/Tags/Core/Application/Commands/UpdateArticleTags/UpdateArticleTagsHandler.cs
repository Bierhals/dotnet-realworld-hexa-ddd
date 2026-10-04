using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application.Cqrs;
using Conduit.Tags.Core.Domain;
using ErrorOr;

namespace Conduit.Tags.Core.Application.Commands.UpdateArticleTags;

public sealed class UpdateArticleTagsHandler(
    IArticleTagUsageRepository usageRepository,
    TagReferenceCounter tagReferenceCounter,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateArticleTagsCommand>
{
    public async Task<ErrorOr<Success>> Handle(UpdateArticleTagsCommand command, CancellationToken cancellationToken)
    {
        var names = TagReferenceCounter.ParseNames(command.TagNames);
        if (names.IsError)
        {
            return names.Errors;
        }

        var usage = await usageRepository.GetAsync(command.ArticleId, cancellationToken);
        if (usage is null)
        {
            usage = ArticleTagUsage.Start(command.ArticleId);
            usageRepository.Add(usage);
        }

        // Nothing to do for a state that is not newer than the one already known.
        if (usage.Update(command.Revision, names.Value) is not { } change)
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
