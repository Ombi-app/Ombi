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

### Confirmed diagnostics
- By default the app boots in the `<root>` zone (effectively zoneless) even though
  zone.js is bundled and active (`setTimeout` is patched).
- Adding `provideZoneChangeDetection()` puts the app in the real `angular` NgZone
  (`NgZone` is `_NgZone`, not Noop) and `NgZone.onMicrotaskEmpty` fires (~30×),
  **but the change-detection scheduler still never ticks** — pages stay blank.
- `ApplicationRef` has the bootstrapped `AppComponent` attached
  (`components.length === 1`), yet `ApplicationRef.tick()` (called manually, via an
  HTTP interceptor `finalize`, and via `onMicrotaskEmpty`) does **not** re-render
  routed components.
- `ChangeDetectorRef.detectChanges()` on an individual component **does** render it.
- No component in the routing chain (AppComponent → router-outlet → page) is
  `OnPush`; no dependency enables zoneless change detection.

### Fixes that were tried and did NOT resolve it
`provideZoneChangeDetection()`, `provideAnimations()` (instead of
`BrowserAnimationsModule`), `provideRouter()` (instead of
`importProvidersFrom(RouterModule.forRoot())`), switching from `withFetch()` to the
XHR backend, NGXS `NoopNgxsExecutionStrategy`, a manual
`NgZone.onMicrotaskEmpty → ApplicationRef.tick()` loop, and an HTTP interceptor that
ticks after each response. None made automatic change detection propagate to routed
components.

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
