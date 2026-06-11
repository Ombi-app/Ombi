# Ombi.Api.IntegrationTests

In-memory API **contract tests** for the endpoints the Ombi mobile app
(ReactNativeOmbi) consumes. They boot the real API with
`WebApplicationFactory`/`TestServer` and assert the HTTP status and the
serialized JSON shape (property names + casing) of each response, so a breaking
change to a consumed contract fails CI instead of silently breaking the app at
runtime.

## How it works

`Harness/OmbiApiTestFactory` hosts a trimmed-down `TestStartup` that reuses the
production `Startup.ConfigureServices` but replaces the HTTP pipeline so the
parts that make in-memory hosting impractical are not started:

- **Quartz** scheduling, the **Angular SPA** static files and the **SignalR**
  hub are not wired up.
- The three EF Core contexts point at a fresh **temp-file SQLite** database
  (real migrations run via the concrete `*SqliteContext` constructors).
- Every request is authenticated as an admin/power user via a deterministic
  `TestAuthHandler`.
- Search/request/Plex engines that would otherwise call TheMovieDb, Plex, etc.
  are swapped for Moq mocks, so the tests pin the wire shape without external
  calls. `IMediaCacheService` is replaced with a pass-through.

## Coverage

- **Search (v2):** popular/top-rated/upcoming/now-playing movies,
  popular/trending/anticipated tv, movie & tv details, multi-search,
  movie/tv streams, actor credits, Rotten Tomatoes ratings.
- **Requests:** paginated movie/tv lists, create movie/tv, approve/deny
  (movie + tv child), mark available, advanced options, subscribe/unsubscribe,
  delete, request info, tv children, recently requested.
- **Identity:** current user, user list, dropdown, claims.
- **Issues:** categories, list, comments.
- **Settings:** customization, sonarr, authentication, plex, client id,
  issues-enabled.
- **Radarr/Sonarr:** enabled, profiles, root folders.
- **Recently added:** movies, tv. **Plex:** libraries.
- **Mobile:** push-token register/remove. **Token:** unauthorized login contract.

## Intentionally out of scope (follow-ups)

These need infrastructure beyond a contract harness and are deliberately not
covered here:

- **Job triggers** (`api/v1/Job/*`) - require a running Quartz scheduler.
- **Settings writes** (`POST sonarr`/`plex`) - schedule Quartz jobs on save.
- **Tester endpoints** (`tester/plex`, `tester/sonarr`) - exercise live external
  connections.
- **Images** (`api/v1/Images/background/*`) - proxy binary/image data from
  external providers.

## Running

```bash
cd src
dotnet test Ombi.Api.IntegrationTests/Ombi.Api.IntegrationTests.csproj -c Release
```

The tests target `net8.0` and run as part of the solution-wide `dotnet test` in
CI. On a Linux distro whose RID-specific apphost pack is unavailable locally,
add `-p:UseAppHost=false` (only needed to build the referenced `Ombi` host
project locally; CI is unaffected).
