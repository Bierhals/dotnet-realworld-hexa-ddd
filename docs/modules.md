# Modulith Architecture Concept: Module-Cut and Boundary Rules

- Version: 1.1
- Last updated: 2026-07-07
- Owner: Backend maintainers

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
| **Identity** | Authentication and account credentials: enforcing username/email uniqueness, verifying credentials at login, issuing sessions/JWTs | `Credentials` (`Username`, `Email`, `Hash`, `Salt`) | `Users/Create` (registration — also triggers `Profile` creation via a **Profiles** contract), `Users/Login`; hosts `Users/Details` and `Users/Edit` but coordinates with **Profiles** for those (see below) |
| **Profiles** | Public-facing profile data (bio/image), follow/unfollow relationships between users | `Profile` (`Bio`, `Image`, plus a read-only cached copy of `Username` used as the public lookup key), `FollowedPeople` | `Profiles/Details`, `Followers/Add`, `Followers/Delete` |
| **Articles** | Authoring and browsing articles, commenting, favoriting, and tagging of articles | `Article`, `Comment`, `ArticleFavorite`, `ArticleTag` | `Articles/Create`, `Articles/Edit`, `Articles/Delete`, `Articles/List`, `Articles/Details`, `Comments/Create`, `Comments/Delete`, `Comments/List`, `Favorites/Add`, `Favorites/Delete` |
| **Tags** | Tag catalog (the set of known tag names) | `Tag` | `Tags/List` |

`Person` is split into two entities along an auth-vs-public-profile line:
`Username`, `Email`, `Hash`, and `Salt` are authentication data — they are
required to log in, to enforce account uniqueness, and to identify the
subject of a JWT — so they are owned by **Identity** as a `Credentials`
entity. `Bio` and `Image` are public-facing profile data with no bearing on
authentication, so they remain owned by **Profiles** as a `Profile` entity.
Because profiles are looked up and displayed by `Username` (see
`Profiles/Details`), **Profiles** keeps a read-only cached copy of `Username`
alongside its own `Profile` row, kept in sync with **Identity** via an
in-process event (e.g. `UsernameChanged`) whenever **Identity** changes it;
**Profiles** never treats this cached copy as authoritative or writes it back.

This split has consequences for three use cases that previously fit cleanly
inside a single module:

- **`Users/Create` (registration)** becomes a two-part write: **Identity**
  creates the `Credentials` row first (after checking username/email
  uniqueness), then synchronously calls a **Profiles** command contract
  (e.g. `IProfileWriter.CreateAsync(personId, username)`) to create the
  matching `Profile` row (with empty `Bio`/`Image`) before returning the
  registration response. A synchronous call — not an event — is used here
  because the API response must reflect the fully created account
  immediately.
- **`Users/Details`** (the authenticated user's own account view) and
  **`Users/Edit`** are no longer owned by a single module: both read/write
  `Username`/`Email` (**Identity**) and `Bio`/`Image` (**Profiles**).
  **Identity** hosts the endpoint and handler (as the entry point for "the
  current authenticated user"), reads/writes its own `Credentials`, and
  composes the result with a call to **Profiles**'s `IProfileReader` (for
  `Users/Details`) or a **Profiles** command contract (for `Users/Edit`) to
  read/update `Bio`/`Image`.

These three use cases are the explicit exception to "each use case lives
entirely inside one module" — they are coordinated, contract-mediated
operations spanning **Identity** and **Profiles**, not violations of module
isolation, since neither module ever touches the other's entities directly.

`Articles`, `Comments`, and `Favorites` are treated as a **single module**
rather than three separate ones. All three center on the `Article` aggregate,
share its lifecycle (a comment or favorite cannot outlive its article, and
both are always looked up/listed together with the article they belong to),
and have no independent meaning outside of an article. Splitting them into
separate modules would force constant chatty cross-module contract calls for
what is really one cohesive context, without providing any real isolation
benefit. `Comment` and `ArticleFavorite` therefore remain internal
implementation details of the `Articles` module — accessible from within the
module without going through a `Contracts` interface — while still following
the "own your commands/queries" rule *within* that module (e.g. comment
creation logic doesn't leak into article-editing code).

`Tags` remains a separate module because the tag catalog (the set of known
tag names) is a distinct concern from any single article and is intended to
be reusable/queryable independently of articles (see `Tags/List`).

These map closely onto the existing `Features/*` folders (with `Comments` and
`Favorites` folded into `Articles`), which keeps the migration low-risk:
today's vertical slices already approximate the module boundaries, they
simply are not yet enforced by tooling/project structure.

## Module Boundaries

Each module:

- **Owns its data.** Only the owning module may read/write its entities
  directly through EF Core (`DbContext` access, migrations for that entity's
  table). For example, only **Articles** creates/removes `Comment` and
  `ArticleFavorite` rows (these are internal to the `Articles` module); only
  **Identity** creates/removes `Credentials` rows; only **Profiles**
  creates/removes `Profile` rows (except for its cached `Username` copy,
  which is only ever updated in reaction to an **Identity** event, never
  written from another module directly).
- **Owns its use cases.** Application/command-query logic for a capability
  lives in the owning module (e.g. all logic to add/remove a favorite or
  comment lives in **Articles**, since it is part of the same context, not
  scattered across separate modules).
- **Exposes a public contract**, not its internal entities. Other modules must
  not reference another module's EF Core entity types, `DbContext`, or
  internal handlers. Instead each module exposes:
  - **Query/read contracts** — small DTOs and read interfaces (e.g. the
    existing `IProfileReader` / `Profile` pattern in
    `Features/Profiles/IProfileReader.cs`) that other modules can depend on to
    read data they don't own.
  - **Command contracts** — a narrow public API (interface + DTO) for actions
    other modules need to trigger (e.g. "does this article exist and who is
    its author" for **Profiles** to use, without touching `Article` directly).

### Cross-module data needs today

Mapping current implicit couplings to the target module-cut:

- **Articles** (including its internal `Comment`/`ArticleFavorite` concerns)
  references author display data (`Username`, `Bio`, `Image`) → **Articles**
  depends on a read-only **Profiles** contract (`IProfileReader`-style,
  the same one already used by other consumers of profile data), not
  **Identity**'s `Credentials`, since article/comment authoring only needs
  the public profile fields, never `Email`/`Hash`/`Salt`.
- **Comments** and **Favorites** are internal to the `Articles` module and
  reference `Article` directly in-process; no cross-module contract is
  needed between them and `Article` since they share the same module
  boundary. They still depend on the read-only **Profiles** contract for
  author information.
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
- **Identity**/`Users/Create` (registration) needs a matching `Profile` row
  created → **Identity** depends on a **Profiles** write contract (e.g.
  `IProfileWriter.CreateAsync(personId, username)`), since `Profile` is owned
  by **Profiles**. This is a synchronous contract call, not an event,
  because the registration response must include the fully created account.
- **Identity**/`Users/Login` only needs its own `Credentials`
  (`Email`/`Hash`/`Salt`) → no cross-module dependency.
- **Identity**/`Users/Details` and `Users/Edit` need `Bio`/`Image` in
  addition to their own `Username`/`Email` → **Identity** depends on
  **Profiles**'s `IProfileReader` (for `Users/Details`) and a **Profiles**
  write contract (for `Users/Edit`), composing the response/update from both
  modules (see "Candidate Modules" above).
- **Profiles**'s cached `Username` copy needs to stay current whenever
  **Identity** changes it (there is no `Users`-style rename feature today,
  but the contract should be resilient to future username changes) →
  **Profiles** subscribes to an **Identity**-published in-process event
  (e.g. `UsernameChanged`) rather than querying **Identity**'s `Credentials`
  on every read.
- **Profiles** owns `Profile` and `FollowedPeople` directly (same module), so
  no cross-module contract is needed between them.

## Cross-Module Communication

1. **Prefer read contracts for queries.** A module that needs data owned by
   another module depends on a small interface (e.g. `IArticleReader`,
   `IProfileReader`) implemented by the owning module and registered in DI.
   This mirrors the existing `IProfileReader` pattern already used by
   **Profiles**.
2. **Prefer in-process domain/integration events for side effects.** When an
   action in one module must trigger behavior in another (e.g. "a person was
   deleted, so their comments/favorites/articles must be handled"), the owning
   module publishes an in-process event (e.g. via `MediatR`-style
   `INotification` or a lightweight in-process event bus) instead of the
   consuming module reaching into the other module's tables. Handlers for
   these events live in the *consuming* module. Side effects that stay within
   the same module (e.g. deleting an article's comments/favorites when the
   article itself is deleted) do not need events — they are handled directly
   in-process since `Article`, `Comment`, and `ArticleFavorite` share the same
   module boundary.
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
      Comment.cs
      ArticleFavorite.cs
      ArticleTag.cs
    Create.cs, Edit.cs, ...# use cases (internal)
    Comments/               # Comments sub-folder — internal to this module
      Create.cs, Delete.cs, List.cs
    Favorites/              # Favorites sub-folder — internal to this module
      Add.cs, Delete.cs
    ArticlesEndpoints.cs    # HTTP endpoints for this module
    CommentsEndpoints.cs
    FavoritesEndpoints.cs
  Tags/
    Contracts/
    Domain/
    ...
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
                     ┌───────────────────────────────┐
                     │            Profiles            │
                     │ (Profile: Username(cached),    │
                     │  Bio, Image; FollowedPeople)    │
                     └───▲──────────────────▲──────────┘
      IProfileWriter/    │                  │ IProfileReader (contract)
      IProfileReader     │                  │
      UsernameChanged    │                  │
      (event, Identity ──┘                  │
       → Profiles)                          │
              ┌──────────┴──────┐   ┌────────┴──────────────────┐
              │     Identity     │   │        Articles           │
              │ (Credentials:    │   │ (Article, Comment,        │
              │  Username, Email,│   │  ArticleFavorite,         │
              │  Hash, Salt)     │   │  ArticleTag)               │
              └──────────────────┘   └────────────▲────────────────┘
                                                    │ ITagReader (contract)
                                           ┌────────┴────────┐
                                           │      Tags        │
                                           │      (Tag)        │
                                           └───────────────────┘

Legend:
  ──►  allowed dependency, via the target module's public Contracts only
  All other module-to-module access is forbidden.
  Comment/ArticleFavorite/ArticleTag are internal to the Articles module —
  no contract boundary exists between Article, Comment, and ArticleFavorite.
  Identity → Profiles also includes a one-way `UsernameChanged` event so
  Profiles can keep its cached `Username` copy current.
```

Only **Contracts** (and the narrow `UsernameChanged` event) are consumed
across module boundaries; the arrows above represent dependencies on read
interfaces/DTOs (e.g. `IProfileReader`, `ITagReader`) or command
contracts/events, never on another module's domain entities or persistence
layer.

## Trade-offs and Follow-up

- **Positive:** Modules can evolve, be tested, and (eventually) be extracted
  into separate deployables with minimal churn, since coupling is limited to
  narrow, explicit contracts.
- **Positive:** New contributors have a clear, documented rule set for where
  code belongs and what it may depend on.
- **Trade-off:** Some duplication of small read DTOs across modules (e.g. an
  `ArticleSummary` exposed by **Articles** for **Profiles**/**Tags** to
  consume) is expected and preferred over sharing entities.
- **Trade-off:** Grouping `Articles`, `Comments`, and `Favorites` into one
  module means that module carries more responsibility than the others. This
  is accepted because the alternative (three separate modules) would add
  contract overhead without real isolation, since none of the three can be
  meaningfully used or deployed independently of `Article`.
- **Trade-off:** Splitting `Person` into **Identity**'s `Credentials` and
  **Profiles**'s `Profile` makes `Users/Create`, `Users/Details`, and
  `Users/Edit` coordinated, multi-module operations instead of a single
  local write/read, and requires **Profiles** to maintain a cached copy of
  `Username` that is updated in-process (synchronously, via the
  `UsernameChanged` event handler) whenever **Identity** changes it, rather
  than being authoritative itself. This is accepted because it keeps
  authentication secrets (`Hash`/`Salt`) and public profile data cleanly
  separated, which matters more as the app grows (e.g. only **Identity**
  ever needs to touch password hashes).
- **Follow-up work (not part of this document):** Physically moving
  `Features/*`/`Domain/*` into the `Modules/*` layout described above, and
  introducing the read/write-contract interfaces/in-process events for the
  cross-module data needs listed above. This document intentionally
  describes the target design first so future feature work can migrate
  incrementally.
