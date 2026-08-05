using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.Application.Cqrs;
using Conduit.Tags.Core.Domain;
using ErrorOr;

namespace Conduit.Tags.Core.Application.Commands.ReferenceTags;

public sealed class ReferenceTagsHandler(ITagsRepository tagsRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<ReferenceTagsCommand>
{
    public async Task<ErrorOr<Success>> Handle(ReferenceTagsCommand command, CancellationToken cancellationToken)
    {
        var names = new List<TagName>();
        foreach (var tagName in command.TagNames)
        {
            var name = TagName.Create(tagName);
            if (name.IsError)
            {
                return name.Errors;
            }

            // The same tag may be listed twice by a caller; the catalog counts it once.
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
            if (!knownTagsByName.TryGetValue(name, out var tag))
            {
                tag = Tag.Create(name);
                tagsRepository.Add(tag);
            }

            tag.Reference();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
