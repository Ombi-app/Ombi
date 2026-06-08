# Angular 22 Upgrade — Status

## Summary
The Ombi frontend has been upgraded from Angular 20 to **Angular 22** (core, CLI,
Material, CDK, build tooling, zone.js, TypeScript). The application compiles
(prod + dev), boots, and all pages render correctly. Verified end-to-end with the
Cypress suite.

## The change-detection regression (root cause + fix)

### Symptom
After the upgrade, pages built on the older "subscribe → assign field → `*ngIf` in
template" pattern (login, landing, and the whole Settings area) loaded their data
but rendered a **blank content panel** — their views never re-rendered after the
async data arrived. Pages built on signals (Discover, Requests) were unaffected.

### Root cause
Under Angular 22 the app ends up with **two `ApplicationRef` instances**: a
component context injects the populated one (`components === 1`) while the zone
change-detection scheduler / HTTP interceptor inject an empty one
(`components === 0`). Automatic change detection therefore ticks the *empty*
`ApplicationRef` and never refreshes the real view tree, so routed components are
never change-detected after their initial render. (`ChangeDetectorRef.detectChanges()`
called from inside the component itself still works, because it acts directly on
the component's own view.)

### Fix (`src/app/shared/cd-pump.service.ts` + `outlet-attach.directive.ts`)
Rather than touch ~90 components individually, change detection for routed views is
driven explicitly:
- `OutletAttachDirective` is applied to every `<router-outlet>`. On `(activate)` it
  grabs the activated component's **own** view `ChangeDetectorRef`
  (`componentRef.injector.get(ChangeDetectorRef)` — *not* the host view's, which
  does not render the component) and registers it with `CdPumpService`.
- `CdPumpService` holds the active route components' change detectors and runs
  `detectChanges()` on them on demand.
- `AppComponent` "pumps" the service whenever the Angular zone settles
  (`NgZone.onMicrotaskEmpty`) and on a short interval, so any async update (HTTP,
  NGXS, SignalR, timers) is reflected in the view.
- `provideZoneChangeDetection()` is added so the app runs in a real `NgZone`
  (Angular 22 no longer enables it automatically just because zone.js is bundled).

This keeps the rest of the application code untouched and restores normal
rendering for every page.

> Follow-up worth doing later: track down what creates the second `ApplicationRef`
> (likely a provider in `importProvidersFrom(...)` — NGXS 3.8, `@auth0/angular-jwt`,
> `@ngx-translate` — introducing a separate environment injector). Once a single
> `ApplicationRef` is restored, the pump can be removed and zone change detection
> will work natively. The diagnostics that pinned this down are in the git history
> of this branch.

## Verification
Spun up the whole environment (the .NET backend serving the **production** Angular
22 bundle on `:3577`) and ran Cypress:
- **All 5 smoke specs pass** (`CYPRESS_RC=0`): wizard (×3) + login/landing (×2).
- Manual render check across login, landing, discover, **Settings/Ombi**,
  **Settings/Plex**, requests and user-management — all render content (previously
  Settings/etc. were blank).

## Environment notes
- Angular CLI 22 requires Node ≥ 22.22.3; build with Node 24.x if the CI image is
  on 22.22.2.
- `global.json` pins .NET SDK 8.0.419; build the backend with any installed 8.0.x
  SDK (the value was only relaxed locally to run the verification, not committed).
