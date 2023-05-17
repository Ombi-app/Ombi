## [4.38.2](https://github.com/Ombi-app/Ombi/compare/v4.38.1...v4.38.2) (2023-05-17)



## [4.38.1](https://github.com/Ombi-app/Ombi/compare/v4.38.0...v4.38.1) (2023-05-09)


### Bug Fixes

* **API:** Allow RequestOnBehalf rights if requested from the API ([#4919](https://github.com/Ombi-app/Ombi/issues/4919)) ([bb6dedd](https://github.com/Ombi-app/Ombi/commit/bb6deddfaecb3d6c7c3c6970414444b619bb9106))
* **notificaitons:** Add the RequestedByAlias field to the Notification Message ([7e9c8be](https://github.com/Ombi-app/Ombi/commit/7e9c8bec6b02bb4e11f8db50394e493d4dd07723))



# [4.38.0](https://github.com/Ombi-app/Ombi/compare/v4.37.3...v4.38.0) (2023-05-07)


### Bug Fixes

* remove sort header ([969bc7b](https://github.com/Ombi-app/Ombi/commit/969bc7bb25ea900ab9199509b079b36843e5bd6f))


### Features

* **emby:** Show watched status for Movie requests ([9cfb10b](https://github.com/Ombi-app/Ombi/commit/9cfb10bb1ee69067a6f47bd2c8a72d4e6834350e))



## [4.37.3](https://github.com/Ombi-app/Ombi/compare/v4.37.2...v4.37.3) (2023-05-07)


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



## [4.35.12](https://github.com/Ombi-app/Ombi/compare/v4.35.9...v4.35.12) (2023-03-25)


### Bug Fixes

* **sonarr:** :bug: Improved the error handling in the sonarr settings page in the UI ([fcd78fe](https://github.com/Ombi-app/Ombi/commit/fcd78fee619d10ec7d78e8c8ec6c3ac4b0a361a1)), closes [#4877](https://github.com/Ombi-app/Ombi/issues/4877)



## [4.35.9](https://github.com/Ombi-app/Ombi/compare/v4.35.8...v4.35.9) (2023-02-24)



## [4.22.5](https://github.com/Ombi-app/Ombi/compare/v4.22.4...v4.22.5) (2022-08-05)



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



