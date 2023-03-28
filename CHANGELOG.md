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



