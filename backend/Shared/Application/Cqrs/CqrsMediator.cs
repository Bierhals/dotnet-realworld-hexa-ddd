using System;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Shared.Application.Cqrs;

public sealed class CqrsMediator : ICqrsMediator
{
    private readonly IServiceProvider _sp;
    public CqrsMediator(IServiceProvider sp) => _sp = sp;

    public Task<ErrorOr<Success>> Send(ICommand command, CancellationToken ct)
        => InvokeHandler<Success>(typeof(ICommandHandler<>), command, ct, singleGenericArg: true);

    public Task<ErrorOr<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken ct)
        => InvokeHandler<TResponse>(typeof(ICommandHandler<,>), command, ct);

    public Task<ErrorOr<TResponse>> Send<TResponse>(IQuery<TResponse> query, CancellationToken ct)
        => InvokeHandler<TResponse>(typeof(IQueryHandler<,>), query, ct);

    private Task<ErrorOr<TResponse>> InvokeHandler<TResponse>(
        Type openHandlerType, object request, CancellationToken ct, bool singleGenericArg = false)
    {
        var handlerType = singleGenericArg
            ? openHandlerType.MakeGenericType(request.GetType())
            : openHandlerType.MakeGenericType(request.GetType(), typeof(TResponse));

        dynamic handler = _sp.GetRequiredService(handlerType);
        return handler.Handle((dynamic)request, ct);
    }
}
