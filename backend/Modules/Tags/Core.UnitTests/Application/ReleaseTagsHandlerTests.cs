using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Tags.Core.Application.Commands.ReleaseTags;
using Conduit.Tags.Core.UnitTests.TestDoubles;
using ErrorOr;
using Shouldly;

namespace Conduit.Tags.Core.UnitTests.Application;

public class ReleaseTagsHandlerTests
{
    private readonly FakeTagsRepository _tags = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private Task<ErrorOr<Success>> Release(params string[] names) =>
        new ReleaseTagsHandler(_tags, _unitOfWork)
            .Handle(new ReleaseTagsCommand { TagNames = names }, CancellationToken.None);

    [Fact]
    public async Task Giving_up_one_of_several_uses_keeps_the_tag_in_the_catalog()
    {
        _tags.Seed("dragons", referenceCount: 2);

        var result = await Release("dragons");

        result.IsError.ShouldBeFalse();
        _tags.Tags.ShouldHaveSingleItem().ReferenceCount.ShouldBe(1);
    }

    [Fact]
    public async Task Giving_up_the_last_use_of_a_tag_removes_it_from_the_catalog()
    {
        _tags.Seed("dragons", referenceCount: 1);

        await Release("dragons");

        _tags.Tags.ShouldBeEmpty();
        _unitOfWork.SaveCount.ShouldBe(1);
    }

    [Fact]
    public async Task Only_the_tags_that_lost_their_last_use_leave_the_catalog()
    {
        _tags.Seed("dragons", referenceCount: 1);
        _tags.Seed("training", referenceCount: 2);

        await Release("dragons", "training");

        _tags.Tags.ShouldHaveSingleItem().Id.Value.ShouldBe("training");
    }

    [Fact]
    public async Task Giving_up_a_tag_the_catalog_never_knew_is_silently_accepted()
    {
        var result = await Release("dragons");

        result.IsError.ShouldBeFalse();
        _tags.Tags.ShouldBeEmpty();
    }

    [Fact]
    public async Task An_unusable_tag_name_is_rejected_and_leaves_the_catalog_untouched()
    {
        _tags.Seed("dragons", referenceCount: 1);

        var result = await Release("dragons", "");

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Validation);
        _tags.Tags.ShouldHaveSingleItem().ReferenceCount.ShouldBe(1);
        _unitOfWork.SaveCount.ShouldBe(0);
    }
}
