using System.Threading.Tasks;

namespace Conduit.Shared.Domain;

public interface IDomainEventHandler<T> where T : DomainEvent
{
    Task Handle(T domainEvent);
}
