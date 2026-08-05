using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Tags.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Tags.Core.Infrastructure.Persistence;

public sealed class TagsRepository(TagsDbContext dbContext) : ITagsRepository
{
    public async Task<IReadOnlyCollection<Tag>> GetByNamesAsync(IReadOnlyCollection<TagName> names, CancellationToken cancellationToken = default) =>
        await dbContext.Tags.Where(tag => names.Contains(tag.Id)).ToListAsync(cancellationToken);

    public void Add(Tag tag) => dbContext.Tags.Add(tag);

    public void Remove(Tag tag) => dbContext.Tags.Remove(tag);
}
