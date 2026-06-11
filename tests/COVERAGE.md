# Ombi Cypress Automation — Coverage & Stability Notes

This document captures the current state of the Cypress end‑to‑end suite: how to
run it locally, what is covered today, where the biggest gaps are, and the known
stability hazards. It is meant to be a living map for anyone extending the suite.

## How the suite is wired up

The end‑to‑end stack has three moving parts (mirrored by
`.github/workflows/automation-tests.yml`):

| Part | Command | Port |
|------|---------|------|
| Angular dev server | `yarn --cwd src/Ombi/ClientApp start` | `3578` |
| Ombi backend (.NET) | `dotnet run --project ./src/Ombi -- --host 'http://*:3577'` | `3577` |
| Wiremock (Plex mock) | `docker run --rm -p 32400:8080 wiremock/wiremock:2.35.0` | `32400` |

The backend proxies the SPA, so Cypress points at `http://localhost:3577`
(`cypress.config.ts` → `baseUrl`). The default database is SQLite; CI also runs a
full MySQL pass and Postgres/SQLite smoke passes.

### Running locally

```bash
# 1. install
yarn --cwd src/Ombi/ClientApp install
yarn --cwd tests install

# 2. start the SPA + backend (each in its own shell, or backgrounded)
yarn --cwd src/Ombi/ClientApp start
dotnet run --project ./src/Ombi -- --host 'http://*:3577'

# 3. (optional, only needed for the Plex settings spec) start Wiremock
docker run --rm -p 32400:8080 wiremock/wiremock:2.35.0

# 4. run the tests
cd tests
npx cypress run                       # whole suite
npx cypress run --spec 'cypress/tests/login/login.spec.ts'   # one spec
```

The first spec of every run (except the wizard feature) calls `cy.ensureSetup()`,
which idempotently creates the `a`/`a` admin user through the wizard API so specs
no longer depend on the wizard UI having run first.

## What is covered today

Specs live under `cypress/tests/**` (plus two Cucumber features under
`cypress/features/**`). Latest local baseline: **19 specs, ~105 tests, 96
passing**; the only failures are the Plex spec when Wiremock is absent and a few
intentionally `.skip`ped cases.

| Area | Spec(s) | Notes |
|------|---------|-------|
| Wizard first‑run | `features/01-wizard` | happy path + validation |
| Login | `features/login`, `tests/login` | OAuth toggle, bad creds, success |
| Discover | `tests/discover/*` (cards, card‑requests, recently‑requested, responsive) | richest area |
| Media details | `tests/details/movie`, `tests/details/tv` | request/approve/available buttons, info panel, season grid |
| Search | `tests/search` | filters, multi‑results, empty results |
| Requests list | `tests/requests` | TV details navigation + delete only |
| Navigation bar | `tests/navigation` | admin vs non‑admin visibility |
| User management | `tests/usermanagement` | create/delete/limits/roles/notifications |
| User preferences | `tests/user-preferences` | profile + security |
| Settings → Plex | `tests/settings/plex` | the only settings page with coverage |
| Settings → Customization | `tests/settings/customization` | **added here** |
| API (v1/v2) | `tests/api/v1/*` | movie/tv request + tv search contract checks |

## Biggest coverage gaps (in priority order)

1. **Settings pages** — the app exposes ~30 settings screens
   (`src/Ombi/ClientApp/src/app/settings/*`). Only **Plex** had any coverage
   before this change; **Customization** is added here. Still uncovered and
   high‑value: **General/Ombi**, **Features**, **Authentication**, **Radarr**,
   **Sonarr**, **Lidarr**, **Emby/Jellyfin**, **Notifications** (email/discord/
   telegram/…), **Jobs/Scheduled tasks**, **Landing Page**, **Customization
   advanced**. Many of these are fully self‑contained (no external service) and
   are therefore cheap, deterministic targets — see Customization as a template.
2. **Issues** feature (`app/issues`, `settings/issues`) — no coverage.
3. **Vote** feature (`app/vote`, `settings/vote`) — no coverage.
4. **Requests list** — only TV navigation + delete is covered. No movie‑tab
   coverage, no approve/deny/filter/search within the requests grid.
5. **Custom page / landing page** rendering — only touched indirectly via the
   login landing‑settings intercept.
6. **Unsubscribe** flow — no coverage.

## Stability hazards to be aware of

These are the patterns most likely to make the suite flaky. New tests should
prefer the deterministic alternatives.

- **Shared database state.** All specs run against one persistent SQLite DB.
  Several specs make *real* requests (`cy.requestMovie`, `cy.requestAllTv`) that
  persist, and other specs assume a particular item is *not yet requested*. The
  `discover-recently-requested` and `details/*` specs assume the requested item
  appears at `body[0]`. Prefer stubbing availability in the response
  (`cy.intercept(... req.reply ...)`) over relying on DB state, and force the
  full state you need (`requested/approved/available/denied`) rather than just
  one field.
- **Live TMDb dependency.** `search`, `details/*` and the discover request specs
  drive real TheMovieDb calls through the backend, so they break if TMDb is
  unreachable or if popular/search ordering shifts. Where the test isn't *about*
  TMDb data, intercept and serve a controlled response.
- **The discover carousel double‑loads.** `carousel-list.component` issues a
  second page request when the first returns `< 20` items. A static fixture
  served to `**/search/Tv/popular/**` is therefore appended to itself, producing
  **duplicate card ids** that break `*ngIf`/selector resolution. Keep the live
  popular response (overriding a single card) rather than a small fixture, or
  ensure the fixture has ≥ 20 unique items.
- **Hover‑gated controls.** Discover card request buttons and the
  recently‑requested approve buttons are revealed on hover/focus. `realHover()`
  once before asserting is racy; re‑assert the hover inside a `waitUntil` poll,
  or use `.focus()` for `:focus-within`‑driven reveals (see
  `DetailedCard.reveal()`).
- **Arbitrary `cy.wait(<ms>)`.** A few specs sleep a fixed number of
  milliseconds to wait for async availability lookups. Prefer waiting on an
  explicit signal — an intercept alias, or `should('exist')` on the element the
  async work renders.
- **The last two TV tests in `discover-cards-requests` are order‑sensitive.**
  When that spec is run in isolation, the popular `**/search/Tv/popular/**`
  request stops firing for the final two TV cases (the SPA appears to serve the
  popular payload from cache after several `/discover` loads), so they only pass
  reliably inside the full‑suite run. Worth hardening (e.g. disabling the service
  worker / cache‑busting the popular call) before adding more discover specs.
- **Plex spec needs Wiremock.** `tests/settings/plex` talks to a Wiremock
  instance on `:32400`; without Docker/Wiremock it fails. It is not a code
  regression.

## What this change adds

- A new, fully self‑contained **Customization settings** spec
  (`tests/settings/customization/customization-settings.spec.ts`) plus its page
  object — the first coverage of a non‑Plex settings page. It is deterministic:
  no external service, every assertion waits on an explicit signal (element
  render, intercepted save response, or a retried `have.value` assertion).
- Stable `data-test` selectors on the Customization component
  (`applicationName`, `applicationUrl`, `hideAvailableFromDiscover`, `save`) so
  the page can be driven without brittle positional selectors. This is the
  pattern to replicate when covering the other settings pages.
