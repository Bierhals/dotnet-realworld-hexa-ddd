using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Shared.RequestHandling;

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    public Task<TResponse> Handle(TQuery query, CancellationToken cancellationToken);
}