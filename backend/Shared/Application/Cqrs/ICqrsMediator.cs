using System.Threading;
using System.Threading.Tasks;
using ErrorOr;

namespace Conduit.Shared.Application.Cqrs;

public interface ICqrsMediator
{
    public Task<ErrorOr<Success>> Send(ICommand command, CancellationToken ct);
    public Task<ErrorOr<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken ct);
    public Task<ErrorOr<TResponse>> Send<TResponse>(IQuery<TResponse> query, CancellationToken ct);
}
