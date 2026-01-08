# [4.47.0](https://github.com/Ombi-app/Ombi/compare/v4.46.8...v4.47.0) (2025-01-03)


### Features

* **wizard:** :sparkles: Added the ability to start with a different database ([#5208](https://github.com/Ombi-app/Ombi/issues/5208)) ([cc98fc6](https://github.com/Ombi-app/Ombi/commit/cc98fc6aca27111a8afc3b7b5b8e53207b73fe15))



## [4.46.8](https://github.com/Ombi-app/Ombi/compare/v4.46.7...v4.46.8) (2025-01-03)


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



## [4.48.3](https://github.com/Ombi-app/Ombi/compare/v4.48.2...v4.48.3) (2025-05-14)


### Bug Fixes

* Correct 4K movie request existence check ([ba6e708](https://github.com/Ombi-app/Ombi/commit/ba6e708e189f52f2ff4ebc073fa38a4f53f1061c)), closes [#4798](https://github.com/Ombi-app/Ombi/issues/4798)



## [4.48.2](https://github.com/Ombi-app/Ombi/compare/v4.48.1...v4.48.2) (2025-05-14)


### Bug Fixes

* **radarr:** ensure RequestedUser is loaded when creating tags ([f8658fe](https://github.com/Ombi-app/Ombi/commit/f8658fe6d56488aa5caa68093245cbf021a31810)), closes [#5045](https://github.com/Ombi-app/Ombi/issues/5045)



## [4.48.1](https://github.com/Ombi-app/Ombi/compare/v4.48.0...v4.48.1) (2025-05-14)



# [4.48.0](https://github.com/Ombi-app/Ombi/compare/v4.47.3...v4.48.0) (2025-05-14)


### Features

* added the watchlist notification ([0dfd453](https://github.com/Ombi-app/Ombi/commit/0dfd4533dba01cb04ea2217c020de8833ddf39c6))



## [4.47.3](https://github.com/Ombi-app/Ombi/compare/v4.47.2...v4.47.3) (2025-04-13)


### Bug Fixes

* [#5223](https://github.com/Ombi-app/Ombi/issues/5223) ([cf0c161](https://github.com/Ombi-app/Ombi/commit/cf0c1614a496b4f7cf19d78e885c3e37dae5cf0f))



## [4.47.2](https://github.com/Ombi-app/Ombi/compare/v4.47.0...v4.47.2) (2025-03-11)


### Bug Fixes

* **user-import:** Do not import users that do not have access to the server [#5064](https://github.com/Ombi-app/Ombi/issues/5064) ([a801cfd](https://github.com/Ombi-app/Ombi/commit/a801cfdb0946cbee3c35b7e917a240f69020f221))



# [4.47.0](https://github.com/Ombi-app/Ombi/compare/v4.46.8...v4.47.0) (2025-01-03)


### Features

* **wizard:** :sparkles: Added the ability to start with a different database ([#5208](https://github.com/Ombi-app/Ombi/issues/5208)) ([cc98fc6](https://github.com/Ombi-app/Ombi/commit/cc98fc6aca27111a8afc3b7b5b8e53207b73fe15))



## [4.46.8](https://github.com/Ombi-app/Ombi/compare/v4.46.7...v4.46.8) (2025-01-03)


### Bug Fixes

* **radarr-settings:** this.normalForm is undefined ([#5207](https://github.com/Ombi-app/Ombi/issues/5207)) ([dc2b958](https://github.com/Ombi-app/Ombi/commit/dc2b958915bf6cb77e093ada843ef6d9f62a3755)), closes [#4994](https://github.com/Ombi-app/Ombi/issues/4994)



## [4.46.7](https://github.com/Ombi-app/Ombi/compare/v4.46.6...v4.46.7) (2024-12-03)


### Bug Fixes

* **requests:** :bug: Power users can now set profiles and root folders when requesting ([138df1e](https://github.com/Ombi-app/Ombi/commit/138df1eb25c709c1939d01d4c9f9ece63f8e0fde))



## [4.46.6](https://github.com/Ombi-app/Ombi/compare/v4.46.5...v4.46.6) (2024-11-24)



## [4.46.5](https://github.com/Ombi-app/Ombi/compare/v4.46.4...v4.46.5) (2024-11-23)


### Bug Fixes

* **Fixed the UI not applying the correct timezone settings:** :bug: ([029ea79](https://github.com/Ombi-app/Ombi/commit/029ea7919220fbc506898733caeb4370053051a7))



## [4.46.4](https://github.com/Ombi-app/Ombi/compare/v4.46.3...v4.46.4) (2024-09-09)



## [4.46.3](https://github.com/Ombi-app/Ombi/compare/v4.46.2...v4.46.3) (2024-09-07)


### Bug Fixes

* Show the ApiAlias in the requests-list ([9ff624c](https://github.com/Ombi-app/Ombi/commit/9ff624ce4646815b239fbb8327117947f0a90e4b))



## [4.37.2](https://github.com/Ombi-app/Ombi/compare/v4.37.1...v4.37.2) (2023-05-03)


### Bug Fixes

* **jellyfin:** Fixed an issue where the sync could stop working. Removed unused properties so the deseralization no longer fails ([0e5e0ad](https://github.com/Ombi-app/Ombi/commit/0e5e0adf862701d0f672beff14ec0aa75e4b5220))



## [4.37.1](https://github.com/Ombi-app/Ombi/compare/v4.37.0...v4.37.1) (2023-05-02)


### Bug Fixes

* Cron Validation ([#4842](https://github.com/Ombi-app/Ombi/issues/4842)) ([97cc42f](https://github.com/Ombi-app/Ombi/commit/97cc42ffa8672e7d0d0996b5fbda7f7fe699da2d))
* **discover:** :children_crossing: Improved the new Genre buttons, it now includes TV results ([b087d60](https://github.com/Ombi-app/Ombi/commit/b087d606ff36565208e564f8856903f2a4098db5))
* **lidarr:** Change monitor to Existing to properly add artist [#3597](https://github.com/Ombi-app/Ombi/issues/3597) ([506f607](https://github.com/Ombi-app/Ombi/commit/506f60773bf1031d0be51ccd34289b855a04ea40)), closes [/github.com/Lidarr/Lidarr/issues/3597#issuecomment-1530804055](https://github.com//github.com/Lidarr/Lidarr/issues/3597/issues/issuecomment-1530804055)



# [4.37.0](https://github.com/Ombi-app/Ombi/compare/v4.36.1...v4.37.0) (2023-04-24)


### Features

* Search by genre ([1837419](https://github.com/Ombi-app/Ombi/commit/18374198f9f2462ba85c5781b0fcc05892728b21))



## [4.36.1](https://github.com/Ombi-app/Ombi/compare/v4.36.0...v4.36.1) (2023-04-20)


### Bug Fixes

* **healthchecks:** Removed redundant ping check ([1751305](https://github.com/Ombi-app/Ombi/commit/1751305064176d2c0135f867773ccc46b03915ec))



# [4.36.0](https://github.com/Ombi-app/Ombi/compare/v4.35.19...v4.36.0) (2023-04-20)


### Features

* **discover:** Add deny option to recently requested ([#4907](https://github.com/Ombi-app/Ombi/issues/4907)) ([78f340e](https://github.com/Ombi-app/Ombi/commit/78f340ee5f309c55690497170897533801957668))



## [4.35.19](https://github.com/Ombi-app/Ombi/compare/v4.35.18...v4.35.19) (2023-04-20)


### Bug Fixes

* **radarr:** Fixed an issue where the radarr sync would break ([de4baad](https://github.com/Ombi-app/Ombi/commit/de4baade9f87248d77106ff1a313a498870f4fb3))



## [4.35.18](https://github.com/Ombi-app/Ombi/compare/v4.35.17...v4.35.18) (2023-04-15)


### Bug Fixes

* **#4906:** :bug: Fixed an issue with power users and permissions ([80884bc](https://github.com/Ombi-app/Ombi/commit/80884bcd725c329867c278ad235cd4096cd4fe7a))



## [4.35.17](https://github.com/Ombi-app/Ombi/compare/v4.35.16...v4.35.17) (2023-04-15)


### Bug Fixes

* **discover:** Fix denied requests displayed as approved ([#4901](https://github.com/Ombi-app/Ombi/issues/4901)) ([1e87f20](https://github.com/Ombi-app/Ombi/commit/1e87f2010491b0f3fdda70d2b19d9afd94438df7))
* Fix denied movie shown as 'processing request' in details view ([#4900](https://github.com/Ombi-app/Ombi/issues/4900)) ([0069bfd](https://github.com/Ombi-app/Ombi/commit/0069bfdf54e0785bad45c832ca052f19fd4b940b))



## [4.35.16](https://github.com/Ombi-app/Ombi/compare/v4.35.15...v4.35.16) (2023-04-13)


### Bug Fixes

* Support duplicates in Emby/JF collections ([#4902](https://github.com/Ombi-app/Ombi/issues/4902)) ([141f96d](https://github.com/Ombi-app/Ombi/commit/141f96da5e45d5b3fa5f496806b102e473da6607))



## [4.35.15](https://github.com/Ombi-app/Ombi/compare/v4.35.14...v4.35.15) (2023-04-06)


### Bug Fixes

* **sonarr:** :bug: Stop the sonarr version endpoint from breaking when Sonarr is down [#4895](https://github.com/Ombi-app/Ombi/issues/4895) ([7bb8bec](https://github.com/Ombi-app/Ombi/commit/7bb8becfb140ef6012356752a71d53b5b404e482))



## [4.35.14](https://github.com/Ombi-app/Ombi/compare/v4.35.13...v4.35.14) (2023-04-06)


### Bug Fixes

* Some minor tweaks to the movie info panel ([#4883](https://github.com/Ombi-app/Ombi/issues/4883)) ([1244487](https://github.com/Ombi-app/Ombi/commit/12444871df2f7602200f73971fce962f06b4a80b))



## [4.35.13](https://github.com/Ombi-app/Ombi/compare/v4.35.12...v4.35.13) (2023-03-28)


### Bug Fixes

* **sonarr:** :bug: Added some more error handling and information around testing sonarr ([bd2c2d3](https://github.com/Ombi-app/Ombi/commit/bd2c2d3901e239393010fd582b207f1571fb4b7e)), closes [#4877](https://github.com/Ombi-app/Ombi/issues/4877)



## [4.35.12](https://github.com/Ombi-app/Ombi/compare/v4.35.10...v4.35.12) (2023-03-25)


### Bug Fixes

* **sonarr:** :bug: Improved the error handling in the sonarr settings page in the UI ([fcd78fe](https://github.com/Ombi-app/Ombi/commit/fcd78fee619d10ec7d78e8c8ec6c3ac4b0a361a1)), closes [#4877](https://github.com/Ombi-app/Ombi/issues/4877)



## [4.35.9](https://github.com/Ombi-app/Ombi/compare/v4.35.8...v4.35.9) (2023-02-24)



## [4.35.8](https://github.com/Ombi-app/Ombi/compare/v4.35.7...v4.35.8) (2023-02-17)


### Bug Fixes

* **plex-oauth:** 🐛 Fixed an issue where using OAuth you could log in as a Ombi Local user [#4835](https://github.com/Ombi-app/Ombi/issues/4835) ([4098da3](https://github.com/Ombi-app/Ombi/commit/4098da305aaea9dae9a552644268a4fed7204cfe))



## [4.35.7](https://github.com/Ombi-app/Ombi/compare/v4.35.6...v4.35.7) (2023-02-10)


### Bug Fixes

* **wizard:** :bug: Stop access to the wizard when you have already setup ombi ([#4866](https://github.com/Ombi-app/Ombi/issues/4866)) ([353de98](https://github.com/Ombi-app/Ombi/commit/353de981a462e1753288d225ec4644a44a62d2bc))



## [4.35.6](https://github.com/Ombi-app/Ombi/compare/v4.35.5...v4.35.6) (2023-01-31)


### Bug Fixes

* Fixed the issue where the login page is still present after logging in with oauth ([aca4ee3](https://github.com/Ombi-app/Ombi/commit/aca4ee37915a28200e5233be3dc711ccc4a5aee9))



## [4.35.5](https://github.com/Ombi-app/Ombi/compare/v4.35.4...v4.35.5) (2023-01-24)


### Bug Fixes

* **radarr-settings:** 🐛 Fixed a typo ([4a50a00](https://github.com/Ombi-app/Ombi/commit/4a50a00d4729d99f4359874b9af4dbc58a0c220b))



## [4.35.4](https://github.com/Ombi-app/Ombi/compare/v4.35.3...v4.35.4) (2023-01-22)


### Bug Fixes

* **discover:** :bug: Fixed the default poster not taking into account the base url in some scenarios [#4845](https://github.com/Ombi-app/Ombi/issues/4845) ([8eda250](https://github.com/Ombi-app/Ombi/commit/8eda250367953183daec03ccb5cdf9fe94275b27))
* **Hide music from navbar and request list when not enabled:** :bug: ([5123a76](https://github.com/Ombi-app/Ombi/commit/5123a76954e9f81d58c05e31afc7a29aec19cb7a))



## [4.35.3](https://github.com/Ombi-app/Ombi/compare/v4.35.2...v4.35.3) (2023-01-13)


### Bug Fixes

* **#4847:** Invalid Discord request fixed, also fixed an issue where App Only users would not show as logged in on the user management page ([#4848](https://github.com/Ombi-app/Ombi/issues/4848)) ([f229d88](https://github.com/Ombi-app/Ombi/commit/f229d88bd744bc5253b5d3db69ae5ef22d014230))



## [4.35.2](https://github.com/Ombi-app/Ombi/compare/v4.35.1...v4.35.2) (2023-01-08)


### Bug Fixes

* **database:** Just some tweaks, shouldn't notice any difference, maybe a less error in the log ([67fb992](https://github.com/Ombi-app/Ombi/commit/67fb9921c0c025025286eb6c0a9d09fd01b18465))



## [4.35.1](https://github.com/Ombi-app/Ombi/compare/v4.35.0...v4.35.1) (2023-01-06)


### Bug Fixes

* **plex-watchlist:** Index out of bounds error ([8cd556e](https://github.com/Ombi-app/Ombi/commit/8cd556e268931596b9c1d1ae0ce533bfaaf330f4))



# [4.35.0](https://github.com/Ombi-app/Ombi/compare/v4.34.1...v4.35.0) (2023-01-04)


### Features

* Add the option for header authentication to create users ([#4841](https://github.com/Ombi-app/Ombi/issues/4841)) ([e6c9ce5](https://github.com/Ombi-app/Ombi/commit/e6c9ce5ad0056608ecda8273fb8124ed292e2942))



## [4.34.1](https://github.com/Ombi-app/Ombi/compare/v4.34.0...v4.34.1) (2023-01-04)


### Bug Fixes

* **plex-watchlist:** Lookup the ID from different sources when Plex doesn't contain the metadata ([#4843](https://github.com/Ombi-app/Ombi/issues/4843)) ([a2cc23b](https://github.com/Ombi-app/Ombi/commit/a2cc23b351c4a568c44e6c855f94db9f71ad084a))



# [4.34.0](https://github.com/Ombi-app/Ombi/compare/v4.33.1...v4.34.0) (2023-01-04)


### Features

* Radarr tags ([#4815](https://github.com/Ombi-app/Ombi/issues/4815)) ([6fa5064](https://github.com/Ombi-app/Ombi/commit/6fa506491fe867cdeef9df79991ae49319d71c3d))



## [4.33.1](https://github.com/Ombi-app/Ombi/compare/v4.33.0...v4.33.1) (2022-12-22)


### Bug Fixes

* **plex:** Added the watchlist request whole show back into the settings ([10701c4](https://github.com/Ombi-app/Ombi/commit/10701c4a0b6190eebb75c5d8b18224f3d0bc8502))



# [4.33.0](https://github.com/Ombi-app/Ombi/compare/v4.32.3...v4.33.0) (2022-12-01)


### Features

* Angular 15 and Dependency upgrades ([#4818](https://github.com/Ombi-app/Ombi/issues/4818)) ([4816acf](https://github.com/Ombi-app/Ombi/commit/4816acf6f94443d23ebef6091d4cfcbca580f9ca))



## [4.32.3](https://github.com/Ombi-app/Ombi/compare/v4.32.2...v4.32.3) (2022-11-24)


### Bug Fixes

* **sonarr:** V4 actually works this time around ([f62e70f](https://github.com/Ombi-app/Ombi/commit/f62e70fc493c7971da5e4508ce10522f5df0bbf7))



## [4.32.2](https://github.com/Ombi-app/Ombi/compare/v4.32.1...v4.32.2) (2022-11-23)


### Bug Fixes

* **sonarr:** :bug: Sonarr V4 should work now ([#4810](https://github.com/Ombi-app/Ombi/issues/4810)) ([37655af](https://github.com/Ombi-app/Ombi/commit/37655aff9d3d133b42f5664bc9445d6571e966d6))



## [4.32.1](https://github.com/Ombi-app/Ombi/compare/v4.32.0...v4.32.1) (2022-11-21)


### Bug Fixes

* **plex:** :bug: Fixed the issue where you couldn't add a new server on a fresh setup after the settings page rework ([187b18d](https://github.com/Ombi-app/Ombi/commit/187b18d5c01f6a13831e4a410b5d7c349e27d847))



# [4.32.0](https://github.com/Ombi-app/Ombi/compare/v4.31.0...v4.32.0) (2022-11-18)


### Bug Fixes

* **translations:** 🌐 New translations from Crowdin [skip ci] ([#4801](https://github.com/Ombi-app/Ombi/issues/4801)) ([4692003](https://github.com/Ombi-app/Ombi/commit/46920032baed04675b2ffbe1700afdc0740a4ac4))


### Features

* **plex:** Rework the Plex Settings page ([#4805](https://github.com/Ombi-app/Ombi/issues/4805)) ([1b8c47f](https://github.com/Ombi-app/Ombi/commit/1b8c47f3163f618851d4904732cb07015e1e93ff))



# [4.31.0](https://github.com/Ombi-app/Ombi/compare/v4.30.0...v4.31.0) (2022-11-18)


### Features

* **sonarr:** Added the ability to add default tags when sending to Sonarr ([#4803](https://github.com/Ombi-app/Ombi/issues/4803)) ([ecfbb8e](https://github.com/Ombi-app/Ombi/commit/ecfbb8eda91e1a90239dcf8be847afcc2394a78e))



# [4.30.0](https://github.com/Ombi-app/Ombi/compare/v4.29.3...v4.30.0) (2022-11-17)


### Features

* **sonarr:** :sparkles: Add the username to a Sonarr tag when sent to Sonarr ([#4802](https://github.com/Ombi-app/Ombi/issues/4802)) ([1d5fabd](https://github.com/Ombi-app/Ombi/commit/1d5fabd317e3ce8f6dd31f06d15dc81277f39dbd))



## [4.29.3](https://github.com/Ombi-app/Ombi/compare/v4.29.2...v4.29.3) (2022-11-14)


### Bug Fixes

* **notifications:** Fixed the Partially TV notifications going to the admin [#4797](https://github.com/Ombi-app/Ombi/issues/4797) ([#4799](https://github.com/Ombi-app/Ombi/issues/4799)) ([bcb3e7f](https://github.com/Ombi-app/Ombi/commit/bcb3e7f00380a4c4278f59dc55febf43e6d05d47))
* Only log error messages from Microsoft ([#4787](https://github.com/Ombi-app/Ombi/issues/4787)) ([c614e0c](https://github.com/Ombi-app/Ombi/commit/c614e0ca5fe5023cbe7ced326145273cd75be85d))



## [4.29.2](https://github.com/Ombi-app/Ombi/compare/v4.29.1...v4.29.2) (2022-10-24)


### Bug Fixes

* **plex:** Fixed an issue where sometimes the availability checker would throw an exception when checking episodes ([17ba202](https://github.com/Ombi-app/Ombi/commit/17ba2020ee0950c2c0e0e03fdb7835b579da75a9))



## [4.29.1](https://github.com/Ombi-app/Ombi/compare/v4.29.0...v4.29.1) (2022-10-22)


### Bug Fixes

* Consistently reset loading flag when requesting movies on discover page. ([#4777](https://github.com/Ombi-app/Ombi/issues/4777)) ([a40ab5c](https://github.com/Ombi-app/Ombi/commit/a40ab5cddf769d4147696eca50c1610b466ab99b))
* **sonarr:** :bug: Fixed an issue where the language list didn't correctly load for power users in the advanced options [#4782](https://github.com/Ombi-app/Ombi/issues/4782) ([2173670](https://github.com/Ombi-app/Ombi/commit/217367047d1568070dd507e54ad3fd2c68f05b88))



# [4.29.0](https://github.com/Ombi-app/Ombi/compare/v4.28.1...v4.29.0) (2022-10-19)


### Bug Fixes

* Partially Available prevents further TV requests ([#4768](https://github.com/Ombi-app/Ombi/issues/4768)) ([#4779](https://github.com/Ombi-app/Ombi/issues/4779)) ([031e2b9](https://github.com/Ombi-app/Ombi/commit/031e2b9283b239827cabaca4e35f69f2f93a4d7b))
* Unable to Delete Jellyfin Server ([#4705](https://github.com/Ombi-app/Ombi/issues/4705)) ([#4780](https://github.com/Ombi-app/Ombi/issues/4780)) ([76a0d0d](https://github.com/Ombi-app/Ombi/commit/76a0d0d26893bd480fea4735f77522ac6261a425))


### Features

* Provide a flag for missing users on Plex Server ([#4688](https://github.com/Ombi-app/Ombi/issues/4688)) ([#4778](https://github.com/Ombi-app/Ombi/issues/4778)) ([b4a14c2](https://github.com/Ombi-app/Ombi/commit/b4a14c2d28218409390e517b226130e3e84efee1))



## [4.28.1](https://github.com/Ombi-app/Ombi/compare/v4.28.0...v4.28.1) (2022-10-19)


### Bug Fixes

* **plex:** :bug: Fixed not being able to enable watchlist requests in the Plex settings ([3e5158e](https://github.com/Ombi-app/Ombi/commit/3e5158ef9cda58ea2dd3be143f07aa5433691d79))
* Reworked the version check ([#4719](https://github.com/Ombi-app/Ombi/issues/4719)) ([#4781](https://github.com/Ombi-app/Ombi/issues/4781)) ([55855c5](https://github.com/Ombi-app/Ombi/commit/55855c5adda3cd1c51b7fbd0c19b469fc813f98e))



# [4.28.0](https://github.com/Ombi-app/Ombi/compare/v4.27.8...v4.28.0) (2022-10-07)


### Features

* **plex:** ✨ Added the ability to configure the watchlist to request the whole TV show rather than latest season ([#4774](https://github.com/Ombi-app/Ombi/issues/4774)) ([fa65712](https://github.com/Ombi-app/Ombi/commit/fa65712bd570fe8d5d21b8ca0abe182b84960017))



## [4.27.8](https://github.com/Ombi-app/Ombi/compare/v4.27.7...v4.27.8) (2022-10-07)



## [4.27.7](https://github.com/Ombi-app/Ombi/compare/v4.27.6...v4.27.7) (2022-10-07)


### Bug Fixes

* Fixes default image for recently requested items. ([#4767](https://github.com/Ombi-app/Ombi/issues/4767)) ([2e6f35f](https://github.com/Ombi-app/Ombi/commit/2e6f35f89abb3dd3685ec8289f8620c7ef7072cd))



## [4.27.6](https://github.com/Ombi-app/Ombi/compare/v4.27.5...v4.27.6) (2022-10-01)


### Bug Fixes

* **notifications:** Fixed the error when sending multiple test notifications. Added more logging when Discord complains the message is invalid ([fc14780](https://github.com/Ombi-app/Ombi/commit/fc14780bd354483119ddcbb55a8c382e1890a783))



## [4.27.5](https://github.com/Ombi-app/Ombi/compare/v4.27.4...v4.27.5) (2022-09-30)


### Bug Fixes

* **importer:** 🐛 Allow you to only import Plex Admins without the Plex Users ([8c9ad9b](https://github.com/Ombi-app/Ombi/commit/8c9ad9b414fdc6c88bdb911d6057ae5d38783b98))



## [4.27.4](https://github.com/Ombi-app/Ombi/compare/v4.27.3...v4.27.4) (2022-09-30)



## [4.27.3](https://github.com/Ombi-app/Ombi/compare/v4.27.2...v4.27.3) (2022-09-30)


### Bug Fixes

* **availability:** 🐛 Fixed a issue with the availability checker after the previous update. Added full test coverage around that area ([28e2480](https://github.com/Ombi-app/Ombi/commit/28e248046ad56390595f84172bbd5f5961325b4d))



## [4.27.2](https://github.com/Ombi-app/Ombi/compare/v4.27.1...v4.27.2) (2022-09-29)


### Bug Fixes

* **sonarr:** :bug: Cleaned up and removed Sonarr v3 option, sonarr v3 is now the default. This allows us to get ready for the upcoming Sonarr v4 ([#4764](https://github.com/Ombi-app/Ombi/issues/4764)) ([2cddec7](https://github.com/Ombi-app/Ombi/commit/2cddec759004b6490f686ff74cb092238e3dc946))



## [4.27.1](https://github.com/Ombi-app/Ombi/compare/v4.27.0...v4.27.1) (2022-09-20)


### Bug Fixes

* **plex:** stop the plex sync from deleting episodes when we can't find the plex key ([66b05e5](https://github.com/Ombi-app/Ombi/commit/66b05e5a85dbfe1fec5f9366e80987f2cfa1f4fe))



# [4.27.0](https://github.com/Ombi-app/Ombi/compare/v4.26.0...v4.27.0) (2022-09-14)


### Features

* Recently requested improvements ([#4755](https://github.com/Ombi-app/Ombi/issues/4755)) ([ff04d87](https://github.com/Ombi-app/Ombi/commit/ff04d875343604c77c391bf55d0968977e480281))



# [4.26.0](https://github.com/Ombi-app/Ombi/compare/v4.25.1...v4.26.0) (2022-09-07)


### Features

* **notifications:** Add more curly variables for partially available notification ([66aa101](https://github.com/Ombi-app/Ombi/commit/66aa101019c4c4b34e186db9d303049d02b9c781))



## [4.25.1](https://github.com/Ombi-app/Ombi/compare/v4.25.0...v4.25.1) (2022-09-07)


### Bug Fixes

* **webhook:** Remove added trailing slash from webhook URL [#4710](https://github.com/Ombi-app/Ombi/issues/4710) ([369eb33](https://github.com/Ombi-app/Ombi/commit/369eb339171671101be219486e2aab27a20f3d74))



# [4.25.0](https://github.com/Ombi-app/Ombi/compare/v4.24.0...v4.25.0) (2022-08-23)


### Bug Fixes

* fixed stats controller ([#4742](https://github.com/Ombi-app/Ombi/issues/4742)) ([47ea64b](https://github.com/Ombi-app/Ombi/commit/47ea64b5a401770f1943b575ca40f84d515e96b3))


### Features

* Watchlist history errors([#4741](https://github.com/Ombi-app/Ombi/issues/4741)) ([c222f1a](https://github.com/Ombi-app/Ombi/commit/c222f1a945e944ef34e68cad2b61f40e57cab823))



# [4.24.0](https://github.com/Ombi-app/Ombi/compare/v4.23.2...v4.24.0) (2022-08-22)


### Features

* add crew on movie page ([#4722](https://github.com/Ombi-app/Ombi/issues/4722)) ([1d53261](https://github.com/Ombi-app/Ombi/commit/1d532613823804b25984bd1d223d081a54ad143d))



## [4.23.2](https://github.com/Ombi-app/Ombi/compare/v4.23.1...v4.23.2) (2022-08-22)


### Bug Fixes

* Fix conflicting property name for Swagger ([#4733](https://github.com/Ombi-app/Ombi/issues/4733)) ([d661f32](https://github.com/Ombi-app/Ombi/commit/d661f32e8a9e105faab6380b4b7b642896b98163))



## [4.23.1](https://github.com/Ombi-app/Ombi/compare/v4.23.0...v4.23.1) (2022-08-12)


### Bug Fixes

* Localize recently requested on discover page ([#4729](https://github.com/Ombi-app/Ombi/issues/4729)) ([bf65c76](https://github.com/Ombi-app/Ombi/commit/bf65c76ff9ce38f65a9e5feb872734e8d8e35eb6))



# [4.23.0](https://github.com/Ombi-app/Ombi/compare/v4.22.5...v4.23.0) (2022-08-09)


### Bug Fixes

* Log Microsoft warnings to log file ([#4723](https://github.com/Ombi-app/Ombi/issues/4723)) ([26ac75f](https://github.com/Ombi-app/Ombi/commit/26ac75f0c223c2a91e3471797ae46ede3fde89cc))


### Features

* ✨ Recently Requested on Discover Page ([#4387](https://github.com/Ombi-app/Ombi/issues/4387)) ([44d38fb](https://github.com/Ombi-app/Ombi/commit/44d38fbaae521dbb467b61c7471b2384015ac52e))



## [4.22.4](https://github.com/Ombi-app/Ombi/compare/v4.22.3...v4.22.4) (2022-08-04)


### Bug Fixes

* :bug: Fixed missing externals ([#4712](https://github.com/Ombi-app/Ombi/issues/4712)) ([fcc1eaa](https://github.com/Ombi-app/Ombi/commit/fcc1eaaa377683dcdc81d62a2a688fb0c4490c7b))
* fixed trakt image not loading when base url present ([#4711](https://github.com/Ombi-app/Ombi/issues/4711)) ([f102dcf](https://github.com/Ombi-app/Ombi/commit/f102dcf751c2eb62ebfe30f9f8e4b2ad863c3b0d))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([#4713](https://github.com/Ombi-app/Ombi/issues/4713)) ([ff142b0](https://github.com/Ombi-app/Ombi/commit/ff142b09abbb2f9540387284222552e6e12639fe))



## [4.22.3](https://github.com/Ombi-app/Ombi/compare/v4.22.2...v4.22.3) (2022-07-28)


### Bug Fixes

* Override Sonarr V3 Profiles endpoint ([#4678](https://github.com/Ombi-app/Ombi/issues/4678)) ([875da95](https://github.com/Ombi-app/Ombi/commit/875da959f353119b05138d68ee6d32a49e14b91e))



## [4.22.2](https://github.com/Ombi-app/Ombi/compare/v4.22.1...v4.22.2) (2022-07-25)


### Bug Fixes

* fixed an issue where I broke images for some users ([81ddc85](https://github.com/Ombi-app/Ombi/commit/81ddc8553b9094c3f6843b036daebb2eb9262e00))



## [4.22.1](https://github.com/Ombi-app/Ombi/compare/v4.22.0...v4.22.1) (2022-07-25)


### Bug Fixes

* **discover:** :bug: Created new Image component to handle 429's from TMDB ([#4698](https://github.com/Ombi-app/Ombi/issues/4698)) and fixed [#4635](https://github.com/Ombi-app/Ombi/issues/4635) ([#4699](https://github.com/Ombi-app/Ombi/issues/4699)) ([f22d3da](https://github.com/Ombi-app/Ombi/commit/f22d3da765799365455b919027f7563e52b347c3))



# [4.22.0](https://github.com/Ombi-app/Ombi/compare/v4.21.2...v4.22.0) (2022-07-22)


### Features

* **discover:** ✨ Added infinite scroll on advanced search results ([898bc89](https://github.com/Ombi-app/Ombi/commit/898bc89fa78245c1f3de9481f6c724f087a16e39))



## [4.21.2](https://github.com/Ombi-app/Ombi/compare/v4.21.1...v4.21.2) (2022-07-22)


### Bug Fixes

* Landing and Login page improvements ([#4690](https://github.com/Ombi-app/Ombi/issues/4690)) ([6d423b5](https://github.com/Ombi-app/Ombi/commit/6d423b5447c52c5e59d8d2bd92a23b47468eb736))



## [4.21.1](https://github.com/Ombi-app/Ombi/compare/v4.21.0...v4.21.1) (2022-07-11)


### Bug Fixes

* **images:** Retry images with a backoff when we get a Too Many requests from TheMovieDb [#4685](https://github.com/Ombi-app/Ombi/issues/4685) ([3f1f35d](https://github.com/Ombi-app/Ombi/commit/3f1f35df3164db6739691cdda8f925c296239791))



# [4.21.0](https://github.com/Ombi-app/Ombi/compare/v4.20.4...v4.21.0) (2022-06-22)


### Features

* Upgrade to Angular14 ([#4668](https://github.com/Ombi-app/Ombi/issues/4668)) ([b9d55a4](https://github.com/Ombi-app/Ombi/commit/b9d55a469b412558cbf67c1e25db7fdda5964cd8))


### Performance Improvements

* stop populating obsolete subscribe fields ([#4625](https://github.com/Ombi-app/Ombi/issues/4625)) ([9a73463](https://github.com/Ombi-app/Ombi/commit/9a734637665f671b17c2bb440d93b35a891c142b))



## [4.20.4](https://github.com/Ombi-app/Ombi/compare/v4.20.3...v4.20.4) (2022-06-15)


### Bug Fixes

* fixed build ([f877921](https://github.com/Ombi-app/Ombi/commit/f8779219146051ea74f8b6408658ff7975afb88b))



## [4.20.3](https://github.com/Ombi-app/Ombi/compare/v4.20.2...v4.20.3) (2022-06-05)


### Bug Fixes

* **plex:** 🐛 Fixed an issue with the Plex Sync ([ab1a11a](https://github.com/Ombi-app/Ombi/commit/ab1a11af78efbe9d37bd55aa80a640796c138a98))



## [4.20.2](https://github.com/Ombi-app/Ombi/compare/v4.20.1...v4.20.2) (2022-06-03)


### Bug Fixes

* :bug: Fixed the Request on Behalf of having blanks ([#4667](https://github.com/Ombi-app/Ombi/issues/4667)) ([7dd9b1c](https://github.com/Ombi-app/Ombi/commit/7dd9b1cac07f571dd35b362544e4fe0226c4b817))



## [4.20.1](https://github.com/Ombi-app/Ombi/compare/v4.20.0...v4.20.1) (2022-05-27)


### Bug Fixes

* added media type tag to media type text ([#4638](https://github.com/Ombi-app/Ombi/issues/4638)) ([fe501d3](https://github.com/Ombi-app/Ombi/commit/fe501d34a0c36ac9f000b107eca49dbc6694d006))
* **API:** Fix pagination in some edge cases ([#4649](https://github.com/Ombi-app/Ombi/issues/4649)) ([a70bf8f](https://github.com/Ombi-app/Ombi/commit/a70bf8f46c76d74c9dfdf908c53bd9955ca0a35d))
* **discover:** Carousel touch not working when scrolling page and recommendations and similar movie navigation ([#4633](https://github.com/Ombi-app/Ombi/issues/4633)) ([d5ef1d5](https://github.com/Ombi-app/Ombi/commit/d5ef1d53e5f77d19dba8b8059c80b538a3e14f2a))
* Improve Swagger documentation ([#4652](https://github.com/Ombi-app/Ombi/issues/4652)) ([181892b](https://github.com/Ombi-app/Ombi/commit/181892bcfe88e6d76febf49ef57745d04552d08e))
* Missing Poster broken link fix ([#4637](https://github.com/Ombi-app/Ombi/issues/4637)) ([4070f0d](https://github.com/Ombi-app/Ombi/commit/4070f0d093b1c92487a1c80cabad8283a9650f51))
* **sickrage:** Fixed issue with incorrect handling of SiCKRAGE episode results returned during episode status changes, now expects array of objects from data path if present ([#4648](https://github.com/Ombi-app/Ombi/issues/4648)) ([6d16442](https://github.com/Ombi-app/Ombi/commit/6d16442d4d714920367df065a3ced42b729f4233))
* **sync:** Emby+Jellyfin - sync multi-episode files of 3+ episodes ([bd8fd89](https://github.com/Ombi-app/Ombi/commit/bd8fd890554c9d85d6da4d2cee813e82ce698e52))



# [4.20.0](https://github.com/Ombi-app/Ombi/compare/v4.19.1...v4.20.0) (2022-04-28)


### Features

* **discover:** Show more relevant shows in upcoming TV ([8357819](https://github.com/Ombi-app/Ombi/commit/8357819b53b8c675c0b246d7006b5a778bdba33f))



## [4.19.1](https://github.com/Ombi-app/Ombi/compare/v4.19.0...v4.19.1) (2022-04-27)



# [4.19.0](https://github.com/Ombi-app/Ombi/compare/v4.18.0...v4.19.0) (2022-04-27)


### Features

* **sync:** Detect reidentified movies in Emby and Jellyfin ([5938077](https://github.com/Ombi-app/Ombi/commit/5938077d82a5357f79c07b218b3986557a5816e8))
* **sync:** Detect reidentified series in Emby and Jellyfin ([9096e91](https://github.com/Ombi-app/Ombi/commit/9096e91d55d268819bce22831f8a8b27f2a1776b))



# [4.18.0](https://github.com/Ombi-app/Ombi/compare/v4.17.0...v4.18.0) (2022-04-26)


### Bug Fixes

* **discover:** Fix cache mix up ([03d9422](https://github.com/Ombi-app/Ombi/commit/03d94220c7eaafb50c6c80a6ed1150794b873ac3))
* **discover:** Fix new trending feature detection ([6794b88](https://github.com/Ombi-app/Ombi/commit/6794b887f6544fb41528bdd9728b7824b65e47ee))
* **settings:** Allow toggling features when there are more than one ([a373359](https://github.com/Ombi-app/Ombi/commit/a373359ae8e6bad42b558a6e01a8ff2840d3bbaa))


### Features

* **discover:** Add new trending source experimental feature ([1a0823c](https://github.com/Ombi-app/Ombi/commit/1a0823ca80559417c67323aaeaa1ef5243e98031))
* **discover:** Default trending source to new logic ([4f12939](https://github.com/Ombi-app/Ombi/commit/4f12939e22020a67a5ee75e2907923faea136e8d))



# [4.17.0](https://github.com/Ombi-app/Ombi/compare/v4.16.17...v4.17.0) (2022-04-25)



## [4.16.17](https://github.com/Ombi-app/Ombi/compare/v4.16.16...v4.16.17) (2022-04-25)



## [4.16.16](https://github.com/Ombi-app/Ombi/compare/v4.16.15...v4.16.16) (2022-04-25)


### Bug Fixes

* **4616:** :bug: fixed mandatory fields ([d8f2260](https://github.com/Ombi-app/Ombi/commit/d8f2260c7ae3ed48386743b7adbd06e284487034))



## [4.16.15](https://github.com/Ombi-app/Ombi/compare/v4.16.14...v4.16.15) (2022-04-24)


### Features

* **discover:** Add original language filter ([ef7ec86](https://github.com/Ombi-app/Ombi/commit/ef7ec861d8aede2a4817752c990617f583805391))



## [4.16.14](https://github.com/Ombi-app/Ombi/compare/v4.16.13...v4.16.14) (2022-04-19)



## [4.16.13](https://github.com/Ombi-app/Ombi/compare/v4.16.12...v4.16.13) (2022-04-19)



## [4.43.5](https://github.com/Ombi-app/Ombi/compare/v4.43.4...v4.43.5) (2023-08-24)



## [4.43.4](https://github.com/Ombi-app/Ombi/compare/v4.43.3...v4.43.4) (2023-07-28)


### Bug Fixes

* **user-importer:** Fixed not importing all correct users [#4989](https://github.com/Ombi-app/Ombi/issues/4989) ([34c32f8](https://github.com/Ombi-app/Ombi/commit/34c32f8338705ea3f790d95b91c9ada21a41b9f2))



## [4.43.3](https://github.com/Ombi-app/Ombi/compare/v4.43.2...v4.43.3) (2023-07-28)


### Bug Fixes

* switch back to the old plex friends API [#4989](https://github.com/Ombi-app/Ombi/issues/4989) ([c8ad12e](https://github.com/Ombi-app/Ombi/commit/c8ad12eb5f53889609d1793ae907afd33ba6ef38))



## [4.43.2](https://github.com/Ombi-app/Ombi/compare/v4.43.1...v4.43.2) (2023-07-19)


### Bug Fixes

* **plex-api:** Switch over to the new API to avoid deprecation & save… ([#4986](https://github.com/Ombi-app/Ombi/issues/4986)) ([2f2d35e](https://github.com/Ombi-app/Ombi/commit/2f2d35ec867a8e5488e368db294bd37bcf92d843))
* Remove old trending source ([#4987](https://github.com/Ombi-app/Ombi/issues/4987)) ([aacaa3e](https://github.com/Ombi-app/Ombi/commit/aacaa3e140b43f5d196da612f785cc4451717752))



## [4.43.1](https://github.com/Ombi-app/Ombi/compare/v4.43.0...v4.43.1) (2023-07-16)


### Bug Fixes

* **user-importer:** don't delete admins in the cleanup ([895b9bf](https://github.com/Ombi-app/Ombi/commit/895b9bf6a060a678d4b0cca8083aa96c38e47b95))



# [4.43.0](https://github.com/Ombi-app/Ombi/compare/v4.42.3...v4.43.0) (2023-07-14)


### Features

* Add Auto Approve 4K role ([#4982](https://github.com/Ombi-app/Ombi/issues/4982)) ([#4983](https://github.com/Ombi-app/Ombi/issues/4983)) ([ac05495](https://github.com/Ombi-app/Ombi/commit/ac054954254b9d77a42e057f1065570c7fdc1093)), closes [#4957](https://github.com/Ombi-app/Ombi/issues/4957)



## [4.42.3](https://github.com/Ombi-app/Ombi/compare/v4.42.2...v4.42.3) (2023-07-13)


### Bug Fixes

* **user-importer:** Do not delete the Plex Admin as part of the user Importer cleanup [#4870](https://github.com/Ombi-app/Ombi/issues/4870) ([#4981](https://github.com/Ombi-app/Ombi/issues/4981)) ([4e80e7b](https://github.com/Ombi-app/Ombi/commit/4e80e7b7c3239a46a645ab6d1054993734ad4dd6))



## [4.42.2](https://github.com/Ombi-app/Ombi/compare/v4.42.1...v4.42.2) (2023-07-03)


### Bug Fixes

* Remove Angular TSLint ([#4973](https://github.com/Ombi-app/Ombi/issues/4973)) ([93969b5](https://github.com/Ombi-app/Ombi/commit/93969b5a2d82f442299bee418fae43cb590d7743))
* upgrade jquery from 3.6.1 to 3.7.0 ([#4974](https://github.com/Ombi-app/Ombi/issues/4974)) ([f2552ef](https://github.com/Ombi-app/Ombi/commit/f2552ef6ede011080a8d5499e11930c4d41d04c2))
* upgrade multiple dependencies with Snyk ([#4961](https://github.com/Ombi-app/Ombi/issues/4961)) ([3c3edf6](https://github.com/Ombi-app/Ombi/commit/3c3edf6273fa98c420989ebcebfee52b2545e402))
* upgrade zone.js from 0.11.8 to 0.13.0 ([#4975](https://github.com/Ombi-app/Ombi/issues/4975)) ([37f6564](https://github.com/Ombi-app/Ombi/commit/37f65648a2f8742020b0954acec4168aee048942))



## [4.42.1](https://github.com/Ombi-app/Ombi/compare/v4.42.0...v4.42.1) (2023-06-20)


### Bug Fixes

* More automation tests mainly around the Plex Settings page ([#4821](https://github.com/Ombi-app/Ombi/issues/4821)) ([21bfc5a](https://github.com/Ombi-app/Ombi/commit/21bfc5a45adf6da6a80854e19494a8ffdc9c0761))
* src/Ombi.Notifications/Ombi.Notifications.csproj to reduce vulnerabilities ([#4969](https://github.com/Ombi-app/Ombi/issues/4969)) [skip ci] ([8584ad4](https://github.com/Ombi-app/Ombi/commit/8584ad46053c51f5da40b24f3efd1b9e5a031ddd))
* upgrade @fortawesome/fontawesome-free from 6.1.2 to 6.4.0 ([#4965](https://github.com/Ombi-app/Ombi/issues/4965)) [skip ci] ([84454e5](https://github.com/Ombi-app/Ombi/commit/84454e53c00c808e8a393c7750bdc418a7593e91))
* upgrade @microsoft/signalr from 6.0.11 to 6.0.16 ([#4964](https://github.com/Ombi-app/Ombi/issues/4964)) [skip ci] ([a0201e3](https://github.com/Ombi-app/Ombi/commit/a0201e3f585dc52f717e33c46ede35a4eccac736))
* upgrade cypress-real-events from 1.7.4 to 1.8.1 ([#4968](https://github.com/Ombi-app/Ombi/issues/4968)) [skip ci] ([8a24b56](https://github.com/Ombi-app/Ombi/commit/8a24b56299c3bc98bf0d719ba448972aaa7f7461))
* upgrade multiple dependencies with Snyk ([#4963](https://github.com/Ombi-app/Ombi/issues/4963)) [skip ci] ([6025c5e](https://github.com/Ombi-app/Ombi/commit/6025c5ed757438d3a5d79bd36fd789ef0297ce70))
* upgrade primeng from 15.0.0-rc.1 to 15.4.1 ([#4962](https://github.com/Ombi-app/Ombi/issues/4962)) [skip ci] ([23a4fed](https://github.com/Ombi-app/Ombi/commit/23a4fede69898a25b342aed78a8cda553c1fd18d))



# [4.42.0](https://github.com/Ombi-app/Ombi/compare/v4.41.1...v4.42.0) (2023-06-02)


### Bug Fixes

* **translations:** 🌐 New translations from Crowdin [skip ci] ([#4926](https://github.com/Ombi-app/Ombi/issues/4926)) ([151efe1](https://github.com/Ombi-app/Ombi/commit/151efe19d0012b85f317c175da5ab4802ea14e20))


### Features

* **emby:** Show watched status for TV requests ([1f37de0](https://github.com/Ombi-app/Ombi/commit/1f37de08888812b6d130d92bb664a89e89149105))



## [4.41.1](https://github.com/Ombi-app/Ombi/compare/v4.41.0...v4.41.1) (2023-05-27)



# [4.41.0](https://github.com/Ombi-app/Ombi/compare/v4.40.0...v4.41.0) (2023-05-27)


### Bug Fixes

* Fix various styling issues ([#4935](https://github.com/Ombi-app/Ombi/issues/4935)) ([90b934a](https://github.com/Ombi-app/Ombi/commit/90b934a36996c0f489096f3641350a1c0d3b7c89))


### Features

* **emby:** Show end-user external IP address to Emby when logging in as an Emby user ([#4949](https://github.com/Ombi-app/Ombi/issues/4949)) ([79cef7e](https://github.com/Ombi-app/Ombi/commit/79cef7e0f8643e36536a9ea84dd1a07c232403a9)), closes [#4947](https://github.com/Ombi-app/Ombi/issues/4947)



