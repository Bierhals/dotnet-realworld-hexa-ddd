# ADR: Module-Cut for a Modulith Architecture

- Status: Accepted
- Date: 2026-07-07
- Related issue: [#45](https://github.com/Bierhals/dotnet-realworld-hexa-ddd/issues/45)

## Context

The backend currently lives in a single `Conduit` project organized by vertical
feature slices under `Features/*` (`Articles`, `Comments`, `Favorites`,
`Followers`, `Profiles`, `Tags`, `Users`), with shared persistence entities
under `Domain/*` (`Article`, `Comment`, `Tag`, `ArticleTag`, `ArticleFavorite`,
`Person`, `FollowedPeople`).

As the application grows, we want to evolve this into a **Modulith**: a single
deployable unit composed of independent, loosely coupled modules, each owning
its own data and use cases. This document defines the module-cut, the rules
for module isolation, allowed dependencies, and how modules are allowed to
communicate with each other. It does not require a big-bang rewrite; it is the
target design that future feature-slice work should converge towards.

## Candidate Modules

Based on the RealWorld domain, the following modules are identified:

| Module | Responsibility | Owned entities | Owned use cases (current `Features/*`) |
|---|---|---|---|
| **Identity** | Registration, authentication, user profile data (the user's own account) | `Person` (account-facing fields: `Username`, `Email`, `Bio`, `Image`, `Hash`, `Salt`) | `Users/Create`, `Users/Login`, `Users/Details`, `Users/Edit` |
| **Profiles** | Public-facing profile view, follow/unfollow relationships between users | `FollowedPeople` (reads `Person` via a published contract, does not own it) | `Profiles/Details`, `Followers/Add`, `Followers/Delete` |
| **Articles** | Authoring and browsing articles, and tagging of articles | `Article`, `ArticleTag` | `Articles/Create`, `Articles/Edit`, `Articles/Delete`, `Articles/List`, `Articles/Details` |
| **Comments** | Commenting on articles | `Comment` | `Comments/Create`, `Comments/Delete`, `Comments/List` |
| **Favorites** | Favoriting/unfavoriting articles | `ArticleFavorite` | `Favorites/Add`, `Favorites/Delete` |
| **Tags** | Tag catalog (the set of known tag names) | `Tag` | `Tags/List` |

These map almost 1:1 onto the existing `Features/*` folders, which keeps the
migration low-risk: today's vertical slices already approximate the module
boundaries, they simply are not yet enforced by tooling/project structure.

## Module Boundaries

Each module:

- **Owns its data.** Only the owning module may read/write its entities
  directly through EF Core (`DbContext` access, migrations for that entity's
  table). For example, only **Favorites** creates/removes `ArticleFavorite`
  rows; only **Comments** creates/removes `Comment` rows.
- **Owns its use cases.** Application/command-query logic for a capability
  lives in the owning module (e.g. all logic to add/remove a favorite lives in
  **Favorites**, not duplicated in **Articles**).
- **Exposes a public contract**, not its internal entities. Other modules must
  not reference another module's EF Core entity types, `DbContext`, or
  internal handlers. Instead each module exposes:
  - **Query/read contracts** — small DTOs and read interfaces (e.g. the
    existing `IProfileReader` / `Profile` pattern in
    `Features/Profiles/IProfileReader.cs`) that other modules can depend on to
    read data they don't own.
  - **Command contracts** — a narrow public API (interface + DTO) for actions
    other modules need to trigger (e.g. "does this article exist and who is
    its author" for **Comments**/**Favorites** to use, without touching
    `Article` directly).

### Cross-module data needs today

Mapping current implicit couplings to the target module-cut:

- **Articles** references `Person` (`Author`) → **Articles** depends on a
  read-only **Identity** contract (`IPersonReader`-style), not the `Person`
  entity.
- **Comments** references `Article` and `Person` → **Comments** depends on
  read-only **Articles** and **Identity** contracts.
- **Favorites** references `Article` and `Person` → **Favorites** depends on
  read-only **Articles** and **Identity** contracts.
- **Tags**/`ArticleTag` references `Article` → **`ArticleTag` is owned by the
  Articles module**, not by Tags. `ArticleTag` is the join between an article
  and a tag, and tagging an article is part of the Articles aggregate's own
  lifecycle (set on create/edit, deleted when the article is deleted). The
  **Tags** module owns only the `Tag` catalog itself (the set of known tag
  names) and exposes a read-only contract (e.g. `ITagReader`) that
  **Articles** depends on when validating/attaching tags to an article.
  **Articles** exposes the article's current tag list to other
  modules/consumers via its own `IArticleReader` contract (as it already does
  today through `Article.TagList`); no other module queries `ArticleTag`
  directly.
- **Profiles**/`FollowedPeople` references `Person` → **Profiles** depends on
  a read-only **Identity** contract.

## Cross-Module Communication

1. **Prefer read contracts for queries.** A module that needs data owned by
   another module depends on a small interface (e.g. `IArticleReader`,
   `IPersonReader`) implemented by the owning module and registered in DI.
   This mirrors the existing `IProfileReader` pattern already used by
   **Profiles**.
2. **Prefer in-process domain/integration events for side effects.** When an
   action in one module must trigger behavior in another (e.g. "an article
   was deleted, so its comments and favorites must be removed"), the owning
   module publishes an in-process event (e.g. via `MediatR`-style
   `INotification` or a lightweight in-process event bus) instead of the
   consuming module reaching into the other module's tables. Handlers for
   these events live in the *consuming* module.
3. **No direct cross-module entity/DbContext access.** A module must never
   `Include()`/query another module's EF Core entity types directly, and must
   never share a `DbSet` across module boundaries in application code (shared
   `DbContext`/database is acceptable at the infrastructure level during the
   Modulith stage, but access is mediated through the owning module's
   contracts).
4. **No direct references to another module's internal types.** Only types
   under a module's `Contracts` (or equivalent public) namespace may be
   referenced by other modules. Everything else (handlers, entities,
   repositories) is internal to the module.

## Folder / Project Structure Conventions

To make boundaries visible and enforceable, each module follows this shape
under `backend/Conduit/Modules/<ModuleName>/`:

```text
Modules/
  Articles/
    Contracts/            # public DTOs + interfaces other modules may depend on
      IArticleReader.cs
      ArticleSummary.cs
    Domain/                # entities owned by this module (internal)
      Article.cs
    Create.cs, Edit.cs, ...# use cases (internal)
    ArticlesEndpoints.cs    # HTTP endpoints for this module
  Comments/
    Contracts/
    Domain/
    ...
  Favorites/
  Tags/
  Identity/
  Profiles/
```

Namespace convention: `Conduit.Modules.<ModuleName>` for internals and
`Conduit.Modules.<ModuleName>.Contracts` for the public surface, mirroring the
folder layout.

Allowed dependency rules:

- A module may reference another module's `Contracts` namespace only.
- A module must not reference another module's `Domain`, handlers, or
  persistence types.
- Shared, generic infrastructure (error handling, request pipeline helpers,
  security) stays under `Shared/`/`Infrastructure/` at the application root
  and may be referenced by any module, but must not contain module-specific
  logic.
- The composition root (`Program.cs`) is the only place allowed to wire
  modules together (DI registrations of one module's implementation against
  another module's contract interface).

This convention can be enforced incrementally, starting with code review and
optionally later with architecture tests (e.g. `NetArchTest`) asserting that
no type outside a module's `Contracts` namespace is referenced from another
module.

## Module Boundary Diagram

```text
                     ┌─────────────────────────┐
                     │        Identity          │
                     │  (Person: account data)   │
                     └───────────▲───────────────┘
                                 │ IPersonReader (contract)
        ┌────────────────────────┼─────────────────────────┐
        │                        │                         │
┌───────┴────────┐      ┌────────┴────────┐        ┌───────┴────────┐
│    Profiles     │      │    Articles     │        │    Comments     │
│ (FollowedPeople) │      │(Article,        │◄───────┤   (Comment)     │
└─────────────────┘      │ ArticleTag)     │  IArticleReader
                          └───▲────────▲───┘
                              │        │ ITagReader (contract)
              IArticleReader  │        │
                     ┌────────┘        └────────┐
                     │                          │
             ┌───────┴────────┐        ┌────────┴────────┐
             │   Favorites     │        │      Tags        │
             │ (ArticleFavorite)│       │      (Tag)        │
             └─────────────────┘        └──────────────────┘

Legend:
  ──►  allowed dependency, via the target module's public Contracts only
  All other module-to-module access is forbidden.
```

Only **Contracts** are consumed across module boundaries; the arrows above
represent dependencies on read interfaces/DTOs (e.g. `IPersonReader`,
`IArticleReader`), never on another module's domain entities or persistence
layer.

## Consequences

- **Positive:** Modules can evolve, be tested, and (eventually) be extracted
  into separate deployables with minimal churn, since coupling is limited to
  narrow, explicit contracts.
- **Positive:** New contributors have a clear, documented rule set for where
  code belongs and what it may depend on.
- **Trade-off:** Some duplication of small read DTOs across modules (e.g. an
  `ArticleSummary` used by both **Comments** and **Favorites**) is expected
  and preferred over sharing entities.
- **Follow-up work (not part of this ADR):** Physically moving
  `Features/*`/`Domain/*` into the `Modules/*` layout described above, and
  introducing the read-contract interfaces/in-process events for the
  cross-module data needs listed above. This ADR intentionally documents the
  target design first so future feature work can migrate incrementally.
