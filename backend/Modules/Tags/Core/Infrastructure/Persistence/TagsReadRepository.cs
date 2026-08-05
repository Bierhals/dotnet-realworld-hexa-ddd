using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Tags.Core.Application;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Tags.Core.Infrastructure.Persistence;

public sealed class TagsReadRepository(TagsDbContext dbContext) : ITagsReadRepository
{
    public async Task<IReadOnlyCollection<string>> GetTagNamesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Tags.AsNoTracking()
            .OrderBy(tag => tag.Id)
            .Select(tag => tag.Id.Value)
            .ToListAsync(cancellationToken);
}
