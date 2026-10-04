using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Tags.Core.Domain;
using ErrorOr;

namespace Conduit.Tags.Core.Application;

/// <summary>
/// Applies the tags an article started and stopped using to the catalog: a tag nobody used before
/// enters it, and a tag that loses its last use leaves it. Saving is left to the caller, so that
/// the catalog and the record of the article's tags commit together.
/// </summary>
public sealed class TagReferenceCounter(ITagsRepository tagsRepository)
{
    public async Task<ErrorOr<Success>> ApplyAsync(TagUsageChange change, CancellationToken cancellationToken)
    {
        var knownTags = await tagsRepository.GetByNamesAsync(
            [.. change.Added, .. change.Removed],
            cancellationToken);
        var knownTagsByName = knownTags.ToDictionary(tag => tag.Id);

        foreach (var name in change.Added)
        {
            if (!knownTagsByName.TryGetValue(name, out var tag))
            {
                tag = Tag.Create(name);
                tagsRepository.Add(tag);
                knownTagsByName[name] = tag;
            }

            tag.Reference();
        }

        foreach (var name in change.Removed)
        {
            // A name the catalog does not know has nothing to release.
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

        return Result.Success;
    }

    /// <summary>
    /// Turns the names an article carries into tag names, or reports the first one that is not
    /// usable.
    /// </summary>
    public static ErrorOr<List<TagName>> ParseNames(IEnumerable<string> tagNames)
    {
        var names = new List<TagName>();
        foreach (var tagName in tagNames)
        {
            var name = TagName.Create(tagName);
            if (name.IsError)
            {
                return name.Errors;
            }

            names.Add(name.Value);
        }

        return names;
    }
}
