# Modulith Architecture

This document explains the overall architecture style used in this backend — a
**Modulith** built from independent modules that are each internally layered
using Clean/Hexagonal Architecture — and documents the concrete module cut
chosen for this project.

See also: [Module Structure](module-structure.md) for how a single module is
organized internally (namespaces, Ports & Adapters, assembly layout options),
[DDD Patterns](ddd-patterns.md) for the tactical building blocks used inside a
module's Domain layer, and [Patterns and Rules](patterns-and-rules.md) for
further cross-cutting patterns and rules.

---

## 1. What is a Modulith?

A **Modulith** ("modular monolith") is a single deployable unit composed of
independent, loosely coupled modules, each owning its own data and use cases.
Every module is a **Bounded Context** in the DDD sense: it has its own
ubiquitous language, its own data, and communicates with other modules only
through an explicit, narrow public contract — never through shared entities or
direct database access.

Compared to a classic layered monolith (one big `Domain`/`Application`/
`Infrastructure` for the whole application), a Modulith keeps the operational
simplicity of a single deployable (one process, one deployment, no network
hops between modules) while still enforcing the boundaries that a
microservices architecture would enforce at the network level. This makes it a
good default starting point: boundaries are established early while the
option to physically split a module into its own service later stays open,
without requiring a big-bang rewrite.

Each module is, internally, structured using **Clean/Hexagonal Architecture**:

```
Api → Application → Domain
Infrastructure → Application   (Infrastructure implements ports defined by Application)
```

- **Domain**: entities, value objects, domain events, business rules — no
  dependency on anything outside the Domain layer.
- **Application**: use cases (commands/queries), and the **ports** (interfaces)
  it needs to talk to the outside world (repository interfaces, `IUnitOfWork`,
  etc.). Depends only on Domain.
- **Infrastructure**: **adapters** implementing the Application's ports (EF
  Core repositories, HTTP clients, external services). Depends on Application
  (to implement its ports) and Domain.
- **Api**: HTTP endpoints, request/response mapping. Depends only on
  Application.

See [Module Structure](module-structure.md) for the full namespace breakdown
and the Ports & Adapters mapping.

## 2. Solution Structure

Test projects live **next to** the production project they test, not in a
separate mirrored `tests/` tree — see [§5](#5-tests) for the rationale and the
naming convention:

```
src/
├── Modules/
│   ├── Identity/
│   │   ├── Identity.Contracts/            # public DTOs, events, interfaces
│   │   ├── Identity.Domain/               # entities, value objects, domain events
│   │   ├── Identity.Domain.UnitTests/
│   │   ├── Identity.Application/          # use cases, ports (interfaces)
│   │   ├── Identity.Application.UnitTests/
│   │   ├── Identity.Infrastructure/        # adapters: EF Core, external APIs
│   │   ├── Identity.IntegrationTests/
│   │   ├── Identity.Api/                   # endpoints, module registration
│   │   └── Identity.ArchitectureTests/
│   ├── Articles/
│   │   └── ... (same shape)
│   └── Tags/
│       └── ... (same shape)
├── Shared/
│   ├── Shared.Domain/           # base types shared by all modules' Domain layer (Entity<TId>, AggregateRoot<TId>, IDomainEvent, IBusinessRule, ...)
│   ├── Shared.Domain.UnitTests/
│   ├── Shared.Application/      # CQRS abstractions, IUnitOfWork, event-handling abstractions
│   ├── Shared.Application.UnitTests/
│   ├── Shared.Infrastructure/    # EF Core interceptors, domain-event dispatcher implementation
│   ├── Shared.Infrastructure.UnitTests/
│   └── Shared.Testing/           # shared test builders, fixtures, fakes — used BY other test projects, not itself a test project
├── Host/
│   ├── WebApi/                   # Composition Root, single deployable process
│   └── WebApi.IntegrationTests/   # cross-module, end-to-end scenarios
└── AppHost/
    ├── AppHost/                   # Aspire orchestration (local & deployment)
    └── ServiceDefaults/           # Telemetry, health checks, resilience defaults
```

`Shared` is intentionally named **`Shared`**, not `SharedKernel`: it is kept
distinct from the strategic DDD "Shared Kernel" pattern (shared *domain*
knowledge between teams). This project's `Shared` contains only **technical**
building blocks without any business meaning.

## 3. Contracts: How Modules Talk to Each Other

The `Contracts` project is a module's public surface — the **only** thing
another module is allowed to reference at all. It contains:

- **Integration events** — something that happened in this module, published
  for other modules to react to (`*.Contracts.Events`, past tense, e.g.
  `ArticlePublished`).
- **Query contracts** — small DTOs and read interfaces that other modules can
  depend on to read data they don't own (`*.Contracts.Queries`, naming
  `I{Module}QueryService`).
- **Command contracts** — a narrow public API for actions other modules need
  to trigger on data they don't own (`*.Contracts.Commands`, naming
  `I{Module}Service`).

The interface lives in `Contracts`; the implementation stays `internal` inside
`Application`, so the compiler — not just convention — prevents other modules
from bypassing the interface:

```csharp
// Identity.Contracts.Queries
public interface IProfileQueryService
{
    Task<ProfileDto?> GetProfileAsync(Guid personId, CancellationToken ct);
}

public record ProfileDto(Guid Id, string Username, string Bio, string Image);
```

```csharp
// Identity.Application.Queries
internal sealed class ProfileQueryService : IProfileQueryService
{
    private readonly IPersonRepository _repository; // a port from Application

    public async Task<ProfileDto?> GetProfileAsync(Guid personId, CancellationToken ct)
    {
        var person = await _repository.GetByIdAsync(new PersonId(personId), ct);
        return person is null ? null : new ProfileDto(person.Id.Value, person.Username, person.Bio, person.Image);
    }
}
```

Only DTOs (`ProfileDto`) cross the boundary — never Domain entities. Otherwise
the internal Domain model leaks into other modules, and any future Domain
refactor risks breaking unrelated modules. This much matches the naive
picture of "modules call each other's Contracts". The **consuming** side,
however, needs one more step to stay consistent with Ports & Adapters — see
next.

### Consuming another module's Contracts: through your own Infrastructure, never from Application

`Application` is only allowed to know about its **own** module's `Domain`,
its own ports, and `Shared` — never another module's `Contracts`. A dependency
on another module is, from the consuming module's point of view, exactly the
same kind of external dependency as a database or a third-party HTTP API: it
must sit behind a port that `Application` owns, with the actual call adapted
away in `Infrastructure` (see [Module Structure §2](module-structure.md#2-ports--adapters-hexagonal-architecture-per-module)).

Concretely, `Articles` consuming `Identity`'s profile data looks like this:

```csharp
// Articles.Application.Articles — a port interface Articles.Application owns and knows
// nothing about Identity; it lives directly next to the use case(s) that consume it,
// no dedicated Ports namespace or Port suffix
public interface IProfileReader
{
    Task<AuthorProfile?> GetAuthorProfileAsync(Guid personId, CancellationToken ct);
}

public record AuthorProfile(Guid Id, string Username, string Bio, string Image);
```

```csharp
// Articles.Infrastructure.Adapters — the only place in Articles allowed to reference Identity.Contracts
internal sealed class IdentityProfileReaderAdapter(IProfileQueryService identityProfileQueryService) : IProfileReader
{
    public async Task<AuthorProfile?> GetAuthorProfileAsync(Guid personId, CancellationToken ct)
    {
        var profile = await identityProfileQueryService.GetProfileAsync(personId, ct);
        return profile is null ? null : new AuthorProfile(profile.Id, profile.Username, profile.Bio, profile.Image);
    }
}
```

```csharp
// Host composition root
services.AddScoped<IProfileReader, IdentityProfileReaderAdapter>();
```

`Articles.Application` and `Articles.Domain` only ever see `IProfileReader`
and `AuthorProfile` — types `Articles` itself owns. If `Identity.Contracts`
changes shape (a field is renamed, the interface is split, or `Identity` is
later extracted into its own service reachable only over HTTP), only
`IdentityProfileReaderAdapter` has to change. Nothing in `Articles.Application`
or `Articles.Domain` notices. This is the same protection Ports & Adapters
already gives against a change to the database schema or a third-party API —
applied consistently to cross-module dependencies as well.

### Referencing rules

| From → To | Allowed? |
|---|---|
| `ModuleA.Infrastructure` → `ModuleB.Contracts` | ✅ (adapter implementing one of `ModuleA.Application`'s own ports) |
| `ModuleA.Application` → `ModuleB.Contracts` | ❌ (Application only knows its own ports plus `Shared`) |
| `ModuleA.Application`/`Domain` → `ModuleB.Application`/`Domain` | ❌ |
| any module → another module's `DbContext`/EF Core entities/tables | ❌ (even with a shared physical database, see [§7](#7-database-ownership)) |

### Asynchronous communication: integration events

When an action in one module must trigger behavior in another (e.g. "an
account was deleted, so its comments/favorites/articles must be handled"),
the owning module publishes an in-process integration event instead of the
consuming module reaching into the other module's data.

```csharp
// Articles.Contracts.Events
public record ArticlePublished(Guid ArticleId, Guid AuthorId);
```

The same rule as above applies to the handler on the consuming side: it is a
driving adapter (comparable to `Api`, just triggered by an event instead of an
HTTP request), so it lives in the consuming module's `Infrastructure` (or a
dedicated `Integration` folder next to it), and its only job is translating
the event into a call against the consuming module's own `Application`
command/use case. `Application` itself is never given the other module's
event type directly.

Events are processed **in-process** today (e.g. via a lightweight in-process
event bus or `MediatR`-style notifications), optionally combined with the
Outbox pattern described in [DDD Patterns §4](ddd-patterns.md#4-domain-event) for delivery
guarantees.

### Naming conventions

| Purpose | Namespace | Naming convention |
|---|---|---|
| Integration event | `*.Contracts.Events` | past tense: `ArticlePublished`, `AccountRegistered` |
| Sync query contract | `*.Contracts.Queries` | `I{Module}QueryService`, methods `Get...Async` |
| Sync command contract | `*.Contracts.Commands` | `I{Module}Service`, methods `{Verb}Async` |

### A note on synchronous writes across modules

A read-only query contract is unproblematic. If a module ever needs a
synchronous **write** into a module it doesn't own, treat that as a signal to
double check: either the module boundary is drawn wrong, or an integration
event (rather than a direct command) is the better fit. When in doubt, raise
it in architecture review rather than reaching for a shortcut.

## 4. Host and AppHost: Composition Root

`Host/WebApi` is the single ASP.NET Core process that hosts all modules. Its
only job is to collect and wire up modules — it must never contain
module-specific business logic itself:

```csharp
// Program.cs in the Host
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddArticlesModule(builder.Configuration);
builder.Services.AddTagsModule(builder.Configuration);

var app = builder.Build();
app.MapIdentityEndpoints();
app.MapArticlesEndpoints();
app.MapTagsEndpoints();
```

Each module brings its own registration extension (`Add{Module}Module`,
`Map{Module}Endpoints`), so `Program.cs` stays thin and modules can be
registered/tested independently of each other.

`AppHost` (using .NET Aspire) orchestrates the Host process together with
external resources for local development and deployment:

```csharp
// AppHost/AppHost.cs
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var conduitDb = postgres.AddDatabase("conduit-db");

var webApi = builder.AddProject<Projects.Conduit_Host_WebApi>("webapi")
    .WithReference(conduitDb);

builder.Build().Run();
```

## 5. Tests

Test projects are co-located next to the production project they test (see
[§2](#2-solution-structure)), not gathered in a separate mirrored `tests/` tree. Given each module
already owns its Domain/Application/Infrastructure/Api projects, its test
projects are just further siblings in the same module folder — moving or
extracting a module later takes its tests along automatically, and there's no
second, parallel directory structure to keep in sync.

| Test kind | Project scope | Purpose | Dependencies |
|---|---|---|---|
| Unit tests | per layer, suffix `.UnitTests`: `{Module}.Domain.UnitTests`, `{Module}.Application.UnitTests` | isolated logic tests; Application tests mock ports | none (no test containers) |
| Integration tests | per module, suffix `.IntegrationTests`: `{Module}.IntegrationTests` | Application + Infrastructure together (real DB/HTTP) | Testcontainers, WireMock, etc. |
| Architecture tests | per module (or one global project), suffix `.ArchitectureTests` | automated dependency/boundary rule checks | NetArchTest / ArchUnitNET |
| End-to-end tests | `Host/WebApi.IntegrationTests` | cross-module scenarios (e.g. "publish an article → tag catalog is updated") | full infrastructure |

Naming rules:
- Test namespaces mirror production code: `{Module}.{Layer}.UnitTests.{Area}`.
- Test classes: `{ClassUnderTest}Tests`.
- `Shared.Testing` holds cross-module test builders, fixtures, and fakes that
  other test projects depend on. It deliberately does **not** use the
  `.UnitTests`/`.IntegrationTests` suffix, since it contains no tests of its
  own — using either suffix would risk a test runner picking it up as (empty)
  test project, and `Shared.UnitTests` specifically would be confusable with
  actual unit tests *of* `Shared.Domain`/`Shared.Application`.

Recommended CI staging (fastest feedback first):
1. `*.Domain.UnitTests` + `*.Application.UnitTests`
2. `*.ArchitectureTests`
3. `*.IntegrationTests`

## 6. Enforcing Boundaries Automatically

Module boundaries should not just live in this document — they should be
enforced by **NetArchTest** or **ArchUnitNET** so they can't erode silently
over time. Rules worth automating:

- Domain must not reference Infrastructure.
- No type from `*.Infrastructure` may be referenced by another module's `*.Api`.
- No module's `Application`/`Domain` may reference another module's
  `*.Domain`, `*.Application`, or `*.Contracts` — only that module's own
  `Infrastructure` adapters may reference `*.Contracts` (see [§3](#3-contracts-how-modules-talk-to-each-other)).
- Explicitly allowed cross-module dependency directions (e.g. "Articles may
  depend on Identity's contracts, never the reverse") should be listed and
  checked.

## 7. Database Ownership

Recommended: one schema per module (same physical database is fine, but
logically separated). This makes a later cut to microservices cheaper, since
each module already has its own data ownership boundary — and it also makes
the "no module reads another module's tables directly" rule
([§3](#3-contracts-how-modules-talk-to-each-other)) easy to
verify: a module simply has no `DbSet`/migration access to another module's
schema.

## 8. Concrete Module Cut for This Project

Based on the RealWorld domain, this project uses three modules:

| Module | Responsibility | Owned entities | Owned use cases |
|---|---|---|---|
| **Identity** | Authentication, account credentials, public profile data, and the follow/unfollow relationship between accounts | `User`, `UserFollow` | `Users/Create`, `Users/Login`, `Users/Details`, `Users/Edit`, `Profiles/Details`, `Followers/Add`, `Followers/Delete` |
| **Articles** | Authoring and browsing articles, commenting, favoriting, and tagging of articles | `Article`, `Comment`, `ArticleFavorite`, `ArticleTag` | `Articles/Create`, `Articles/Edit`, `Articles/Delete`, `Articles/List`, `Articles/Details`, `Comments/Create`, `Comments/Delete`, `Comments/List`, `Favorites/Add`, `Favorites/Delete` |
| **Tags** | Tag catalog | `Tag` | `Tags/List` |

### Why Identity absorbs Profiles and Followers

An earlier design split `Person` along an auth-vs-public-profile line
(`Credentials` owning `Username`/`Email`/`Hash`/`Salt`, `Profile` owning
`Bio`/`Image`, in two separate modules). That split was reverted: the
RealWorld API itself models `Users/Details` and `Users/Edit` as single REST
endpoints that read/write `username`, `email`, `bio`, and `image` together —
an external contract this project doesn't control. Honoring a clean
auth/profile boundary underneath that endpoint shape required a two-part
write for `Users/Create`, coordinated cross-module reads/writes for
`Users/Details`/`Users/Edit`, and a cached, event-synced copy of `Username` on
the profile side — a meaningful amount of coordination overhead to preserve a
boundary the public API doesn't actually respect. Keeping `Person` (and
`FollowedPeople`, since follow relationships are likewise keyed off account
identity) as a single **Identity** module removes all of that, at the cost of
a slightly larger module that spans authentication, public profile, and the
social graph.

### Why Comments and Favorites live inside Articles

`Articles`, `Comments`, and `Favorites` are treated as a single module rather
than three. All three center on the `Article` aggregate, share its lifecycle
(a comment or favorite cannot outlive its article, and both are always
looked up/listed together with the article they belong to), and have no
independent meaning outside of an article. Splitting them into separate
modules would force constant, chatty cross-module contract calls for what is
really one cohesive context, without providing any real isolation benefit.
`Comment` and `ArticleFavorite` therefore remain internal implementation
details of the `Articles` module — accessible from within the module without
going through a `Contracts` interface — while still following the "own your
commands/queries" rule *within* that module (e.g. comment creation logic
doesn't leak into article-editing code).

### Why Tags stays separate

The tag catalog (the set of known tag names) is a distinct concern from any
single article and is intended to be reusable/queryable independently of
articles.

### Cross-module data needs

- **Articles** references author display data (`Username`, `Bio`, `Image`) →
  depends on a read-only **Identity** contract (`IProfileQueryService`-style),
  never on `Email`/`Hash`/`Salt`, since article/comment authoring only needs
  public profile fields.
- **Comments** and **Favorites** reference `Article` directly in-process — no
  cross-module contract is needed, since they share the `Articles` module
  boundary. They still depend on the read-only **Identity** contract for
  author information.
- `ArticleTag` is owned by **Articles**, not **Tags** — it's the join between
  an article and a tag, and tagging an article is part of the `Article`
  aggregate's own lifecycle (set on create/edit, deleted when the article is
  deleted). **Tags** owns only the `Tag` catalog itself and exposes a single
  write contract, `ITagCatalogService`:
  - `ReferenceTagsAsync(tagNames)` — called when an article starts using a
    tag. Names that aren't in the catalog yet are added, and each name's
    reference count goes up by one.
  - `ReleaseTagsAsync(tagNames)` — called when an article stops using a tag
    (on edit, or when the article is deleted). A tag that loses its last
    reference is removed from the catalog, so `Tags/List` never returns a tag
    that no article uses.

### Module boundary diagram

```text
              ┌───────────────────┐      IProfileQueryService       ┌───────────────────────────┐
              │     Identity      │◀─────────(contract)─────────────│         Articles          │
              │ (Person: Username,│                                 │ (Article, Comment,        │
              │  Email, Hash,     │                                 │  ArticleFavorite,         │
              │  Salt, Bio, Image;│                                 │  ArticleTag)              │
              │  FollowedPeople)  │                                 └───────────────────────────┘
              └───────────────────┘                                              │
                                                                                 │ ITagCatalogService (contract)
                                                                                 ▼
                                                                        ┌───────────────────┐
                                                                        │       Tags        │
                                                                        │ (Tag: TagName,    │
                                                                        │  ReferenceCount)  │
                                                                        └───────────────────┘
```

Legend: an arrow is an allowed dependency, mediated exclusively through the
target module's `Contracts`. All other module-to-module access is forbidden.
`Comment`/`ArticleFavorite`/`ArticleTag` are internal to `Articles` — no
contract boundary exists between `Article`, `Comment`, and `ArticleFavorite`.

### Trade-offs

- **Positive:** modules can evolve, be tested, and (eventually) be extracted
  into separate deployables with minimal churn, since coupling is limited to
  narrow, explicit contracts.
- **Positive:** new contributors have a documented rule set for where code
  belongs and what it may depend on.
- **Trade-off:** some duplication of small read DTOs across modules (e.g. an
  `ArticleSummary` exposed by `Articles` for `Identity`/`Tags` to consume) is
  expected and preferred over sharing entities.
- **Trade-off:** grouping `Articles`, `Comments`, and `Favorites` into one
  module gives it more responsibility than the others. Accepted because the
  alternative (three modules) would add contract overhead without real
  isolation, since none of the three can be meaningfully used or deployed
  independently of `Article`.
- **Trade-off:** keeping authentication, public profile, and the follow graph
  together in `Identity` gives it more responsibility than `Tags`. Accepted
  because the RealWorld API models `Users/Details`/`Users/Edit` as single
  endpoints spanning auth and profile fields (see above).

### Quick reference for new developers

- **Adding a new module?** Copy the five projects (`Contracts`, `Domain`,
  `Application`, `Infrastructure`, `Api`) of an existing module as a template
  — or start with a single assembly per module, see
  [Module Structure §3](module-structure.md#3-how-many-assemblies-per-module).
- **Another module needs data you own?** Add an interface + DTO to your
  `Contracts` project; keep the implementation `internal` in `Application`.
- **You need data owned by another module?** Define your own port interface
  directly in your `Application` (next to the use case that needs it) and
  implement it in your `Infrastructure` as an adapter that calls the other
  module's `Contracts` — never reference another module's `Contracts` from
  your own `Application` (see [§3](#3-contracts-how-modules-talk-to-each-other)).
- **You need to react to something another module did?** Handle the
  integration event in your `Infrastructure` (or an `Integration` folder next
  to it) and translate it into a call against your own `Application` — never
  reference another module's `Application`/`Domain` directly, and never pass
  the raw external event straight into your `Application`.
- **Unsure if a dependency is allowed?** Rule: only your own module's
  `Infrastructure` may reference another module's `Contracts`; `Application`
  and `Domain` never reference anything outside their own module plus
  `Shared`. If in doubt, ask in architecture review or add an architecture
  test.

## Further Reading

- Kamil Grzybek, [*Modular Monolith: A Primer*](https://www.kamilgrzybek.com/blog/posts/modular-monolith-primer) — practical write-up and reference implementation of the modular monolith style used here.
- Simon Brown, [*Modular Monoliths*](https://www.milanjovanovic.tech/blog/modular-monolith-architecture) (Milan Jovanović) — accessible overview with .NET examples.
- Alistair Cockburn, [*Hexagonal Architecture*](https://alistair.cockburn.us/hexagonal-architecture/) — the Ports & Adapters style each module follows internally.
- Eric Evans, *Domain-Driven Design* (2003) — origin of the Bounded Context concept that underlies the module cut.
- Vlad Khononov, *Balancing Coupling in Software Design* (2024) — deeper treatment of module boundaries and coupling trade-offs.
- Microsoft Learn, [*Design a DDD-oriented microservice*](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/ddd-oriented-microservice) — Microsoft's own take on applying DDD layering per service/module.
