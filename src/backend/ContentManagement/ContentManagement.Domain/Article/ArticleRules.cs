/* using Conduit.Users.Domain.Common;
using CSharpFunctionalExtensions;

namespace Conduit.Users.Domain.Article;

static class ArticleRules
{
    public static UnitResult<Error> SlugMustBeUniqueRule(ArticleSlug slug, IArticleCounter articleCounter)
    {
        if (articleCounter.CountArticlesWithSlugAsync(slug).GetAwaiter().GetResult() == 0)
        {
            return UnitResult.Success<Error>();
        }
        else
        {
            return ArticleErrors.SlugIsNotUnique();
        }
    }

    public static UnitResult<Error> TitleMustBeUniqueRule(ArticleTitle title, IArticleCounter articleCounter)
    {
        if (articleCounter.CountArticlesWithTitleAsync(title).GetAwaiter().GetResult() == 0)
        {
            return UnitResult.Success<Error>();
        }
        else
        {
            return ArticleErrors.SlugIsNotUnique();
        }
    }
}
 */
