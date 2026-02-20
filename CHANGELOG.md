## [4.55.5](https://github.com/Ombi-app/Ombi/compare/v4.55.4...v4.55.5) (2026-02-20)


### Bug Fixes

* **build:** silence remaining esbuild/sass warnings and remove unused imports ([1915b09](https://github.com/Ombi-app/Ombi/commit/1915b09e557128d43d70cdecb87f38277497bacb))
* **tmdb:** Fix rendering of the movie db page ([a0b84f7](https://github.com/Ombi-app/Ombi/commit/a0b84f74620f7e0b8c33a03fa3cc5f7d6d42ab93))



## [4.55.4](https://github.com/Ombi-app/Ombi/compare/v4.55.3...v4.55.4) (2026-02-20)


### Bug Fixes

* **emby/jellyfin:** fix [#5338](https://github.com/Ombi-app/Ombi/issues/5338) ([6217461](https://github.com/Ombi-app/Ombi/commit/62174617bbef0839527c179909cdbe50e90f5955))



## [4.55.3](https://github.com/Ombi-app/Ombi/compare/v4.55.2...v4.55.3) (2026-02-20)


### Performance Improvements

* Improvements to api calls ([6907604](https://github.com/Ombi-app/Ombi/commit/69076047f5ace434b175fa424acd78d87eeeb1de))



## [4.55.2](https://github.com/Ombi-app/Ombi/compare/v4.55.1...v4.55.2) (2026-02-10)



## [4.55.1](https://github.com/Ombi-app/Ombi/compare/v4.55.0...v4.55.1) (2026-02-10)


### Bug Fixes

* **plex:** content sync duplicate key errors ([cefe989](https://github.com/Ombi-app/Ombi/commit/cefe989d4efc9b2a686b50f47479d283992f9267))



# [4.55.0](https://github.com/Ombi-app/Ombi/compare/v4.54.5...v4.55.0) (2026-02-10)


### Features

* Add Ntfy ([dfcea55](https://github.com/Ombi-app/Ombi/commit/dfcea555887efdff4311d2a5734561ab7bbd9380))



## [4.54.5](https://github.com/Ombi-app/Ombi/compare/v4.54.4...v4.54.5) (2026-02-10)



## [4.54.4](https://github.com/Ombi-app/Ombi/compare/v4.54.3...v4.54.4) (2026-02-10)


### Bug Fixes

* src/Ombi/ClientApp/package.json & src/Ombi/ClientApp/yarn.lock to reduce vulnerabilities ([e55de4e](https://github.com/Ombi-app/Ombi/commit/e55de4e4e5496bd61d3f80d9f2719b8df04b1d4b))
* src/Ombi/ClientApp/package.json & src/Ombi/ClientApp/yarn.lock to reduce vulnerabilities ([80c96a7](https://github.com/Ombi-app/Ombi/commit/80c96a77c9667fc56b8f8c2d79ce0cc0f75e53f6))



## [4.54.3](https://github.com/Ombi-app/Ombi/compare/v4.54.2...v4.54.3) (2026-02-10)



## [4.54.2](https://github.com/Ombi-app/Ombi/compare/v4.54.1...v4.54.2) (2026-02-03)


### Bug Fixes

* Add missing using System to EmbyHelper ([bb6300f](https://github.com/Ombi-app/Ombi/commit/bb6300fe984dd8b7d5d3f10e64885301e80693d4))
* Correct EmbyHelper app.emby.media path detection ([9adb2ed](https://github.com/Ombi-app/Ombi/commit/9adb2ed4ef6deae79de1f1f2b5c8d249f9400e24))
* Correct EmbyHelper app.emby.media path detection ([88b5215](https://github.com/Ombi-app/Ombi/commit/88b52159e7582022b7261b79c24359edc8e70da2)), closes [#689](https://github.com/Ombi-app/Ombi/issues/689)
* Correct EmbyHelper app.emby.media path detection ([840d740](https://github.com/Ombi-app/Ombi/commit/840d740cd0860b46b185a82ef4855334b3f48301)), closes [#689](https://github.com/Ombi-app/Ombi/issues/689)
* Correct EmbyHelper app.emby.media path detection ([d16858d](https://github.com/Ombi-app/Ombi/commit/d16858dbbc5e0b1adb2c96fc0e5c6650a7431556))
* SonarrSync now deletes episodes per-series to prevent incomplete cache ([89b8c52](https://github.com/Ombi-app/Ombi/commit/89b8c5255b5f251d1eafc36df32b05fa6488657a)), closes [#5306](https://github.com/Ombi-app/Ombi/issues/5306)



## [4.54.1](https://github.com/Ombi-app/Ombi/compare/v4.54.0...v4.54.1) (2026-01-10)


### Bug Fixes

* **availability:** Make sure we check radarr/sonarr in the availability rules for it's prioritization [#5286](https://github.com/Ombi-app/Ombi/issues/5286) ([8af1d67](https://github.com/Ombi-app/Ombi/commit/8af1d678ac5e91b927ddabfad0f2194d8b763ebb))
* resolve inconsistencies in view at 768px screen width ([7cd4225](https://github.com/Ombi-app/Ombi/commit/7cd42257a333ec6e62ffd603fcd3dac77c9fe4b8))



# [4.54.0](https://github.com/Ombi-app/Ombi/compare/v4.53.3...v4.54.0) (2026-01-08)


### Bug Fixes

* reduce cognitive complexity ([acc5bb8](https://github.com/Ombi-app/Ombi/commit/acc5bb8a06950bb9426e99a851824c8dd768de1d))
* remove hard-coded item width for recently requested carousel ([5d79c80](https://github.com/Ombi-app/Ombi/commit/5d79c80462692a6d4d68f1469ec46e0d5f0bb994))
* remove negated condition ([6902895](https://github.com/Ombi-app/Ombi/commit/69028954021e09aaf95797746d24d4a8af754e8c))
* window size check ([4fd47c1](https://github.com/Ombi-app/Ombi/commit/4fd47c18b57793303dc4c75c7fd6acb15173e064))


### Features

* centre requested carousel items ([01c4fa9](https://github.com/Ombi-app/Ombi/commit/01c4fa916f5e6cc57cea459ad4300443ba457caa))
* use dynamic scroll sizing for recently requested carousel ([5b0f325](https://github.com/Ombi-app/Ombi/commit/5b0f3252f68d33f93c26b54cdf83780697ed6172))



## [4.53.3](https://github.com/Ombi-app/Ombi/compare/v4.53.2...v4.53.3) (2026-01-08)


### Bug Fixes

* small fix to search by decade ([7da1721](https://github.com/Ombi-app/Ombi/commit/7da1721a557e7aa2e92e760a7672b2dd2c88b060))



## [4.53.2](https://github.com/Ombi-app/Ombi/compare/v4.53.1...v4.53.2) (2026-01-08)


### Bug Fixes

* **availability:** Ensure that when Radarr/Sonarr has priority, stick to it [#5286](https://github.com/Ombi-app/Ombi/issues/5286) ([8f3f87a](https://github.com/Ombi-app/Ombi/commit/8f3f87a1896b5bc2b251fc9ed819933620a3bcfa))



## [4.53.1](https://github.com/Ombi-app/Ombi/compare/v4.53.0...v4.53.1) (2026-01-08)


### Bug Fixes

* **radarr/sonarr:** :bug: Sanitize usernames when adding them as tags to Radarr/Sonarr [#5307](https://github.com/Ombi-app/Ombi/issues/5307) ([d3d1d38](https://github.com/Ombi-app/Ombi/commit/d3d1d380d5695fcd6d55239966bbf18a0082d961))



# [4.53.0](https://github.com/Ombi-app/Ombi/compare/v4.52.0...v4.53.0) (2025-11-06)


### Features

* **availability:** :sparkles: Add the option for the *arr to take media availability priority ([d43a2e4](https://github.com/Ombi-app/Ombi/commit/d43a2e4efcbf9ea55e25305a52657dc107acaee3)), closes [#5286](https://github.com/Ombi-app/Ombi/issues/5286)



# [4.52.0](https://github.com/Ombi-app/Ombi/compare/v4.51.6...v4.52.0) (2025-11-06)


### Features

* allow admins to force Plex watchlist revalidation ([4fa17a8](https://github.com/Ombi-app/Ombi/commit/4fa17a8f8155a6eeb40840444eca01a96f0134e0))


### Reverts

* Revert "deterministic refresh (rather than 3 second wait)" ([a8bd017](https://github.com/Ombi-app/Ombi/commit/a8bd01793f67d86f6da345d7a3fe3adbaee920ca))
* Revert "address sonarqube feedback" ([10992c7](https://github.com/Ombi-app/Ombi/commit/10992c78b6c9c33fa5b31407b02fc8eee9be59f2))



## [4.51.6](https://github.com/Ombi-app/Ombi/compare/v4.51.5...v4.51.6) (2025-10-15)


### Bug Fixes

* **emby:** :bug: Skip very large multipart episodes ([fb70aa1](https://github.com/Ombi-app/Ombi/commit/fb70aa16dea5e682a27878c0845d2ef2cf3e7b07))



## [4.51.5](https://github.com/Ombi-app/Ombi/compare/v4.51.4...v4.51.5) (2025-10-14)


### Bug Fixes

* **emby:** actually fix the dupes ([4167942](https://github.com/Ombi-app/Ombi/commit/41679427f65cfa6756ab8bb1d89eb37cf8a40b42))



## [4.51.4](https://github.com/Ombi-app/Ombi/compare/v4.51.3...v4.51.4) (2025-10-14)


### Bug Fixes

* **emby:** fixed duplicate episodes ([a1d3755](https://github.com/Ombi-app/Ombi/commit/a1d37554bdb9167dc82df121caa194e7641142df))



## [4.51.3](https://github.com/Ombi-app/Ombi/compare/v4.51.2...v4.51.3) (2025-10-14)



## [4.51.2](https://github.com/Ombi-app/Ombi/compare/v4.51.1...v4.51.2) (2025-10-10)


### Bug Fixes

* **emby:** :bug: Fixed where we didn't scan episodes for mixed content libraries ([b815853](https://github.com/Ombi-app/Ombi/commit/b81585363ff5fd8eb06a814f36f20692077cdb27))



## [4.51.1](https://github.com/Ombi-app/Ombi/compare/v4.51.0...v4.51.1) (2025-10-09)


### Bug Fixes

* **user-management:** Put back the user filter ([cb63060](https://github.com/Ombi-app/Ombi/commit/cb63060c778011927dc280f47f8b1836b6739150))



# [4.51.0](https://github.com/Ombi-app/Ombi/compare/v4.50.2...v4.51.0) (2025-10-06)


### Bug Fixes

* **translations:** 🌐 New translations from Crowdin [skip ci] ([102c84e](https://github.com/Ombi-app/Ombi/commit/102c84edb0df41f2f9ca27fd9d76f1e9cb22e755))


### Features

* improve contributor guidance ([d045f32](https://github.com/Ombi-app/Ombi/commit/d045f32b85fd4c019239e13a6a0eefb842ed1b31))
* **TvSender:** add logging for missing seasons in Sonarr during monitoring updates ([0d219e4](https://github.com/Ombi-app/Ombi/commit/0d219e4612ce047d7a507c2447ed6d76d616768e))


### Reverts

* Revert "Update src/Ombi/Controllers/V1/TokenController.cs" ([0294dba](https://github.com/Ombi-app/Ombi/commit/0294dba4cc8d27ace0503fd2518b4419c3f0f08f))



## [4.50.2](https://github.com/Ombi-app/Ombi/compare/v4.50.1...v4.50.2) (2025-10-04)



## [4.50.1](https://github.com/Ombi-app/Ombi/compare/v4.50.0...v4.50.1) (2025-09-26)


### Bug Fixes

* **sonarr:** :bug: Ensure we are monitoring shows that already exist in Sonarr [#5257](https://github.com/Ombi-app/Ombi/issues/5257) ([bf83c95](https://github.com/Ombi-app/Ombi/commit/bf83c95da05feefa956ea73a3959304af94483dd))



# [4.50.0](https://github.com/Ombi-app/Ombi/compare/v4.49.10...v4.50.0) (2025-09-21)


### Features

* pipes ([1ac20e8](https://github.com/Ombi-app/Ombi/commit/1ac20e84dbf9b90e6797cb49cf3fd4e863f7352d))



## [4.49.10](https://github.com/Ombi-app/Ombi/compare/v4.49.9...v4.49.10) (2025-09-19)



## [4.49.9](https://github.com/Ombi-app/Ombi/compare/v4.49.8...v4.49.9) (2025-09-16)



## [4.49.8](https://github.com/Ombi-app/Ombi/compare/v4.49.7...v4.49.8) (2025-09-13)



## [4.49.7](https://github.com/Ombi-app/Ombi/compare/v4.49.6...v4.49.7) (2025-09-11)


### Bug Fixes

* Use new server discovery url [#5260](https://github.com/Ombi-app/Ombi/issues/5260) ([56a91d6](https://github.com/Ombi-app/Ombi/commit/56a91d6240f53a4306fcd0648575976f9d459048))



## [4.49.6](https://github.com/Ombi-app/Ombi/compare/v4.49.5...v4.49.6) (2025-08-24)



## [4.49.5](https://github.com/Ombi-app/Ombi/compare/v4.49.4...v4.49.5) (2025-08-23)


### Bug Fixes

* set MarkedAsApproved on TV requests ([57d3880](https://github.com/Ombi-app/Ombi/commit/57d3880115f8e65e7d7d522aaa725b01878b45fe))



## [4.49.4](https://github.com/Ombi-app/Ombi/compare/v4.49.3...v4.49.4) (2025-08-23)



## [4.49.3](https://github.com/Ombi-app/Ombi/compare/v4.49.2...v4.49.3) (2025-08-17)


### Bug Fixes

* **plex-api:** update Plex Watchlist URL ([11fd7a5](https://github.com/Ombi-app/Ombi/commit/11fd7a5fc853da75974a16bf4fdecd72a836f54b))



## [4.49.2](https://github.com/Ombi-app/Ombi/compare/v4.49.1...v4.49.2) (2025-07-12)


### Performance Improvements

* **discover:** :zap: Improve the loading performance on the discover page ([97d5167](https://github.com/Ombi-app/Ombi/commit/97d5167db6c9f915021f32b96b281d7db3741d7f))



## [4.49.1](https://github.com/Ombi-app/Ombi/compare/v4.49.0...v4.49.1) (2025-07-12)


### Bug Fixes

* **auth:** Fixed an issue where refreshing the page as a power user would stop the application from loading [#5242](https://github.com/Ombi-app/Ombi/issues/5242) ([cee4014](https://github.com/Ombi-app/Ombi/commit/cee40146ee02f7fb79e2019d6fe2f9d5c5dbdfc8))



# [4.49.0](https://github.com/Ombi-app/Ombi/compare/v4.48.5...v4.49.0) (2025-07-11)


### Features

* Added the ability for the Watchlist to automatically refresh the users token. This will reduce the need for the user to log in ([067c029](https://github.com/Ombi-app/Ombi/commit/067c029f42e9fd853d060fdb2093013b15ac14c0))



## [4.48.5](https://github.com/Ombi-app/Ombi/compare/v4.48.4...v4.48.5) (2025-05-14)


### Bug Fixes

* filter out excluded notification agents from user preferences ([c9ab4f4](https://github.com/Ombi-app/Ombi/commit/c9ab4f4f9faa66dbf263da693db1eefcf68beeec)), closes [#5196](https://github.com/Ombi-app/Ombi/issues/5196)



## [4.48.4](https://github.com/Ombi-app/Ombi/compare/v4.48.3...v4.48.4) (2025-05-14)


### Bug Fixes

* **translations:** 🌐 New translations from Crowdin [skip ci] ([dbbfdd9](https://github.com/Ombi-app/Ombi/commit/dbbfdd926f0808f6d16f0b2cd8b5406e6b610c82))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([53a6a09](https://github.com/Ombi-app/Ombi/commit/53a6a092b14b8b8bdbff95d066926d3dbe6951f4))
* **ui:** correct timezone handling in OmbiDatePipe ([f88c5ad](https://github.com/Ombi-app/Ombi/commit/f88c5ad818fadea7064e7dfbe46f07eae855109a)), closes [#5102](https://github.com/Ombi-app/Ombi/issues/5102)



