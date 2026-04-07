## [4.58.2](https://github.com/Ombi-app/Ombi/compare/v4.58.1...v4.58.2) (2026-04-07)



## [4.58.1](https://github.com/Ombi-app/Ombi/compare/v4.58.0...v4.58.1) (2026-04-07)



# [4.58.0](https://github.com/Ombi-app/Ombi/compare/v4.55.7...v4.58.0) (2026-04-06)


### Bug Fixes

* disable font inlining in Angular production build ([713c2fd](https://github.com/Ombi-app/Ombi/commit/713c2fd5e9456efbf6da016db67e4b3364c82fb0))
* extend request cache TTL and add null guards for TV results ([195c989](https://github.com/Ombi-app/Ombi/commit/195c9898ebcae816fed73ff12859b55b67f3def1))
* guard Docker image push to master/develop branches only ([86427e5](https://github.com/Ombi-app/Ombi/commit/86427e57df1cf91dc68a911870939db9a208fae2))
* make discover page processing sequential to fix test failures ([aa087c7](https://github.com/Ombi-app/Ombi/commit/aa087c7b9b860b4c39283b54163c95da83d5b08a))
* move API throttle inside cache factory to avoid contention on cache hits ([4e41ce6](https://github.com/Ombi-app/Ombi/commit/4e41ce6dcf9fc900a193338ac729be56d8ab28ce))
* parallelize discover page API calls and processing for faster load times ([c417eea](https://github.com/Ombi-app/Ombi/commit/c417eeaa68f313a37369726fa4917d4550a7fa71))
* remove SemaphoreSlim throttle - unnecessary given TMDB rate limits ([feee9f8](https://github.com/Ombi-app/Ombi/commit/feee9f8514b07ddd877615eb42beedd470daeef5))
* remove unsupported linux/arm/v7 platform from Docker build ([90d502f](https://github.com/Ombi-app/Ombi/commit/90d502f8c3359d0d09c229f501a6478abe81c230))
* restore missing SearchViewModel using in Emby and Jellyfin rules ([afad1c9](https://github.com/Ombi-app/Ombi/commit/afad1c9890c30cb5483a9b6b8e61e2b96a345eaa))
* restore TransformMovieResultsToResponse to match original code ([5cd43e2](https://github.com/Ombi-app/Ombi/commit/5cd43e282e609be953c5b9527e901111bb9df24b))
* run search rules sequentially to avoid DbContext thread-safety issues ([7172269](https://github.com/Ombi-app/Ombi/commit/7172269d1026a43a8cfcb87a29f1ccfd45bab8fd))
* set PartlyAvailable when fully available with unaired episodes ([783620b](https://github.com/Ombi-app/Ombi/commit/783620bc7ea6b99e75480757ab8cdf6b1622fe62))
* the post path on the requests page for TV ([089eef5](https://github.com/Ombi-app/Ombi/commit/089eef5e30e0e2dc2cdc13d709678c58ad0c0b92))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([cb6e7ce](https://github.com/Ombi-app/Ombi/commit/cb6e7ce96cbe25a9ad02a69d710440d0a032b5b1))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([cbeb8a3](https://github.com/Ombi-app/Ombi/commit/cbeb8a325e56634695348b2737cd4796138e8342))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([116775e](https://github.com/Ombi-app/Ombi/commit/116775ecbccafdcb70948f4f464444d370ff2bfb))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([5a561c9](https://github.com/Ombi-app/Ombi/commit/5a561c974c8000cbb361ee35aa0091e1dea91ae2))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([8474267](https://github.com/Ombi-app/Ombi/commit/8474267fbd59e841597f62593db4401792c9f495))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([ff56bce](https://github.com/Ombi-app/Ombi/commit/ff56bce713cc1f3f021cb3de0fd61f4035ec0138))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([3cb52a2](https://github.com/Ombi-app/Ombi/commit/3cb52a27aa39c3972f855ee22307957cfd53609b))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([9446001](https://github.com/Ombi-app/Ombi/commit/94460017111d493e9678a11684d01c239fc97cf9))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([12c6c8a](https://github.com/Ombi-app/Ombi/commit/12c6c8a37d57375dc5520a66bf3cedb9ef5b57c5))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([b16dcef](https://github.com/Ombi-app/Ombi/commit/b16dcef1925ca087cc677873c7abae450f2f1d7f))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([f04cf8d](https://github.com/Ombi-app/Ombi/commit/f04cf8d8ce2d8899fefb2e55c5e5647d1da3ced4))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([f14ea54](https://github.com/Ombi-app/Ombi/commit/f14ea540912bc738d2cacef96a851424867cdb90))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([6ed8ebc](https://github.com/Ombi-app/Ombi/commit/6ed8ebc64bc4418d771f5dde734e9a8048d812a0))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([6aeeafb](https://github.com/Ombi-app/Ombi/commit/6aeeafb02fe8eb9f51de4f69bbda036889f4c226))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([cbf7421](https://github.com/Ombi-app/Ombi/commit/cbf742197286cef05d344112683f0d86c1620f08))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([9c785fb](https://github.com/Ombi-app/Ombi/commit/9c785fb7dcbcb43b26af5aabc8746cccd2c69744))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([32ecec1](https://github.com/Ombi-app/Ombi/commit/32ecec1cdf7b4858c0a37e9588f88cc9e0379bb3))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([7497cf2](https://github.com/Ombi-app/Ombi/commit/7497cf2b6d4c5ca1d8f140d3198b66e9b098030d))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([6d92d48](https://github.com/Ombi-app/Ombi/commit/6d92d4839889417e3fd3b2b95c82133ae4d17f7a))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([3236324](https://github.com/Ombi-app/Ombi/commit/3236324efd0c40c173738fe5fbd68f9f18e638cd))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([2e2aacd](https://github.com/Ombi-app/Ombi/commit/2e2aacd1767f6c6b7571f6d40e6454caa939f4a0))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([c282583](https://github.com/Ombi-app/Ombi/commit/c2825833c17b64b70ba5166ca2d5e34c2a35b783))
* update actions/checkout to v4 in docker job ([d8c2042](https://github.com/Ombi-app/Ombi/commit/d8c2042e037cbaf73c4064eb57be13cbebbc5066))
* update Dockerfile COPY commands to match current project structure ([3d67329](https://github.com/Ombi-app/Ombi/commit/3d6732919d8208d7552df25f6d4f4e700a825cd1))
* use DateTime.Now.Date consistently in AvailabilityRuleHelper ([59dca57](https://github.com/Ombi-app/Ombi/commit/59dca5799ba1701ae06d5762a23799c7a60495de))


### Features

* add Docker image publishing to CI pipeline ([2268f7b](https://github.com/Ombi-app/Ombi/commit/2268f7b8f4b72001c1821e892d1f43e09af5e4fe))



## [4.55.7](https://github.com/Ombi-app/Ombi/compare/v4.55.6...v4.55.7) (2026-03-01)



## [4.55.6](https://github.com/Ombi-app/Ombi/compare/v4.55.5...v4.55.6) (2026-02-22)


### Bug Fixes

* base url issue [#5343](https://github.com/Ombi-app/Ombi/issues/5343) ([fb39386](https://github.com/Ombi-app/Ombi/commit/fb39386e1615cd2eabb882484b64e6a76485e4cb))



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



