# Patterns and Rules

This document covers patterns that show up across the codebase but aren't
themselves DDD tactical patterns (see [DDD Patterns](ddd-patterns.md) for
those), plus a set of cross-cutting rules that apply project-wide.

## 1. CQRS / Mediator (Commands & Queries)

**Purpose:** a single, uniform entry point for use cases. This is an
application-layer/messaging pattern, not a DDD building block — it sits one
level above the Domain layer described in [DDD Patterns](ddd-patterns.md),
and is kept **separate** from Domain Event dispatch:

| | Domain Event | Command | Query |
|---|---|---|---|
| Number of handlers | 0 to N | exactly 1 | exactly 1 |
| Timing | after a successful aggregate change | entry point of a use case | entry point of a use case |
| Return value | none (fire-and-forget) | `ErrorOr<Success>` / `ErrorOr<T>` | `ErrorOr<T>` |
| Delivery | via Outbox, potentially async/delayed | synchronous, within the transaction | synchronous, usually read-only |

They are kept as separate interfaces and dispatch paths because of these
differing characteristics (cardinality, timing, error handling), even though
both share the underlying principle of "one interface per message type,
handler resolved via DI".

**Implementation**

```csharp
public interface ICommand { }
public interface ICommand<TResponse> { }
public interface IQuery<TResponse> { }

public interface ICommandHandler<TCommand> where TCommand : ICommand
{
    Task<ErrorOr<Success>> Handle(TCommand command, CancellationToken ct);
}

public interface ICommandHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>
{
    Task<ErrorOr<TResponse>> Handle(TCommand command, CancellationToken ct);
}

public interface IQueryHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    Task<ErrorOr<TResponse>> Handle(TQuery query, CancellationToken ct);
}

public interface ISender
{
    Task<ErrorOr<Success>> Send(ICommand command, CancellationToken ct);
    Task<ErrorOr<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken ct);
    Task<ErrorOr<TResponse>> Send<TResponse>(IQuery<TResponse> query, CancellationToken ct);
}
```

All three variants (`ICommand`, `ICommand<T>`, `IQuery<T>`) consistently
return `ErrorOr<...>` — there is no difference in shape between "with" and
"without" a payload.

**Example handler**

```csharp
public sealed record PublishArticleCommand(Guid ArticleId) : ICommand<PublishArticleResult>;

public sealed class PublishArticleCommandHandler : ICommandHandler<PublishArticleCommand, PublishArticleResult>
{
    private readonly IArticleRepository _articleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<ErrorOr<PublishArticleResult>> Handle(PublishArticleCommand command, CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);

        var article = await _articleRepository.GetByIdAsync(new ArticleId(command.ArticleId), ct);
        if (article is null)
        {
            await _unitOfWork.RollbackAsync(ct);
            return Error.NotFound("Article.NotFound", "Article was not found.");
        }

        var result = article.Publish();
        if (result.IsError)
        {
            await _unitOfWork.RollbackAsync(ct);
            return result.Errors;
        }

        await _unitOfWork.CommitAsync(ct); // outbox write happens here automatically, invisibly
        return article.ToPublishResult(); // mapping to DTO, see §2 below
    }
}
```

## 2. Response Mapping (Command/Query → DTO)

**Purpose:** Application handlers **never** return Domain objects (no
`Article`, `ArticleId`, or any other Domain type leaves the Application
layer).

**Rules**
- Explicit mapping to an Application DTO before the handler returns.
- Rationale: decoupling from Domain structure, and avoiding serialization
  problems with private constructors/internal types that Domain objects
  often have on purpose.
- Simple cases: manual mapping inline in the handler, or as an extension
  method. More complex aggregates: a dedicated mapper class.

This is what keeps [Contracts](modulith-architecture.md) DTOs and Application
DTOs consistent: neither ever wraps a raw Domain type, so a Domain refactor
inside one module can't silently break another module or an API consumer.

## 3. Further Patterns

- **Mediator** — the CQRS dispatcher in §1 is a Mediator: callers depend only
  on `ISender`, not on concrete handlers, decoupling the caller from handler
  resolution/wiring.
- **Decorator** — cross-cutting behavior (e.g. wrapping a command handler in
  a database transaction) is added by wrapping the handler in another class
  implementing the same interface, rather than baking the concern into every
  handler:

  ```csharp
  public sealed class TransactionalCommandHandlerDecorator<TCommand, TResponse>(
      IUnitOfWork unitOfWork, ICommandHandler<TCommand, TResponse> inner)
      : ICommandHandler<TCommand, TResponse>
      where TCommand : ICommand<TResponse>
  {
      public async Task<ErrorOr<TResponse>> Handle(TCommand command, CancellationToken ct)
      {
          await unitOfWork.BeginTransactionAsync(ct);
          var result = await inner.Handle(command, ct);
          if (result.IsError)
          {
              await unitOfWork.RollbackAsync(ct);
              return result.Errors;
          }
          await unitOfWork.CommitAsync(ct);
          return result;
      }
  }
  ```

- **Interceptor** — EF Core's `SaveChangesInterceptor` is used to collect
  Domain Events from the `ChangeTracker` before `SaveChanges()` and turn them
  into Outbox rows (see [DDD Patterns §4](ddd-patterns.md)) — a cross-cutting
  concern hooked into the persistence pipeline rather than scattered across
  every repository/handler.
- **Strategy** — used wherever a behavior needs to be swappable at runtime or
  per-configuration without an `if/switch` inside Domain/Application code
  (e.g. a pluggable password-hashing or token-generation strategy behind an
  interface).

## 4. Further Rules

- **Introduce an interface or base class only for a concrete reason** — never
  as a generic abstraction "just in case" (applies equally to repositories,
  factories, domain services — see [DDD Patterns](ddd-patterns.md)).
- **Ubiquitous language over technical labeling** — no `MoneyValueObject`, no
  `ArticlePublishedEvent`; the classification comes from the
  folder/namespace, not the class name.
- **Use `ErrorOr` consistently** for domain-level outcomes (business rules,
  aggregate methods, commands, queries) — exceptions stay reserved for
  programming errors and genuine technical failures.
- **Early-exit on rule checks** — don't collect multiple errors in a chain;
  the first failing rule wins.
- **Domain stays persistence- and transaction-free** — `IUnitOfWork` is only
  ever used from the Application layer.
- **Domain objects never leave the Application layer** — always map to a DTO
  at the boundary (see §2).
- **Project/root namespace naming stays consistent**: `RootNamespace` =
  `AssemblyName` = project name (see [Module Structure §1](module-structure.md)).
- **The layer name is always present in the namespace**, so a layer
  violation is visible in a `using` statement before any tooling flags it.

## Further Reading

- Martin Fowler, [*Mediator*](https://martinfowler.com/eaaDev/EventMediator.html) and [*Decorator*](https://en.wikipedia.org/wiki/Decorator_pattern) — the generic pattern descriptions behind §3.
- Microsoft Learn, [*EF Core: SaveChanges interceptors*](https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors) — the interception mechanism used for the Outbox flow.
- Refactoring Guru, [*Design Patterns catalogue*](https://refactoring.guru/design-patterns) — reference catalogue for Mediator, Decorator, and Strategy.
- [ErrorOr](https://github.com/amantinband/error-or) — the library used throughout for `ErrorOr<T>` return values.
