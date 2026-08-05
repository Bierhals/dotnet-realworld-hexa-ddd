using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application.Cqrs;
using Conduit.Tags.Core.Domain;
using ErrorOr;

namespace Conduit.Tags.Core.Application.Commands.ReleaseTags;

public sealed class ReleaseTagsHandler(ITagsRepository tagsRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<ReleaseTagsCommand>
{
    public async Task<ErrorOr<Success>> Handle(ReleaseTagsCommand command, CancellationToken cancellationToken)
    {
        var names = new List<TagName>();
        foreach (var tagName in command.TagNames)
        {
            var name = TagName.Create(tagName);
            if (name.IsError)
            {
                return name.Errors;
            }

            if (!names.Contains(name.Value))
            {
                names.Add(name.Value);
            }
        }

        if (names.Count == 0)
        {
            return Result.Success;
        }

        var knownTags = await tagsRepository.GetByNamesAsync(names, cancellationToken);
        var knownTagsByName = knownTags.ToDictionary(tag => tag.Id);

        foreach (var name in names)
        {
            // A name that is not in the catalog has nothing to release - releasing must stay
            // idempotent so that a caller can clean up after a partially failed write.
            if (!knownTagsByName.TryGetValue(name, out var tag))
            {
                continue;
            }

            var release = tag.Release();
            if (release.IsError)
            {
                return release.Errors;
            }

            if (tag.IsUnreferenced)
            {
                tagsRepository.Remove(tag);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
