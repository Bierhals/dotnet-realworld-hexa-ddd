using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Shared.RequestHandling;
using Microsoft.EntityFrameworkCore;

namespace Conduit.Infrastructure;

public class DBContextTransacionCommandDecorator<TComand, TResponse>(ConduitContext context, ICommandHandler<TComand, TResponse> next)
    : ICommandHandler<TComand, TResponse>
    where TComand : ICommand<TResponse>
{
    public async Task<TResponse> Handle(TComand request, CancellationToken cancellationToken)
    {
        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            try
            {
                context.BeginTransaction();
                var result = await next.Handle(request, cancellationToken);
                context.CommitTransaction();

                return result;
            }
            catch (Exception)
            {
                context.RollbackTransaction();
                throw;
            }
        });
    }
}

public class DBContextTransacionCommandDecorator<TComand>(ConduitContext context, ICommandHandler<TComand> next)
    : ICommandHandler<TComand>
    where TComand : ICommand
{
    public async Task Handle(TComand request, CancellationToken cancellationToken)
    {
        var strategy = context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            try
            {
                context.BeginTransaction();
                await next.Handle(request, cancellationToken);
                context.CommitTransaction();
            }
            catch (Exception)
            {
                context.RollbackTransaction();
                throw;
            }
        });
    }
}
