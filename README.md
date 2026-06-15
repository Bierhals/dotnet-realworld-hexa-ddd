# ![RealWorld Example App](logo.png)

ASP.NET Core codebase containing real world examples (CRUD, auth, advanced patterns, etc) that adheres to the [RealWorld](https://github.com/gothinkster/realworld) spec and API.

## [RealWorld](https://github.com/gothinkster/realworld)

This repository contains a fullstack RealWorld implementation with a .NET backend and a Vue frontend. The backend provides the RealWorld API with CRUD operations, authentication, routing, pagination, and more.

The code follows the standard style guidelines of .NET and Vue 3 and is guided by established best practices for both ecosystems.

For more information about the RealWorld specification and compatible implementations, see the [RealWorld](https://github.com/gothinkster/realworld) repo.

## Goals

- Provide a backend-focused implementation of the RealWorld API for articles, comments, tags, users, profiles, favorites, and followers.
- Follow ideas from Hexagonal Architecture, Domain Driven Design, and vertical feature slices.
- Prefer .NET and Microsoft packages wherever possible. External packages should only be added when they provide clear value.
- Avoid framework abstractions for their own sake. Commands, queries, and handlers are intentionally small and project-specific.
- Keep local development comfortable with .NET Aspire, PostgreSQL, a YARP gateway, and the existing Vue frontend.

## Technologies

Backend:

- .NET 10
- ASP.NET Core Minimal APIs
- Entity Framework Core
- SQLite for simple local API runs
- PostgreSQL through .NET Aspire
- Microsoft OpenAPI with Scalar as the API UI
- JWT Bearer Authentication
- xUnit for unit and integration tests

Frontend:

- Vue
- Vite
- Pinia
- Playwright/Vitest for frontend tests

## Running

### Full Stack With Aspire

The preferred local startup path is the Aspire AppHost:

```bash
dotnet run --project backend/Conduit.AppHost/Conduit.AppHost.csproj
```

The AppHost starts and wires together:

- PostgreSQL
- the Conduit API
- the Vue/Vite frontend
- a YARP gateway

### API Only

The API can also be started directly without Aspire. By default, it uses SQLite with `realworld.db`.

```bash
dotnet run --project backend/Conduit/Conduit.csproj
```

The API uses `/api` as its path base. The API documentation is available at:

```text
/api/api-docs
```

## Tests

Backend tests:

```bash
dotnet test backend/Conduit.slnx
```

Frontend tests live under `frontend/` and are run through the npm scripts defined there.
