using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Contracts.Events;
using Conduit.Tags.Core.Application.Commands.ReferenceTags;
using Conduit.Tags.Core.Application.Commands.ReleaseTags;
using Conduit.Tags.Core.Infrastructure.EventHandling;
using Conduit.Tags.Core.UnitTests.TestDoubles;
using Shouldly;

namespace Conduit.Tags.Core.UnitTests.Infrastructure;

public class ArticleTagsHandlerTests
{
    private readonly FakeTagsRepository _tags = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private ArticleTagsHandler Handler => new(
        new ReferenceTagsHandler(_tags, _unitOfWork),
        new ReleaseTagsHandler(_tags, _unitOfWork));

    [Fact]
    public async Task Tags_an_article_started_using_are_added_to_the_catalog()
    {
        await Handler.Handle(new ArticleTagsReferenced(Guid.NewGuid(), ["dragons", "training"]), CancellationToken.None);

        _tags.Tags.Select(tag => tag.Id.Value).ShouldBe(["dragons", "training"], ignoreOrder: true);
    }

    [Fact]
    public async Task Tags_an_article_stopped_using_leave_the_catalog_when_nothing_else_uses_them()
    {
        _tags.Seed("dragons", referenceCount: 1);
        _tags.Seed("training", referenceCount: 2);

        await Handler.Handle(new ArticleTagsReleased(Guid.NewGuid(), ["dragons", "training"]), CancellationToken.None);

        var remaining = _tags.Tags.ShouldHaveSingleItem();
        remaining.Id.Value.ShouldBe("training");
        remaining.ReferenceCount.ShouldBe(1);
    }

    [Fact]
    public async Task A_tag_name_the_catalog_rejects_fails_the_message_so_it_is_retried()
    {
        await Should.ThrowAsync<InvalidOperationException>(() =>
            Handler.Handle(new ArticleTagsReferenced(Guid.NewGuid(), [new string('x', 200)]), CancellationToken.None));
    }
}
