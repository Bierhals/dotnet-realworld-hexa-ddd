using System.Threading;
using System.Threading.Tasks;
using ErrorOr;

namespace Conduit.Shared.Application.Cqrs;

public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    public Task<ErrorOr<Success>> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public Task<ErrorOr<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
}
