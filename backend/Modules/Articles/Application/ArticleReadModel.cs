using System;
using System.Collections.Generic;

namespace Conduit.Articles.Application;

/// <summary>
/// An article as the outside world sees it, with the author's profile already resolved.
/// </summary>
public sealed record ArticleReadModel(
    string Slug,
    string Title,
    string Description,
    string Body,
    IReadOnlyCollection<string> TagList,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool Favorited,
    int FavoritesCount,
    AuthorProfile Author);

public sealed record ArticleListReadModel(IReadOnlyCollection<ArticleReadModel> Articles, int ArticlesCount);
