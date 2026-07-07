# Modulith Architecture Concept: Module-Cut and Boundary Rules

- Version: 1.0
- Last updated: 2026-07-07
- Owner: Sven-Uwe Bierhals

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
| **Identity** | Authentication, account credentials, and public profile data, including the follow/unfollow relationship between users | `Person` (`Username`, `Email`, `Hash`, `Salt`, `Bio`, `Image`), `FollowedPeople` | `Users/Create`, `Users/Login`, `Users/Details`, `Users/Edit`, `Profiles/Details`, `Followers/Add`, `Followers/Delete` |
| **Articles** | Authoring and browsing articles, commenting, favoriting, and tagging of articles | `Article`, `Comment`, `ArticleFavorite`, `ArticleTag` | `Articles/Create`, `Articles/Edit`, `Articles/Delete`, `Articles/List`, `Articles/Details`, `Comments/Create`, `Comments/Delete`, `Comments/List`, `Favorites/Add`, `Favorites/Delete` |
| **Tags** | Tag catalog (the set of known tag names) | `Tag` | `Tags/List` |

`Identity` and `Profiles` are treated as a **single module** rather than two
separate ones. An earlier draft of this document split `Person` along an
auth-vs-public-profile line (`Credentials` in `Identity` owning
`Username`/`Email`/`Hash`/`Salt`, `Profile` in `Profiles` owning
`Bio`/`Image`). That split was reverted: the RealWorld API itself models
`Users/Details` and `Users/Edit` as single REST endpoints that
read/write `username`, `email`, `bio`, and `image` together — an external
contract this project doesn't control. Honoring a clean auth/profile
boundary underneath that endpoint shape required a two-part write for
`Users/Create`, coordinated cross-module reads/writes for `Users/Details`
and `Users/Edit`, and a `Profiles`-side cached copy of `Username` kept in
sync via an in-process `UsernameChanged` event — a meaningful amount of
coordination overhead to preserve a boundary the public API doesn't actually
respect. Keeping `Person` (and `FollowedPeople`, since follow relationships
are likewise keyed off account identity) as a single **Identity** module
removes all of that: `Users/Create`, `Users/Details`, and `Users/Edit` go
back to being simple, single-module operations, at the cost of a slightly
larger module that spans authentication, public profile, and the social
graph.

`Articles`, `Comments`, and `Favorites` are likewise treated as a **single
module** rather than three separate ones. All three center on the `Article`
aggregate, share its lifecycle (a comment or favorite cannot outlive its
article, and both are always looked up/listed together with the article
they belong to), and have no independent meaning outside of an article.
Splitting them into separate modules would force constant chatty
cross-module contract calls for what is really one cohesive context, without
providing any real isolation benefit. `Comment` and `ArticleFavorite`
therefore remain internal implementation details of the `Articles` module —
accessible from within the module without going through a `Contracts`
interface — while still following the "own your commands/queries" rule
*within* that module (e.g. comment creation logic doesn't leak into
article-editing code).

`Tags` remains a separate module because the tag catalog (the set of known
tag names) is a distinct concern from any single article and is intended to
be reusable/queryable independently of articles (see `Tags/List`).

These map closely onto the existing `Features/*` folders (with `Profiles`
and `Followers` folded into `Identity`, and `Comments`/`Favorites` folded
into `Articles`), which keeps the migration low-risk: today's vertical
slices already approximate the module boundaries, they simply are not yet
enforced by tooling/project structure.

## Module Boundaries

Each module:

- **Owns its data.** Only the owning module may read/write its entities
  directly through EF Core (`DbContext` access, migrations for that entity's
  table). For example, only **Articles** creates/removes `Comment` and
  `ArticleFavorite` rows (these are internal to the `Articles` module); only
  **Identity** creates/removes `Person` and `FollowedPeople` rows.
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
    other modules need to trigger (e.g. `ITagWriter.GetOrCreateAsync` for
    **Articles** to use when it needs to attach a tag name that may not yet
    exist in the **Tags** catalog).

### Cross-module data needs today

Mapping current implicit couplings to the target module-cut:

- **Articles** (including its internal `Comment`/`ArticleFavorite` concerns)
  references author display data (`Username`, `Bio`, `Image`) → **Articles**
  depends on a read-only **Identity** contract (`IProfileReader`-style, the
  same one already used by other consumers of profile data), never
  **Identity**'s `Email`/`Hash`/`Salt`, since article/comment authoring only
  needs the public profile fields.
- **Comments** and **Favorites** are internal to the `Articles` module and
  reference `Article` directly in-process; no cross-module contract is
  needed between them and `Article` since they share the same module
  boundary. They still depend on the read-only **Identity** contract for
  author information.
- **Tags**/`ArticleTag` references `Article` → **`ArticleTag` is owned by the
  Articles module**, not by Tags. `ArticleTag` is the join between an article
  and a tag, and tagging an article is part of the Articles aggregate's own
  lifecycle (set on create/edit, deleted when the article is deleted). The
  **Tags** module owns only the `Tag` catalog itself (the set of known tag
  names) and exposes both:
  - a read-only contract (e.g. `ITagReader`) that **Articles** depends on
    when listing/validating existing tags, and
  - a write contract (`ITagWriter.GetOrCreateAsync(tagName)`) that
    **Articles** depends on when creating/editing an article, since the
    RealWorld API allows an article to introduce brand-new tag names that
    don't yet exist in the catalog. **Articles** never inserts `Tag` rows
    itself — it always goes through `ITagWriter` so **Tags** remains the
    sole owner of the catalog, mirroring how **Identity** exposes a read
    contract for other modules that need data it owns.
  **Articles** exposes the article's current tag list to other
  modules/consumers via its own `IArticleReader` contract (as it already does
  today through `Article.TagList`); no other module queries `ArticleTag`
  directly.
- **Identity** owns `Person` and `FollowedPeople` directly (same module), so
  `Users/Create`, `Users/Details`, `Users/Edit`, `Profiles/Details`, and
  `Followers/*` are all single-module operations with no cross-module
  contract calls needed.

## Cross-Module Communication

1. **Prefer read contracts for queries.** A module that needs data owned by
   another module depends on a small interface (e.g. `IArticleReader`,
   `IProfileReader`) implemented by the owning module and registered in DI.
   This mirrors the existing `IProfileReader` pattern already used by
   **Identity**.
2. **Prefer write contracts for cross-module commands.** When a module needs
   to trigger a create/update on data owned by another module (e.g.
   **Articles** needing a new tag name added to the **Tags** catalog), it
   depends on a narrow write interface (e.g. `ITagWriter`) exposed by the
   owning module, instead of writing to that module's entities directly.
3. **Prefer in-process domain/integration events for side effects.** When an
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
4. **No direct cross-module entity/DbContext access.** A module must never
   `Include()`/query another module's EF Core entity types directly, and must
   never share a `DbSet` across module boundaries in application code (shared
   `DbContext`/database is acceptable at the infrastructure level during the
   Modulith stage, but access is mediated through the owning module's
   contracts).
5. **No direct references to another module's internal types.** Only types
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
      ITagReader.cs
      ITagWriter.cs
    Domain/
    ...
  Identity/
    Contracts/
      IProfileReader.cs
    Domain/
      Person.cs
      FollowedPeople.cs
    ...
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
              ┌──────────────────┐         IProfileReader          ┌───────────────────────────┐
              │     Identity      │◄────────(contract)─────────────│         Articles           │
              │ (Person: Username,│                                 │ (Article, Comment,        │
              │  Email, Hash,     │                                 │  ArticleFavorite,         │
              │  Salt, Bio, Image;│                                 │  ArticleTag)               │
              │  FollowedPeople)  │                                 └────────────▲────────────────┘
              └───────────────────┘                                              │ ITagReader (contract)
                                                                                  │ ITagWriter (contract)
                                                                         ┌────────┴────────┐
                                                                         │      Tags        │
                                                                         │      (Tag)        │
                                                                         └───────────────────┘

Legend:
  ──►  allowed dependency, via the target module's public Contracts only
  All other module-to-module access is forbidden.
  Comment/ArticleFavorite/ArticleTag are internal to the Articles module —
  no contract boundary exists between Article, Comment, and ArticleFavorite.
  Articles → Tags uses both a read contract (ITagReader, for listing/
  validating existing tags) and a write contract (ITagWriter, for
  getting-or-creating a tag name that doesn't yet exist in the catalog).
```

Only **Contracts** are consumed across module boundaries; the arrows above
represent dependencies on read interfaces/DTOs (e.g. `IProfileReader`,
`ITagReader`) or command contracts (e.g. `ITagWriter`), never on another
module's domain entities or persistence layer.

## Trade-offs and Follow-up

- **Positive:** Modules can evolve, be tested, and (eventually) be extracted
  into separate deployables with minimal churn, since coupling is limited to
  narrow, explicit contracts.
- **Positive:** New contributors have a clear, documented rule set for where
  code belongs and what it may depend on.
- **Trade-off:** Some duplication of small read DTOs across modules (e.g. an
  `ArticleSummary` exposed by **Articles** for **Identity**/**Tags** to
  consume) is expected and preferred over sharing entities.
- **Trade-off:** Grouping `Articles`, `Comments`, and `Favorites` into one
  module means that module carries more responsibility than the others. This
  is accepted because the alternative (three separate modules) would add
  contract overhead without real isolation, since none of the three can be
  meaningfully used or deployed independently of `Article`.
- **Trade-off:** Keeping authentication, public profile, and the follow graph
  together in a single **Identity** module means that module also carries
  more responsibility than **Tags**. This is accepted because the RealWorld
  API models `Users/Details`/`Users/Edit` as single endpoints spanning
  auth and profile fields; splitting `Person` into separate
  `Identity`/`Profiles` entities was tried and reverted (see "Candidate
  Modules") because it turned those into coordinated, multi-module
  operations and required a cached, event-synced copy of `Username` for no
  isolation benefit the public API would ever let us realize.
- **Follow-up work (not part of this document):** Physically moving
  `Features/*`/`Domain/*` into the `Modules/*` layout described above, and
  introducing the read/write-contract interfaces for the cross-module data
  needs listed above (`IProfileReader`, `ITagReader`, `ITagWriter`). This
  document intentionally describes the target design first so future
  feature work can migrate incrementally.
