using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Domain;
using Conduit.Articles.Domain.ValueObjects;

namespace Conduit.Articles.Application.UnitTests.TestDoubles;

internal sealed class FakeArticlesRepository : IArticlesRepository
{
    private static readonly DateTime PublishedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly List<Article> _articles = [];

    public IReadOnlyCollection<Article> Articles => _articles;

    public Article Seed(string title = "How to train your dragon", string author = "alice", params string[] tags)
    {
        var article = Article.Publish(
            AuthorUsername.Create(author).Value,
            ArticleTitle.Create(title).Value,
            ArticleDescription.Create("Ever wonder how?").Value,
            ArticleBody.Create("You have to believe").Value,
            [.. tags.Select(tag => TagName.Create(tag).Value)],
            PublishedAt);

        article.ClearDomainEvents();
        _articles.Add(article);

        return article;
    }

    public Task<Article?> GetBySlugAsync(ArticleSlug slug, CancellationToken cancellationToken = default) =>
        Task.FromResult(_articles.Find(article => article.Slug == slug));

    public Task<ArticleId?> GetIdBySlugAsync(ArticleSlug slug, CancellationToken cancellationToken = default) =>
        Task.FromResult(_articles.Find(article => article.Slug == slug)?.Id);

    public void Add(Article article) => _articles.Add(article);

    public void Remove(Article article) => _articles.Remove(article);
}
