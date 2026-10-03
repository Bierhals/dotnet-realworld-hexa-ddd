using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application.Commands.EditArticle;
using Conduit.Articles.Application.UnitTests.TestDoubles;
using Conduit.Shared.Application.Optional;
using ErrorOr;
using Shouldly;

namespace Conduit.Articles.Application.UnitTests.Commands.EditArticle;

public class EditArticleHandlerTests
{
    private readonly FakeArticlesRepository _articles = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private Task<ErrorOr<string>> Edit(
        string slug,
        string? editor = "alice",
        Optional<string> title = default,
        Optional<string> body = default,
        Optional<string[]> tags = default) =>
        new EditArticleHandler(_articles, _unitOfWork, new StubCurrentUserAccessor(editor))
            .Handle(
                new EditArticleCommand { Slug = slug, Title = title, Body = body, TagList = tags },
                CancellationToken.None);

    [Fact]
    public async Task Renaming_an_article_reports_the_slug_it_moved_to()
    {
        _articles.Seed();

        var result = await Edit("how-to-train-your-dragon", title: new Optional<string>("How to tame your dragon"));

        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe("how-to-tame-your-dragon");
        _unitOfWork.SaveCount.ShouldBe(1);
    }

    [Fact]
    public async Task An_article_you_do_not_own_cannot_be_edited()
    {
        var article = _articles.Seed();

        var result = await Edit("how-to-train-your-dragon", editor: "bob", title: new Optional<string>("Hijacked"));

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Forbidden);
        article.Title.Value.ShouldBe("How to train your dragon");
        _unitOfWork.SaveCount.ShouldBe(0);
    }

    [Fact]
    public async Task Editing_an_article_that_does_not_exist_reports_it_as_missing()
    {
        var result = await Edit("no-such-article", title: new Optional<string>("Whatever"));

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.NotFound);
    }

    [Fact]
    public async Task An_edit_with_an_unusable_tag_name_is_not_persisted()
    {
        _articles.Seed();

        var result = await Edit("how-to-train-your-dragon", tags: new Optional<string[]>([new string('x', 200)]));

        result.IsError.ShouldBeTrue();
        _unitOfWork.SaveCount.ShouldBe(0);
    }
}
