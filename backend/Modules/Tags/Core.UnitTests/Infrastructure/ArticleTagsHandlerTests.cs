using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Contracts.Events;
using Conduit.Tags.Contracts.Catalog;
using Conduit.Tags.Core.Infrastructure.EventHandling;
using ErrorOr;
using Shouldly;

namespace Conduit.Tags.Core.UnitTests.Infrastructure;

public class ArticleTagsHandlerTests
{
    private readonly RecordingTagCatalogService _catalog = new();

    private ArticleTagsHandler Handler => new(_catalog);

    [Fact]
    public async Task Tags_an_article_started_using_are_referenced_in_the_catalog()
    {
        await Handler.Handle(new ArticleTagsReferenced(Guid.NewGuid(), ["dragons", "training"]), CancellationToken.None);

        _catalog.Referenced.ShouldBe(["dragons", "training"]);
        _catalog.Released.ShouldBeEmpty();
    }

    [Fact]
    public async Task Tags_an_article_stopped_using_are_released_in_the_catalog()
    {
        await Handler.Handle(new ArticleTagsReleased(Guid.NewGuid(), ["dragons"]), CancellationToken.None);

        _catalog.Released.ShouldBe(["dragons"]);
        _catalog.Referenced.ShouldBeEmpty();
    }

    [Fact]
    public async Task A_catalog_that_rejects_the_tags_fails_the_message_so_it_is_retried()
    {
        _catalog.Rejection = Error.Validation("Tag.NameTooLong", "too long");

        await Should.ThrowAsync<InvalidOperationException>(() =>
            Handler.Handle(new ArticleTagsReferenced(Guid.NewGuid(), ["dragons"]), CancellationToken.None));
    }

    private sealed class RecordingTagCatalogService : ITagCatalogService
    {
        public List<string> Referenced { get; } = [];

        public List<string> Released { get; } = [];

        public Error? Rejection { get; set; }

        public Task<ErrorOr<Success>> ReferenceTagsAsync(IReadOnlyCollection<string> tagNames, CancellationToken cancellationToken = default)
        {
            Referenced.AddRange(tagNames);

            return Task.FromResult(Rejection is { } error ? (ErrorOr<Success>)error : Result.Success);
        }

        public Task<ErrorOr<Success>> ReleaseTagsAsync(IReadOnlyCollection<string> tagNames, CancellationToken cancellationToken = default)
        {
            Released.AddRange(tagNames);

            return Task.FromResult<ErrorOr<Success>>(Result.Success);
        }
    }
}
