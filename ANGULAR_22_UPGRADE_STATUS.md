# Angular 22 Upgrade — Status

## Summary
The Ombi frontend has been upgraded from Angular 20 to **Angular 22** (core, CLI,
Material, CDK, build tooling, zone.js, TypeScript). The application **compiles**
(prod + dev) and boots. Modern, signal-based pages render correctly. However, a
**blocking change-detection regression** remains for pages built on the older
"subscribe → assign field → `*ngIf` in template" pattern.

## What works
- `yarn build` / `yarn build:dev` succeed (Angular 22.0.0).
- App boots and routes; wizard, discover/home, and requests render correctly.
- Login and landing pages render (via a targeted `ChangeDetectorRef.detectChanges()`
  workaround — see below).
- Cypress smoke specs (wizard + login features) pass 5/5 — but note these assert on
  the **URL**, not on rendered content, so they did not catch the issue below.

## Blocking issue: automatic change detection does not run for routed components
Pages that populate their view from plain async subscriptions render **blank
content** (e.g. the Settings pages: the nav and settings menu render, but the
content panel is empty). The data loads, but the view never re-renders.

### Confirmed root cause: TWO `ApplicationRef` instances
There are **two `ApplicationRef` instances** in the running app:
- Injecting `ApplicationRef` from a **component context** (`AppComponent`) reports
  `components.length === 1` / `viewCount === 1` — this is the one the views live in.
- Injecting `ApplicationRef` from the **HTTP interceptor** (and, by extension, the
  zone change-detection scheduler `NgZoneChangeDetectionScheduler`) reports
  `components.length === 0` / `viewCount === 0` — an **empty** ApplicationRef.

So Angular's automatic change detection (zone- or scheduler-driven) runs
`tick()` / `_tick()` on the **empty** ApplicationRef and never refreshes the real
view tree. That is why nothing re-renders on its own, while
`ChangeDetectorRef.detectChanges()` (which acts directly on the component's own
view) always works.

### Other confirmed diagnostics
- By default the app boots in the `<root>` zone (effectively zoneless) even though
  zone.js is bundled and active (`setTimeout` is patched).
- `provideZoneChangeDetection()` puts the app in the real `angular` NgZone and
  `NgZone.onMicrotaskEmpty` fires (~30×), but the scheduler ticks the empty
  ApplicationRef, so pages stay blank.
- A forced *global* pass (`applicationRef.dirtyFlags |= 1; applicationRef._tick()`)
  from both the interceptor (empty ref) and a component context (`AppComponent`)
  still did not refresh routed pages.
- There is a single copy of `@angular/core` in `node_modules` (no duplicate-version
  issue) and no second `bootstrapApplication` / `createApplication` call in the app
  or its dependencies — so the duplicate `ApplicationRef` comes from the injector /
  provider structure, which still needs to be pinned down.

### Fixes that were tried and did NOT resolve it
`provideZoneChangeDetection()`, `provideAnimations()` (instead of
`BrowserAnimationsModule`), `provideRouter()` (instead of
`importProvidersFrom(RouterModule.forRoot())`), switching from `withFetch()` to the
XHR backend, NGXS `NoopNgxsExecutionStrategy`, a manual
`NgZone.onMicrotaskEmpty → ApplicationRef.tick()` loop (from both the interceptor
and `AppComponent`), an HTTP interceptor that runs a global tick after each
response, moving the `<router-outlet>` out of the OnPush `mat-sidenav-content`,
removing the `ngTemplateOutlet` indirection around the outlet, a `router-outlet`
directive that re-attaches each activated view via `ApplicationRef.attachView`, and
bisecting the `importProvidersFrom(...)` NgModules (PrimeNG and Material globals
removed — ruled out). None made automatic change detection propagate to routed
components.

### Recommended next step for the root cause
Find what creates the **second `ApplicationRef`** (the empty one the scheduler/
interceptor receive). Likely candidates: a provider in `importProvidersFrom(...)`
(NGXS 3.8, `@auth0/angular-jwt`, `@ngx-translate`) introducing a separate
environment injector, or an Angular 22 interaction with `provideHttpClient(... ,
withInterceptorsFromDi())`. A minimal reproduction (bootstrap `AppComponent` with
providers added back one group at a time, logging `inject(ApplicationRef)` identity
from a component vs. an HTTP interceptor) should isolate it quickly. Once a single
`ApplicationRef` is restored, `provideZoneChangeDetection()` should make automatic
change detection work again.

### Current workaround (partial)
`login.component.ts` and `landingpage.component.ts` call
`ChangeDetectorRef.detectChanges()` after their async data arrives so those
(critical) pages render. This is a band-aid — the same treatment would be required
on every other affected component (most of the Settings module, etc.), which is not
a sustainable fix.

## Recommended next steps
1. Root-cause why the `NgZoneChangeDetectionScheduler` does not tick on
   `onMicrotaskEmpty` despite a real NgZone — likely a provider interaction from the
   legacy NgModules still imported via `importProvidersFrom(...)` (NGXS 3.8,
   PrimeNG 17, ngx-translate, JWT, Material modules). Bisecting those imports in a
   minimal reproduction is the fastest path.
2. Failing that, finish the signals / standalone migration for the affected
   components (see `STANDALONE_MIGRATION_PROGRESS.md`), or move them to
   `OnPush` + `async` pipe so change detection is driven by signals/async, not
   imperative field assignment.

## Environment notes
- Angular CLI 22 requires Node ≥ 22.22.3; build with Node 24.x if the CI image is
  on 22.22.2.
