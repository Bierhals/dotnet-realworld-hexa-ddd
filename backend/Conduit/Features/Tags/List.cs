using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Infrastructure;
using Conduit.Shared.RequestHandling;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Features.Tags;

public class List
{
    public record Query : IQuery<TagsEnvelope>;

    public class QueryHandler(ConduitContext context) : IQueryHandler<Query, TagsEnvelope>
    {
        public async Task<TagsEnvelope> Handle(Query message, CancellationToken cancellationToken)
        {
            var tags = await context
                .Tags.OrderBy(x => x.TagId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
            return new TagsEnvelope
            {
                Tags = tags?.Select(x => x.TagId ?? string.Empty).ToList() ?? new List<string>(),
            };
        }
    }
}
