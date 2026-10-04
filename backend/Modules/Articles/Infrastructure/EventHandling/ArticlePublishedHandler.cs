using System;
using Conduit.Articles.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Conduit.Articles.Infrastructure.EventHandling;

/// <summary>
/// Runs after the article has been committed, in a transaction of its own. This is the shape every
/// in-module follow-up step takes: a plain class with a Handle method, no messaging types in its
/// signature, discovered through <see cref="ArticlesMessagingExtensions"/>.
/// </summary>
/// <remarks>
/// Delivery is at-least-once, so a handler may see the same event more than once and must be
/// idempotent. Logging is, which is why this one is a safe first example.
/// </remarks>
public sealed class ArticlePublishedHandler(ILogger<ArticlePublishedHandler> logger)
{
    private static readonly Action<ILogger, string, Guid, string, Exception?> LogArticlePublished =
        LoggerMessage.Define<string, Guid, string>(
            LogLevel.Information,
            new EventId(1, "ArticlePublishedHandled"),
            "Handled ArticlePublished for article {Slug} ({ArticleId}) by {Author}");

    public void Handle(ArticlePublishedDomainEvent domainEvent) =>
        LogArticlePublished(logger, domainEvent.Article.Slug, domainEvent.Article.ArticleId, domainEvent.Article.Author, null);
}
