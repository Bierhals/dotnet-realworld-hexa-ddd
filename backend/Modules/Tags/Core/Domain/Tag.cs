using Conduit.Shared.Domain;
using Conduit.Tags.Core.Domain.Events;
using Conduit.Tags.Core.Domain.Rules;
using ErrorOr;

namespace Conduit.Tags.Core.Domain;

/// <summary>
/// An entry of the tag catalog. A tag knows how many articles currently reference it and leaves
/// the catalog once that count drops back to zero.
/// </summary>
public sealed class Tag : AggregateRoot<TagName>
{
    public int ReferenceCount { get; private set; }

    private Tag() { } // for EF Core

    private Tag(TagName name) : base(name) => ReferenceCount = 0;

    public bool IsUnreferenced => ReferenceCount == 0;

    public static Tag Create(TagName name)
    {
        var tag = new Tag(name);
        tag.AddDomainEvent(new TagAddedToCatalogDomainEvent(name.Value));

        return tag;
    }

    public void Reference() => ReferenceCount++;

    public ErrorOr<Success> Release()
    {
        var check = new TagMustBeReferenced(ReferenceCount).Check();
        if (check.IsError)
        {
            return check.Errors;
        }

        ReferenceCount--;

        if (IsUnreferenced)
        {
            AddDomainEvent(new TagRemovedFromCatalogDomainEvent(Id.Value));
        }

        return Result.Success;
    }
}
