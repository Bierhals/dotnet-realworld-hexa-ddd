using Wolverine;
using Wolverine.RabbitMQ;

namespace Conduit.Articles.Infrastructure;

/// <summary>
/// Everything this module knows about messaging. Kept in one file so that extracting Articles into
/// its own service is a matter of taking this file along.
/// </summary>
public static class ArticlesMessagingExtensions
{
    /// <summary>
    /// The module's inbound queue - deliberately named after the module and not after domain
    /// events. Once other modules start publishing integration events to Articles they arrive here
    /// too, which adds a publishing route elsewhere but leaves this listener untouched. Domain
    /// events never publish here - see docs/domain-events.md.
    /// </summary>
    public const string QueueName = "conduit.articles";

    public static WolverineOptions AddArticlesMessaging(this WolverineOptions options, bool useRabbitMq)
    {
        options.Discovery.IncludeAssembly(typeof(ArticlesMessagingExtensions).Assembly);

        if (useRabbitMq)
        {
            options.ListenToRabbitQueue(QueueName).UseDurableInbox();
        }

        return options;
    }
}
