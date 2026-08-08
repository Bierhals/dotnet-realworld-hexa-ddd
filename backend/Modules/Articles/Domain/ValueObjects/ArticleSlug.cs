using System.Text.RegularExpressions;

namespace Conduit.Articles.Domain.ValueObjects;

/// <summary>
/// The public, URL-facing identity of an article, derived from its title.
/// </summary>
public sealed partial record ArticleSlug
{
    public const int MaximumLength = 45;

    public string Value { get; }

    private ArticleSlug(string value) => Value = value;

    /// <summary>
    /// Derives a slug from a title: lower case, invalid characters removed, runs of whitespace
    /// collapsed, truncated to <see cref="MaximumLength"/> characters and finally hyphenated.
    /// </summary>
    public static ArticleSlug FromTitle(ArticleTitle title)
    {
        var slug = title.Value.ToLowerInvariant();
        slug = InvalidCharsRegex().Replace(slug, "");
        slug = MultipleSpacesRegex().Replace(slug, " ").Trim();
        slug = slug[..(slug.Length <= MaximumLength ? slug.Length : MaximumLength)].Trim();
        slug = WhitespaceRegex().Replace(slug, "-");

        return new ArticleSlug(slug);
    }

    public static ArticleSlug Rehydrate(string value) => new(value);

    [GeneratedRegex("[^a-z0-9\\s-]")]
    private static partial Regex InvalidCharsRegex();

    [GeneratedRegex("\\s+")]
    private static partial Regex MultipleSpacesRegex();

    [GeneratedRegex("\\s")]
    private static partial Regex WhitespaceRegex();
}
