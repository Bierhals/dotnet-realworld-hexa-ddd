using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Articles.Application.Commands.CreateArticle;
using Conduit.Articles.Application.UnitTests.TestDoubles;
using ErrorOr;
using Shouldly;

namespace Conduit.Articles.Application.UnitTests.Commands.CreateArticle;

public class CreateArticleHandlerTests
{
    private readonly FakeArticlesRepository _articles = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private Task<ErrorOr<string>> Create(
        string title = "How to train your dragon",
        string? author = "alice",
        params string[] tags) =>
        new CreateArticleHandler(_articles, _unitOfWork, new StubCurrentUserAccessor(author))
            .Handle(
                new CreateArticleCommand
                {
                    Title = title,
                    Description = "Ever wonder how?",
                    Body = "You have to believe",
                    TagList = tags,
                },
                CancellationToken.None);

    [Fact]
    public async Task A_new_article_is_stored_under_the_slug_it_reports_back()
    {
        var result = await Create();

        result.IsError.ShouldBeFalse();
        result.Value.ShouldBe("how-to-train-your-dragon");
        _articles.Articles.ShouldHaveSingleItem().Author.Value.ShouldBe("alice");
        _unitOfWork.SaveCount.ShouldBe(1);
    }

    [Fact]
    public async Task An_article_with_an_unusable_tag_name_is_not_stored()
    {
        var result = await Create(tags: [new string('x', 200)]);

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Validation);
        _articles.Articles.ShouldBeEmpty();
        _unitOfWork.SaveCount.ShouldBe(0);
    }

    [Fact]
    public async Task An_article_without_a_title_is_rejected()
    {
        var result = await Create(title: "   ");

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Validation);
        _articles.Articles.ShouldBeEmpty();
    }

    [Fact]
    public async Task Writing_an_article_requires_being_signed_in()
    {
        var result = await Create(author: null);

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Unauthorized);
        _articles.Articles.ShouldBeEmpty();
    }
}
