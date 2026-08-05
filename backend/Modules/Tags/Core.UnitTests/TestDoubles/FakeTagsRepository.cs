using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Tags.Core.Domain;

namespace Conduit.Tags.Core.UnitTests.TestDoubles;

internal sealed class FakeTagsRepository : ITagsRepository
{
    private readonly List<Tag> _tags = [];

    public IReadOnlyCollection<Tag> Tags => _tags;

    public void Seed(string name, int referenceCount)
    {
        var tag = Tag.Create(TagName.Create(name).Value);
        for (var i = 0; i < referenceCount; i++)
        {
            tag.Reference();
        }

        tag.ClearDomainEvents();
        _tags.Add(tag);
    }

    public Task<IReadOnlyCollection<Tag>> GetByNamesAsync(IReadOnlyCollection<TagName> names, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Tag> found = [.. _tags.Where(tag => names.Contains(tag.Id))];

        return Task.FromResult(found);
    }

    public void Add(Tag tag) => _tags.Add(tag);

    public void Remove(Tag tag) => _tags.Remove(tag);
}
