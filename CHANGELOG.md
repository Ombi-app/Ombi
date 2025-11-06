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

* **radarr-4k:** :bug: Fixed an issue where the overrides wouldn't work for 4k Requests ([0fb29a0](https://github.com/Ombi-app/Ombi/commit/0fb29a0b16b1fc87f71df1a589f6141324cf2f1b))



## [4.46.2](https://github.com/Ombi-app/Ombi/compare/v4.46.1...v4.46.2) (2024-09-03)


### Bug Fixes

* **radarr:** :bug: Enable validation on the radarr settings page ([0af3511](https://github.com/Ombi-app/Ombi/commit/0af3511e819d24e0f4edf6f33931e61bba743224))



## [4.46.1](https://github.com/Ombi-app/Ombi/compare/v4.46.0...v4.46.1) (2024-08-27)


### Bug Fixes

* src/Ombi.Notifications/Ombi.Notifications.csproj to reduce vulnerabilities ([#5167](https://github.com/Ombi-app/Ombi/issues/5167)) ([e1f2a84](https://github.com/Ombi-app/Ombi/commit/e1f2a848065d79c8bba9eafff4f1f5db4a994b53))



