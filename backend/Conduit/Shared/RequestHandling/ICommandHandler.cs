using System.Threading;
using System.Threading.Tasks;

namespace Conduit.Shared.RequestHandling;

public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    public Task Handle(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public Task<TResponse> Handle(TCommand command, CancellationToken cancellationToken);
}