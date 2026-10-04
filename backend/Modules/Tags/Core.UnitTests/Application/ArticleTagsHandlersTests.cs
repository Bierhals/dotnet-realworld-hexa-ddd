using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Tags.Core.Application;
using Conduit.Tags.Core.Application.Commands.RemoveArticleTags;
using Conduit.Tags.Core.Application.Commands.UpdateArticleTags;
using Conduit.Tags.Core.UnitTests.TestDoubles;
using ErrorOr;
using Shouldly;

namespace Conduit.Tags.Core.UnitTests.Application;

public class ArticleTagsHandlersTests
{
    private readonly FakeTagsRepository _tags = new();
    private readonly FakeArticleTagUsageRepository _usages = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private Task<ErrorOr<Success>> Update(Guid articleId, int revision, params string[] names) =>
        new UpdateArticleTagsHandler(_usages, new TagReferenceCounter(_tags), _unitOfWork)
            .Handle(
                new UpdateArticleTagsCommand { ArticleId = articleId, Revision = revision, TagNames = names },
                CancellationToken.None);

    private Task<ErrorOr<Success>> Remove(Guid articleId, int revision) =>
        new RemoveArticleTagsHandler(_usages, new TagReferenceCounter(_tags), _unitOfWork)
            .Handle(
                new RemoveArticleTagsCommand { ArticleId = articleId, Revision = revision },
                CancellationToken.None);

    private int CountOf(string name) => _tags.Tags.SingleOrDefault(tag => tag.Id.Value == name)?.ReferenceCount ?? 0;

    [Fact]
    public async Task A_published_article_adds_its_tags_to_the_catalog()
    {
        var result = await Update(Guid.NewGuid(), 1, "dragons", "training");

        result.IsError.ShouldBeFalse();
        _tags.Tags.Select(tag => tag.Id.Value).ShouldBe(["dragons", "training"], ignoreOrder: true);
        CountOf("dragons").ShouldBe(1);
        _unitOfWork.SaveCount.ShouldBe(1);
    }

    [Fact]
    public async Task Two_articles_using_a_tag_count_twice()
    {
        await Update(Guid.NewGuid(), 1, "dragons");
        await Update(Guid.NewGuid(), 1, "dragons");

        CountOf("dragons").ShouldBe(2);
    }

    [Fact]
    public async Task An_edit_references_the_new_tags_and_releases_the_old_ones()
    {
        var article = Guid.NewGuid();
        await Update(article, 1, "dragons", "training");

        await Update(article, 2, "dragons", "flying");

        _tags.Tags.Select(tag => tag.Id.Value).ShouldBe(["dragons", "flying"], ignoreOrder: true);
        CountOf("dragons").ShouldBe(1);
        CountOf("flying").ShouldBe(1);
    }

    [Fact]
    public async Task The_same_event_delivered_twice_counts_once()
    {
        var article = Guid.NewGuid();

        await Update(article, 1, "dragons");
        await Update(article, 1, "dragons");

        CountOf("dragons").ShouldBe(1);
        _unitOfWork.SaveCount.ShouldBe(1);
    }

    [Fact]
    public async Task An_older_event_arriving_late_changes_nothing()
    {
        var article = Guid.NewGuid();
        await Update(article, 2, "dragons", "flying");

        await Update(article, 1, "dragons", "training");

        _tags.Tags.Select(tag => tag.Id.Value).ShouldBe(["dragons", "flying"], ignoreOrder: true);
    }

    [Fact]
    public async Task A_deleted_article_gives_up_its_tags_and_the_ones_nobody_else_uses_leave_the_catalog()
    {
        var article = Guid.NewGuid();
        await Update(article, 1, "dragons", "training");
        await Update(Guid.NewGuid(), 1, "dragons");

        await Remove(article, 2);

        _tags.Tags.ShouldHaveSingleItem().Id.Value.ShouldBe("dragons");
        CountOf("dragons").ShouldBe(1);
    }

    [Fact]
    public async Task A_deletion_delivered_twice_releases_once()
    {
        var article = Guid.NewGuid();
        await Update(article, 1, "dragons");
        await Update(Guid.NewGuid(), 1, "dragons");

        await Remove(article, 2);
        await Remove(article, 2);

        CountOf("dragons").ShouldBe(1);
    }

    [Fact]
    public async Task An_edit_arriving_after_the_deletion_does_not_bring_the_tags_back()
    {
        var article = Guid.NewGuid();
        await Update(article, 1, "dragons");
        await Remove(article, 3);

        await Update(article, 2, "dragons", "flying");

        _tags.Tags.ShouldBeEmpty();
    }

    [Fact]
    public async Task An_unusable_tag_name_is_rejected_and_leaves_the_catalog_untouched()
    {
        var result = await Update(Guid.NewGuid(), 1, "dragons", "");

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Validation);
        _tags.Tags.ShouldBeEmpty();
        _usages.Usages.ShouldBeEmpty();
        _unitOfWork.SaveCount.ShouldBe(0);
    }
}
