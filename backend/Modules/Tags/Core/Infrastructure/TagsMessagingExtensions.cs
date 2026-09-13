using Wolverine;
using Wolverine.RabbitMQ;

namespace Conduit.Tags.Core.Infrastructure;

/// <summary>
/// Everything this module knows about messaging. Kept in one file so that extracting Tags into its
/// own service is a matter of taking this file along.
/// </summary>
public static class TagsMessagingExtensions
{
    /// <summary>
    /// The module's inbound queue - deliberately named after the module and not after domain
    /// events. Once other modules start publishing integration events to Tags they arrive here too,
    /// which adds a publishing route elsewhere but leaves this listener untouched. Domain events
    /// never publish here - see docs/domain-events.md.
    /// </summary>
    public const string QueueName = "conduit.tags";

    public static WolverineOptions AddTagsMessaging(this WolverineOptions options, bool useRabbitMq)
    {
        options.Discovery.IncludeAssembly(typeof(TagsMessagingExtensions).Assembly);

        if (useRabbitMq)
        {
            options.ListenToRabbitQueue(QueueName).UseDurableInbox();
        }

        return options;
    }
}
