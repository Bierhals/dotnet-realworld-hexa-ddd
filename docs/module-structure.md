# Module Structure

This document explains how a single module (see
[Modulith Architecture](modulith-architecture.md) for the overall module cut)
is organized internally: the namespaces used per layer, how that maps to the
Ports & Adapters pattern, and the two supported options for how many
assemblies (projects) a module is split into.

## 1. Namespace per Layer

Each module follows a `{Module}.{Layer}[.SubNamespace]` schema:

| Layer | Namespace pattern | Purpose | Example |
|---|---|---|---|
| Domain | `{Module}.Domain[.Entities/ValueObjects/Events]` | Entities, value objects, aggregate roots, domain events, business rules — no outward dependencies | `Articles.Domain.Entities` |
| Application | `{Module}.Application[.Commands/Queries]` | Use cases (commands/queries) and the ports they depend on, placed directly next to the use case that consumes them | `Articles.Application.Articles` |
| Infrastructure | `{Module}.Infrastructure[.Persistence/Adapters]` | Adapters implementing Application's ports (EF Core, HTTP clients, ...) | `Articles.Infrastructure.Adapters` |
| Contracts | `{Module}.Contracts[.Events/Queries/Commands]` | The module's public surface for other modules (see [Modulith Architecture §3](modulith-architecture.md#3-contracts-how-modules-talk-to-each-other)) | `Articles.Contracts.Events` |
| Api | `{Module}.Api[.Endpoints]` | HTTP endpoints, request/response mapping | `Articles.Api.Endpoints` |
| Host | `Host.WebApi[.Extensions/Configuration]` | Composition root wiring modules together | `Host.WebApi.Extensions` |

Rules:

1. **Project/root namespace and assembly name match the project name**
   (`RootNamespace` = `AssemblyName` = project name).
2. **The layer name is always part of the namespace**, not just the folder —
   this makes layer violations visible immediately in `using` statements,
   even before an architecture test catches them.
3. **Adapters are named explicitly** (`Infrastructure.Adapters`); the ports
   they implement are plain interfaces with no dedicated namespace or `Port`
   suffix — they live directly in `Application`, next to the use case that
   consumes them — see [§2](#2-ports--adapters-hexagonal-architecture-per-module).
4. **`Contracts` is kept thin and in its own namespace branch**, so
   IDE autocomplete from another module only ever surfaces what that module
   is allowed to see.
5. Optional company prefix is fine (`Acme.Articles.Domain`), as long as the
   `{Module}.{Layer}` part of the schema is preserved.

## 2. Ports & Adapters (Hexagonal Architecture) per Module

Internally, every module follows the [Ports & Adapters / Hexagonal
Architecture](https://alistair.cockburn.us/hexagonal-architecture/) style:

- **Domain** has no ports at all — it is persistence- and
  transaction-free by design (see [DDD Patterns](ddd-patterns.md)).
- **Application** defines the **ports**: interfaces describing what the use
  cases need from the outside world — `IUnitOfWork` and any other
  outward-facing interface a use case depends on. There is no dedicated
  `Ports` namespace or `Port` suffix: each interface lives directly next to
  the use case(s) that consume it. Repository interfaces are the one
  exception — they live in `Domain`, per the Repository pattern's Dependency
  Inversion rule (see [DDD Patterns §7](ddd-patterns.md#7-repository)).
- **Infrastructure** provides the **adapters**: concrete implementations of
  those ports — an EF Core repository implementing `IArticleRepository`, an
  `IUnitOfWork` implementation wrapping a `DbContext`, an HTTP client
  implementing an outward-facing interface, etc. **Another module's
  `Contracts` is treated as just another external dependency**: the port
  (e.g. `IProfileReader`) is defined and owned by this module's
  `Application`, and only this module's `Infrastructure` is allowed to
  depend on the other module's `Contracts` to implement that port — see
  [Modulith Architecture §3](modulith-architecture.md#3-contracts-how-modules-talk-to-each-other) for the full example.
- **Api** is a *driving* adapter: it adapts an inbound HTTP request into a
  call against the Application layer (a command/query). It is not "inside"
  the hexagon; it is one of possibly several driving adapters (another could
  be a message-queue consumer, a gRPC endpoint, a scheduled job, ...). A
  handler for another module's integration event is also a driving adapter
  in this sense, and lives in `Infrastructure` for the same reason.

```
                  ┌────────────────────────┐
   HTTP request → │   Api (driving adapter) │
                  └───────────┬─────────────┘
                              ▼
                  ┌────────────────────────┐
                  │       Application        │  ← defines ports (interfaces)
                  │   (use cases / ports)    │
                  └───────────┬─────────────┘
                              ▼
                  ┌────────────────────────┐
                  │          Domain           │  (no ports, no outward deps)
                  └────────────────────────┘
                              ▲
                  ┌───────────┴─────────────┐
                  │     Infrastructure        │  ← implements ports (driven adapters)
                  │   (EF Core, HTTP, ...)    │
                  └────────────────────────┘
```

The dependency arrow always points *inward* toward Application/Domain;
Infrastructure and Api both depend on Application, never the other way
around. This is what makes Application's ports swappable: an EF Core adapter
can be replaced with a different persistence technology, or an HTTP-based
adapter with a gRPC one, without Application or Domain changing at all.

## 3. How Many Assemblies per Module?

Two structural options are both valid; pick the one appropriate for the
module's maturity and expected lifetime.

### Option A — One assembly per layer (5 projects)

```
Articles.Contracts.csproj
Articles.Domain.csproj
Articles.Application.csproj
Articles.Infrastructure.csproj
Articles.Api.csproj
```

Project references enforce the dependency direction **at compile time**:
`Articles.Domain` simply cannot reference `Articles.Infrastructure`, because
no project reference exists — the build fails, not just a code review
comment. This is the strictest option and the best fit if:

- the module is expected to be extracted into its own deployable/service
  later, or
- the module is large/complex enough that layer violations are a recurring
  real risk.

Trade-off: five projects per module means more project-setup overhead
(`.csproj` files, solution folders, `Directory.Build.props` wiring) that
scales with the number of modules.

### Option B — One assembly per module, layers as folders/namespaces

```
Articles.csproj
├── Domain/
├── Application/
├── Infrastructure/
├── Contracts/
└── Api/
```

A single project per module, with the same namespace convention from
[§1](#1-namespace-per-layer) kept purely as folder/namespace structure.
Boundaries between layers are enforced by code review and by **architecture
tests** (NetArchTest/ArchUnitNET analyzing namespaces via reflection) instead
of project references — see
[Modulith Architecture §6](modulith-architecture.md#6-enforcing-boundaries-automatically).

This is faster to set up and reduces project-count overhead, which is a good
default for a new or still-evolving module.

**Regardless of which option is chosen, `Contracts` must stay a distinct,
clearly separated namespace** (and everything else marked `internal`), since
it is the one boundary that must never erode — even inside a single assembly,
`internal` visibility still lets the compiler stop another module from
reaching past the interface into the implementation, as long as
`InternalsVisibleTo` is not used to punch a hole in it.

### Recommendation

Start with **Option B** (single assembly per module) for new or actively
evolving modules. Move to **Option A** only when one of the following
becomes true:

- Architecture-test violations for that module keep recurring despite code
  review (a compile-time boundary is cheaper than repeated review friction).
- The module is being actively prepared for extraction into a separate
  deployable/service.

Since both options use the same namespace and Ports & Adapters conventions,
switching from B to A later is a mechanical refactor (moving folders into new
projects), not a redesign.

## Further Reading

- Alistair Cockburn, [*Hexagonal Architecture*](https://alistair.cockburn.us/hexagonal-architecture/) — the original Ports & Adapters write-up.
- Herberto Graça, [*Ports & Adapters Architecture*](https://herbertograca.com/2017/09/14/ports-adapters-architecture/) — detailed walkthrough with concrete examples.
- Jeffrey Palermo, [*The Onion Architecture*](https://jeffreypalermo.com/2008/07/the-onion-architecture-part-1/) — a closely related layering style, useful for comparison.
- Kamil Grzybek, [*Modular Monolith: A Primer*](https://www.kamilgrzybek.com/blog/posts/modular-monolith-primer) — also discusses the one-assembly-per-module vs. one-assembly-per-layer trade-off.
