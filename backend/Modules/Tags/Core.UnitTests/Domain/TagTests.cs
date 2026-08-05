using System.Linq;
using Conduit.Tags.Core.Domain;
using Conduit.Tags.Core.Domain.Events;
using ErrorOr;
using Shouldly;

namespace Conduit.Tags.Core.UnitTests.Domain;

public class TagTests
{
    private static Tag ATag(string name = "dragons") => Tag.Create(TagName.Create(name).Value);

    [Fact]
    public void A_tag_that_enters_the_catalog_is_not_referenced_by_anything_yet()
    {
        var tag = ATag();

        tag.ReferenceCount.ShouldBe(0);
        tag.IsUnreferenced.ShouldBeTrue();
        tag.DomainEvents.OfType<TagAddedToCatalogDomainEvent>().Single().TagName.ShouldBe("dragons");
    }

    [Fact]
    public void Referencing_a_tag_raises_its_reference_count()
    {
        var tag = ATag();

        tag.Reference();
        tag.Reference();

        tag.ReferenceCount.ShouldBe(2);
        tag.IsUnreferenced.ShouldBeFalse();
    }

    [Fact]
    public void Releasing_one_of_several_references_keeps_the_tag_in_the_catalog()
    {
        var tag = ATag();
        tag.Reference();
        tag.Reference();
        tag.ClearDomainEvents();

        var result = tag.Release();

        result.IsError.ShouldBeFalse();
        tag.ReferenceCount.ShouldBe(1);
        tag.IsUnreferenced.ShouldBeFalse();
        tag.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void Releasing_the_last_reference_announces_that_the_tag_leaves_the_catalog()
    {
        var tag = ATag();
        tag.Reference();
        tag.ClearDomainEvents();

        var result = tag.Release();

        result.IsError.ShouldBeFalse();
        tag.IsUnreferenced.ShouldBeTrue();
        tag.DomainEvents.OfType<TagRemovedFromCatalogDomainEvent>().Single().TagName.ShouldBe("dragons");
    }

    [Fact]
    public void A_tag_that_nothing_references_cannot_be_released()
    {
        var tag = ATag();

        var result = tag.Release();

        result.IsError.ShouldBeTrue();
        result.FirstError.Type.ShouldBe(ErrorType.Validation);
        tag.ReferenceCount.ShouldBe(0);
    }
}
