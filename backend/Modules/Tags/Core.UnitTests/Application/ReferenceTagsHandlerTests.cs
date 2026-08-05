using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Tags.Core.Application.Commands.ReferenceTags;
using Conduit.Tags.Core.UnitTests.TestDoubles;
using ErrorOr;
using Shouldly;

namespace Conduit.Tags.Core.UnitTests.Application;

public class ReferenceTagsHandlerTests
{
    private readonly FakeTagsRepository _tags = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private Task<ErrorOr<Success>> Reference(params string[] names) =>
        new ReferenceTagsHandler(_tags, _unitOfWork)
            .Handle(new ReferenceTagsCommand { TagNames = names }, CancellationToken.None);

    [Fact]
    public async Task Using_a_tag_nobody_used_before_adds_it_to_the_catalog()
    {
        var result = await Reference("dragons");

        result.IsError.ShouldBeFalse();
        var tag = _tags.Tags.ShouldHaveSingleItem();
        tag.Id.Value.ShouldBe("dragons");
        tag.ReferenceCount.ShouldBe(1);
    }

    [Fact]
    public async Task Using_a_tag_that_is_already_in_the_catalog_reuses_the_existing_entry()
    {
        _tags.Seed("dragons", referenceCount: 2);

        await Reference("dragons");

        var tag = _tags.Tags.ShouldHaveSingleItem();
        tag.ReferenceCount.ShouldBe(3);
    }

    [Fact]
    public async Task Listing_the_same_tag_twice_counts_as_a_single_use()
    {
        await Reference("dragons", "dragons");

        _tags.Tags.ShouldHaveSingleItem().ReferenceCount.ShouldBe(1);
    }

    [Fact]
    public async Task Several_tags_are_written_to_the_catalog_in_one_go()
    {
        await Reference("dragons", "training");

        _tags.Tags.Count.ShouldBe(2);
        _tags.Tags.ShouldAllBe(tag => tag.ReferenceCount == 1);
        _unitOfWork.SaveCount.ShouldBe(1);
    }

    [Fact]
    public async Task An_unusable_tag_name_is_rejected_and_leaves_the_catalog_untouched()
    {
        var result = await Reference("dragons", "   ");

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Validation);
        _tags.Tags.ShouldBeEmpty();
        _unitOfWork.SaveCount.ShouldBe(0);
    }

    [Fact]
    public async Task Referencing_nothing_does_not_touch_the_catalog()
    {
        var result = await Reference();

        result.IsError.ShouldBeFalse();
        _tags.Tags.ShouldBeEmpty();
        _unitOfWork.SaveCount.ShouldBe(0);
    }
}
