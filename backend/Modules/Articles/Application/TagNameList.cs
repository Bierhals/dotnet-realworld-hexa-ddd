using System.Collections.Generic;
using System.Linq;
using Conduit.Articles.Domain.ValueObjects;
using ErrorOr;

namespace Conduit.Articles.Application;

internal static class TagNameList
{
    /// <summary>
    /// Validates a caller-supplied list of tag names, dropping duplicates. The same tag listed
    /// twice is counted once, so the catalog's reference counts stay honest.
    /// </summary>
    public static ErrorOr<List<TagName>> Create(IReadOnlyCollection<string>? tagNames)
    {
        var result = new List<TagName>();
        if (tagNames is null)
        {
            return result;
        }

        foreach (var tagName in tagNames)
        {
            var name = TagName.Create(tagName);
            if (name.IsError)
            {
                return name.Errors;
            }

            if (!result.Contains(name.Value))
            {
                result.Add(name.Value);
            }
        }

        return result;
    }

    public static List<string> ToValues(IEnumerable<TagName> tagNames) =>
        [.. tagNames.Select(tagName => tagName.Value)];
}
