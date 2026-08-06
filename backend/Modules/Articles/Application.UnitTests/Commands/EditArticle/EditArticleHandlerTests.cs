using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application.Commands.EditArticle;
using Conduit.Articles.Application.UnitTests.TestDoubles;
using ErrorOr;
using Shouldly;

namespace Conduit.Articles.Application.UnitTests.Commands.EditArticle;

public class EditArticleHandlerTests
{
    private readonly FakeArticlesRepository _articles = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeTagCatalog _tagCatalog = new();

    private Task<ErrorOr<string>> Edit(
        string slug,
        string? editor = "alice",
        string? title = null,
        string? body = null,
        IReadOnlyCollection<string>? tags = null) =>
        new EditArticleHandler(_articles, _unitOfWork, _tagCatalog, new StubCurrentUserAccessor(editor))
            .Handle(
                new EditArticleCommand { Slug = slug, Title = title, Body = body, TagList = tags },
                CancellationToken.None);

    [Fact]
    public async Task Renaming_an_article_reports_the_slug_it_moved_to()
    {
        _articles.Seed();

        var result = await Edit("how-to-train-your-dragon", title: "How to tame your dragon");

        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe("how-to-tame-your-dragon");
        _unitOfWork.SaveCount.ShouldBe(1);
    }

    [Fact]
    public async Task Swapping_a_tag_references_the_new_one_and_releases_the_old_one()
    {
        _articles.Seed("How to train your dragon", "alice", "dragons", "training");
        _tagCatalog.Seed("dragons", "training");

        await Edit("how-to-train-your-dragon", tags: ["dragons", "flying"]);

        _tagCatalog.Tags.ShouldBe(["dragons", "flying"], ignoreOrder: true);
        _tagCatalog.ReferenceCountOf("dragons").ShouldBe(1);
        _tagCatalog.ReferenceCountOf("flying").ShouldBe(1);
    }

    [Fact]
    public async Task Leaving_the_tags_out_of_an_edit_does_not_touch_the_catalog()
    {
        _articles.Seed("How to train your dragon", "alice", "dragons");
        _tagCatalog.Seed("dragons");

        await Edit("how-to-train-your-dragon", body: "Believe harder");

        _tagCatalog.ReferenceCountOf("dragons").ShouldBe(1);
    }

    [Fact]
    public async Task An_article_you_do_not_own_cannot_be_edited()
    {
        var article = _articles.Seed();

        var result = await Edit("how-to-train-your-dragon", editor: "bob", title: "Hijacked");

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Forbidden);
        article.Title.Value.ShouldBe("How to train your dragon");
        _unitOfWork.SaveCount.ShouldBe(0);
    }

    [Fact]
    public async Task Editing_an_article_that_does_not_exist_reports_it_as_missing()
    {
        var result = await Edit("no-such-article", title: "Whatever");

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
    }

    [Fact]
    public async Task An_edit_the_catalog_rejects_the_tags_of_is_not_persisted()
    {
        _articles.Seed();
        _tagCatalog.RejectReferencesWith = Error.Validation("Tag.NameTooLong", "too long");

        var result = await Edit("how-to-train-your-dragon", tags: ["flying"]);

        result.IsError.ShouldBeTrue();
        _unitOfWork.SaveCount.ShouldBe(0);
    }
}
