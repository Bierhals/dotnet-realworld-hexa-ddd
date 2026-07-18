# DDD Tactical Patterns

This document describes the Domain-Driven Design (DDD) **tactical** building
blocks used inside a module's Domain layer (see
[Module Structure](module-structure.md) for where the Domain layer sits), when
to use each of them, and how they are implemented in this codebase
(.NET / C#).

Two patterns are intentionally **not** covered here even though they are
sometimes discussed alongside DDD: **CQRS/Mediator** and **Response Mapping**
are application-layer/messaging concerns rather than DDD tactical patterns —
see [Patterns and Rules](patterns-and-rules.md) for those. The
**Specification** pattern is deliberately left out of this document set for
now and can be added once it is actually adopted.

---

## 0. Use Tactical DDD Only for Core Subdomains

Before reaching for any pattern below, first ask: *is this a Core
subdomain?*

Strategic DDD distinguishes three kinds of subdomains:

| Subdomain type | Characteristics | Recommended approach |
|---|---|---|
| **Core** | Where the competitive advantage lives; complex, changes often, worth the investment | Full tactical DDD: aggregates, value objects, domain events, business rules |
| **Supporting** | Necessary for the Core to work, but not itself a differentiator; moderate complexity | Simpler modeling — a lightweight aggregate or even a transaction script; use tactical patterns only where they genuinely reduce complexity |
| **Generic** | Solved problems with well-known solutions (auth, notifications, payments, ...); little to no competitive value | Simple CRUD/transaction scripts, or an off-the-shelf/third-party solution; avoid investing in rich tactical modeling |

**Rule of thumb:** the full weight of Aggregate/Value-Object/Domain-Event
modeling below is expensive to build and maintain. That cost only pays off
where the domain is genuinely complex *and* strategically important — a Core
subdomain. Applying it uniformly everywhere, including Generic/Supporting
subdomains, is a common over-engineering mistake: it adds ceremony without
adding value, since those subdomains don't have complex, differentiating
business rules to protect in the first place.

In practice this means: identify which of a project's modules represent Core
subdomains before applying the patterns in this document, and consciously
allow Supporting/Generic modules to stay closer to a simple CRUD/transaction-
script style even while living in the same Modulith. This decision is
project-specific and should be made explicitly (e.g. in an architecture
decision record) rather than left implicit.

---

## 1. Value Object

**Purpose:** an object without its own identity, defined only by its values,
immutable.

**Implementation**
- `sealed record` by default (value equality, immutability, `with`-support
  come for free).
- `readonly record struct` for small, frequently-copied value objects (e.g.
  `Money`).
- Private constructor + static `Create(...)` factory for validation.
- `Create(...)` returns `ErrorOr<T>` when the values originate from user
  input/external data.
- A constructor-thrown exception is only acceptable for values that are
  guaranteed valid internally (never for external input).
- Collections are never exposed as `List<T>` → use `ImmutableList<T>`, and
  override `Equals`/`GetHashCode` manually with `SequenceEqual` (records
  otherwise compare collections by reference).
- `sealed`, because inheritance doesn't make sense for a Value Object
  fachlich (domain-wise), and record inheritance complicates the
  `EqualityContract` mechanism unnecessarily.
- Naming: use the domain name without a technical suffix (`Money`, not
  `MoneyValueObject`).
- Organization: a `ValueObjects/` folder/namespace signals the
  classification — it does not need to be repeated in the class name.

**Strongly Typed ID** (a special case of Value Object)
- `readonly record struct` wrapping a `Guid`/`int` value, e.g.
  `ArticleId(Guid Value)`.
- Placed directly next to its owning Entity (not in the general
  `ValueObjects/` folder), since it is tightly coupled to exactly one Entity.

```csharp
public sealed record EmailAddress
{
    public string Value { get; }
    private EmailAddress(string value) => Value = value;

    public static ErrorOr<EmailAddress> Create(string value)
    {
        var check = new EmailMustContainAtSignRule(value).Check();
        if (check.IsError) return check.Errors;
        return new EmailAddress(value.Trim().ToLowerInvariant());
    }
}

public readonly record struct ArticleId(Guid Value)
{
    public static ArticleId New() => new(Guid.NewGuid());
}
```

## 2. Entity

**Purpose:** an object with its own identity. Equality is based **only** on
the identity, never on attributes.

**Implementation**
- A plain `class`, **not** a `record` (record equality would compare every
  property, not just the identity).
- Base class `Entity<TId>` with `Equals`/`GetHashCode` based on `Id` plus a
  `GetType()` check (prevents a false-positive equality between two
  different Entity types that happen to share an `Id` value).
- `Id` uses `init`, since the identity is known before the entity is
  created (generated externally, e.g. via a DB sequence or
  `Guid.NewGuid()` beforehand).
- Private constructor + static `Create(...)` factory method.
- No primary constructor — entities need private setters, validation, and
  multiple construction paths (`Create` vs. EF Core rehydration).

```csharp
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    public TId Id { get; init; } = default!;

    protected Entity() { } // EF Core
    protected Entity(TId id) => Id = id;

    protected static ErrorOr<Success> CheckRule(IBusinessRule rule) => rule.Check();

    public bool Equals(Entity<TId>? other)
    {
        if (other is null || GetType() != other.GetType()) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }

    public override bool Equals(object? obj) => Equals(obj as Entity<TId>);
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => Equals(left, right);
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !Equals(left, right);
}
```

## 3. Aggregate Root

**Purpose:** a cluster of related entities/value objects that forms one
transactional and consistency boundary. It is the only entry point from
outside into that cluster.

**Rules**
- Only the root has a repository — never one for an internal entity.
- References to entities in **other** aggregates are always by ID, never by
  object reference.
- Methods that mutate internal entities are `internal` — only the root may
  call them.
- Domain actions (`Submit()`, `AddComment()`) return `ErrorOr<Success>`
  rather than throwing.
- Domain events are raised only **after** a rule check succeeds (no event for
  an action that ultimately fails).

```csharp
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot() { }
    protected AggregateRoot(TId id) : base(id) { }

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

```csharp
public sealed class Article : AggregateRoot<ArticleId>
{
    private readonly List<Comment> _comments = new();
    public IReadOnlyList<Comment> Comments => _comments.AsReadOnly();
    public ArticleStatus Status { get; private set; }

    public ErrorOr<Success> Publish()
    {
        var titleCheck = new ArticleMustHaveTitleRule(Title).Check();
        if (titleCheck.IsError) return titleCheck.Errors;

        var statusCheck = new ArticleMustBeInDraftStatusRule(Status).Check();
        if (statusCheck.IsError) return statusCheck.Errors;

        Status = ArticleStatus.Published;
        RaiseDomainEvent(new ArticlePublished(Id, AuthorId));
        return Result.Success;
    }
}
```

## 4. Domain Event

**Purpose:** represents something that **has happened** in the domain. Raised
after a successful aggregate change.

**Implementation**
- `IDomainEvent` interface (no forced base class for the core concept).
- Concrete events are `sealed record`s.
- Shared technical infrastructure (`Id`, `OccurredOnUtc`) lives in an
  `abstract record DomainEvent` — inheritance on records is unproblematic
  here, because domain events are never compared across types.
- Names are in past tense (`ArticlePublished`, not `ArticlePublishedEvent`).
- Organization: a dedicated `Events/` subfolder per aggregate.

```csharp
public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOnUtc { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}

public sealed record ArticlePublished(ArticleId ArticleId, PersonId AuthorId) : DomainEvent;
```

**Processing (Outbox pattern)**
1. A `SavingChangesInterceptor` collects events from the `ChangeTracker`
   **before** `SaveChanges()` and writes them as `OutboxMessage` rows in the
   same DB transaction → atomic delivery guarantee.
2. A background process (polling, or Postgres `LISTEN`/`NOTIFY`) reads the
   outbox and calls the `DomainEventDispatcher`.
3. This flow is **completely invisible to the Application layer** — a
   command handler just calls `IUnitOfWork.CommitAsync()` and knows nothing
   about the outbox mechanism behind it.
4. Exception: if a synchronous side effect is strictly required before the
   response is sent, that is solved explicitly via a separate
   `IDomainEventPublisher` called from the Application handler — **not**
   via the outbox path.

```csharp
public sealed class DomainEventDispatcher
{
    private readonly IServiceProvider _sp;
    public DomainEventDispatcher(IServiceProvider sp) => _sp = sp;

    public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct)
    {
        foreach (var domainEvent in events)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            foreach (var handler in _sp.GetServices(handlerType))
            {
                var method = handlerType.GetMethod("Handle")!;
                await (Task)method.Invoke(handler, new object[] { domainEvent, ct })!;
            }
        }
    }
}
```

## 5. Business Rule

**Purpose:** replaces plain `if/throw` guards for domain invariants with
named, testable, reusable rule objects.

**Implementation**
- `IBusinessRule` with a single `Check()` method returning `ErrorOr<Success>`
  directly.
- Used both inside aggregate methods and inside a value object's
  `Create(...)`.
- Consistent **early-exit** on every check — errors are not collected into a
  list (no `CheckAll`).

```csharp
public interface IBusinessRule
{
    ErrorOr<Success> Check();
}

public sealed class ArticleMustHaveTitleRule : IBusinessRule
{
    private readonly string _title;
    public ArticleMustHaveTitleRule(string title) => _title = title;

    public ErrorOr<Success> Check()
        => string.IsNullOrWhiteSpace(_title)
            ? Error.Validation("Article.NoTitle", "An article without a title cannot be published.")
            : Result.Success;
}
```

## 6. Domain Service

**Purpose:** domain logic that spans **multiple aggregates at once**, or that
doesn't naturally belong to the responsibility of a single aggregate (e.g.
transferring money between two accounts).

**Rules**
- No forced base class.
- An interface is only introduced for genuine dependencies (DI/mocking) or
  swappable strategies (e.g. `IPricingStrategy`).
- Purely functional services with no dependencies: a plain `static class` is
  enough.
- Stays completely persistence-free — **no** `IUnitOfWork`, no transaction
  control inside the Domain.
- Before reaching for a domain service, check: doesn't this logic actually
  belong on the aggregate root? (The most common modeling mistake here is an
  Anemic Domain Model.)

```csharp
public static class FollowPolicy
{
    public static ErrorOr<Success> Follow(Person follower, Person followee)
    {
        if (follower.Id == followee.Id)
            return Error.Validation("Follow.SelfFollow", "An account cannot follow itself.");

        follower.Follow(followee.Id);
        return Result.Success;
    }
}
```

## 7. Repository

**Purpose:** abstracts access to an aggregate as a whole.

**Rules**
- One interface **per aggregate root**, defined in the **Domain** layer
  (Dependency Inversion — Domain defines what it needs, Infrastructure
  supplies the technology). This is the Repository's role as a *port*; see
  [Module Structure §2](module-structure.md).
- **No** generic `IRepository<T, TId>` — that invites CRUD thinking instead
  of ubiquitous language, and produces methods nobody actually needs.
- Only domain-meaningful, clearly named methods (`GetPublishedArticlesAsync`,
  not `GetAllAsync`).
- The `DbContext` is injected directly into the repository.
- **No** `SaveChangesAsync()` inside the repository — only tracking (`Add`,
  change tracking); the actual save runs centrally through `IUnitOfWork`.

```csharp
// Domain
public interface IArticleRepository
{
    Task<Article?> GetByIdAsync(ArticleId id, CancellationToken ct);
    Task AddAsync(Article article, CancellationToken ct);
}
```

**A separate read repository for queries (CQRS separation)**
- Its own interface in the **Application** layer (not Domain), e.g.
  `IArticleReadRepository`.
- Returns **DTOs/read models**, never Domain types (no `Article`, no
  `ArticleId`).
- Free to use projected, denormalized queries directly (a performance win —
  no need to rebuild the aggregate for pure display purposes).

```csharp
// Application
public interface IArticleReadRepository
{
    Task<ArticleSummaryDto?> GetSummaryAsync(Guid articleId, CancellationToken ct);
}
```

## 8. Factory

**Purpose:** encapsulates complex creation logic.

**Rules**
- Usually **not needed** — a static `Create(...)` on the Entity/Aggregate is
  sufficient in the vast majority of cases.
- A standalone factory class (with interface) is only justified when:
  - creation depends on an external resource (e.g. an ID must come from a
    sequence service), or
  - multiple aggregates are created together in one coordinated step.

## 9. Unit of Work

**Purpose:** an explicit transaction boundary spanning multiple repository
operations.

**Rules**
- Lives in the **Application** layer, not Domain — Domain knows nothing
  about transactions/persistence.
- A thin interface, **transaction control only** (don't misuse it as a
  general-purpose "save" abstraction for repositories).
- `CommitAsync()` internally calls `SaveChangesAsync()` and commits the
  transaction.
- Domain services **never** get access to `IUnitOfWork` — when multiple
  aggregates need coordinating, the Application-layer use case owns the
  transaction.
- The one exception for Domain-layer DB access: read-only interfaces for
  rules that must validate against DB state (e.g. `IUniqueEmailChecker`) — no
  writes, no transaction.

```csharp
public interface IUnitOfWork
{
    Task BeginTransactionAsync(CancellationToken ct);
    Task CommitAsync(CancellationToken ct);
    Task RollbackAsync(CancellationToken ct);
}
```

## Further Reading

- Eric Evans, *Domain-Driven Design: Tackling Complexity in the Heart of Software* (2003) — the origin of every pattern in this document.
- Vaughn Vernon, *Implementing Domain-Driven Design* (2013) — practical, code-heavy companion to Evans' book.
- Martin Fowler's bliki: [*ValueObject*](https://martinfowler.com/bliki/ValueObject.html), [*Repository*](https://martinfowler.com/eaaCatalog/repository.html), [*DDD_Aggregate*](https://martinfowler.com/bliki/DDD_Aggregate.html).
- Vlad Khononov, *Learning Domain-Driven Design* (2021) — clear treatment of Core/Supporting/Generic subdomains referenced in §0.
- [Outbox pattern](https://microservices.io/patterns/data/transactional-outbox.html) on microservices.io — the delivery-guarantee mechanism referenced in §4.
- [ErrorOr](https://github.com/amantinband/error-or) — the library used throughout for `ErrorOr<T>` return values.
