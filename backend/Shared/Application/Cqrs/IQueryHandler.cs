using System.Threading;
using System.Threading.Tasks;
using ErrorOr;

namespace Conduit.Shared.Application.Cqrs;

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    public Task<ErrorOr<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}
