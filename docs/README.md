# Developer Documentation: Architecture & Patterns

This is the entry point for the architecture and pattern documentation of
this backend. Start with [Modulith Architecture](modulith-architecture.md) if
you're new to the project.

| Document | What it covers |
|---|---|
| [Modulith Architecture](modulith-architecture.md) | What a Modulith is, the overall solution structure (`Modules/`, `Shared/`, `Host/`, `AppHost/`), how modules communicate via `Contracts`, the Host as composition root, testing strategy, automated boundary enforcement, and the concrete module cut used in this project (`Identity`, `Articles`, `Tags`). |
| [Module Structure](module-structure.md) | How a single module is organized internally: the namespace convention per layer, how that maps to Ports & Adapters (Hexagonal Architecture), and the trade-off between one assembly per layer vs. one assembly per module. |
| [DDD Patterns](ddd-patterns.md) | The DDD tactical building blocks used in the Domain layer (Value Object, Entity, Aggregate Root, Domain Event, Business Rule, Domain Service, Repository, Factory, Unit of Work), and the rule that tactical DDD should be reserved for Core subdomains. |
| [Patterns and Rules](patterns-and-rules.md) | CQRS/Mediator, Response Mapping, and other cross-cutting patterns (Decorator, Interceptor, Strategy) that aren't DDD-specific, plus a set of project-wide naming and design rules. |

Each document links to the others where relevant — follow the cross-links for
the full picture rather than reading any single document in isolation.
