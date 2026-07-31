# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

ASP.NET Core + Vue 3 implementation of the [RealWorld](https://github.com/gothinkster/realworld) ("Conduit") spec — articles, comments, tags, users, profiles, favorites, followers. Backend goals (from root `README.md`): Hexagonal Architecture + DDD + vertical feature slices, prefer .NET/Microsoft packages over third-party ones, avoid speculative abstractions.

See `docs/README.md` for the full architecture doc set — don't duplicate it here, just the essentials and current-state gaps below.

## Language

Everything in this repository is English-only: code, identifiers, comments, documentation, and commit messages. Even if a prompt or conversation is in another language, write all repository output in English.

## Commands

All backend commands operate on the solution file `backend/Conduit.slnx` (`.slnx` format, not classic `.sln`).

```bash
# restore (locked mode — packages.lock.json files are committed)
dotnet restore backend/Conduit.slnx --locked-mode

# format check (CI enforces this)
dotnet format backend/Conduit.slnx --verify-no-changes

# build
dotnet build backend/Conduit.slnx --configuration Release

# run all backend tests (Microsoft.Testing.Platform runner, opted in via backend/global.json —
# must run from inside backend/ so that global.json is discovered, and pass the solution via --solution)
cd backend && dotnet test --solution Conduit.slnx

# run the full stack via .NET Aspire (Postgres + API + Vue frontend + YARP gateway)
dotnet run --project backend/Host/AppHost/Conduit.Host.AppHost.csproj

# run the API only (SQLite, realworld.db); Scalar API docs at /api-docs
dotnet run --project backend/Host/WebApi/Conduit.Host.WebApi.csproj
```

Frontend (`frontend/`, npm): `npm run dev`, `npm run build`, `npm run type-check`, `npx eslint . --max-warnings 0`, `npm run test:unit` (Vitest), `npm run test:e2e` (Playwright), `npm run generate:api` (regenerates the TS client from the RealWorld OpenAPI spec). CI runs exactly these (see `.github/workflows/ci.yml`).

