## [4.16.7](https://github.com/Ombi-app/Ombi/compare/v4.16.6...v4.16.7) (2022-04-12)



## [4.16.6](https://github.com/Ombi-app/Ombi/compare/v4.16.5...v4.16.6) (2022-04-11)



## [4.16.5](https://github.com/Ombi-app/Ombi/compare/v4.16.4...v4.16.5) (2022-04-08)


### Bug Fixes

* **watchlist:** actually fixed it this time... ([d962a32](https://github.com/Ombi-app/Ombi/commit/d962a3211eca29520662ddce962676e3aea17ec5))



## [4.16.4](https://github.com/Ombi-app/Ombi/compare/v4.16.3...v4.16.4) (2022-04-08)



## [4.16.3](https://github.com/Ombi-app/Ombi/compare/v4.16.2...v4.16.3) (2022-04-08)


### Bug Fixes

* **plex-watchlist:** :bug: Fixed the issue where the watchlist didn't work for users logging in via OAuth ([6398f6a](https://github.com/Ombi-app/Ombi/commit/6398f6a4f7755281ebeac537e3ff623df5cfa0f3))



## [4.16.2](https://github.com/Ombi-app/Ombi/compare/v4.16.1...v4.16.2) (2022-04-07)


### Bug Fixes

* **wizard:** Fixed an issue when using Plex OAuth it could fail setting up ([b743cf4](https://github.com/Ombi-app/Ombi/commit/b743cf4fafa7341ad1b163276f006d7ab0e9dcff))



## [4.16.1](https://github.com/Ombi-app/Ombi/compare/v4.16.0...v4.16.1) (2022-04-07)



# [4.16.0](https://github.com/Ombi-app/Ombi/compare/v4.15.6...v4.16.0) (2022-04-07)



## [4.15.6](https://github.com/Ombi-app/Ombi/compare/v4.15.5...v4.15.6) (2022-04-07)


### Bug Fixes

* **radarr:** Fixed an issue where we couldn't sync radarr content [#4577](https://github.com/Ombi-app/Ombi/issues/4577) ([a5355a3](https://github.com/Ombi-app/Ombi/commit/a5355a3023e6900c4dd1b0da4722d7596c03907f))



## [4.15.5](https://github.com/Ombi-app/Ombi/compare/v4.15.4...v4.15.5) (2022-04-06)



## [4.15.4](https://github.com/Ombi-app/Ombi/compare/v4.15.3...v4.15.4) (2022-03-29)



## [4.15.3](https://github.com/Ombi-app/Ombi/compare/v4.15.2...v4.15.3) (2022-03-24)



## [4.15.2](https://github.com/Ombi-app/Ombi/compare/v4.15.1...v4.15.2) (2022-03-23)


### Bug Fixes

* **metadata:** improved the metadata job to also lookup the media in Plex to see if it has any more uptodate metadata ([83d1a15](https://github.com/Ombi-app/Ombi/commit/83d1a15cc9d0ee91be73bd91c4672cf1bcf2728a))



## [4.15.1](https://github.com/Ombi-app/Ombi/compare/v4.15.0...v4.15.1) (2022-03-18)


### Bug Fixes

* **mediaserver:** fixed an issue where we were not detecting available content correctly [#4542](https://github.com/Ombi-app/Ombi/issues/4542) ([9cdd6f4](https://github.com/Ombi-app/Ombi/commit/9cdd6f41cdab8825a984905c089611409c53c753))



# [4.15.0](https://github.com/Ombi-app/Ombi/compare/v4.14.4...v4.15.0) (2022-03-17)


### Bug Fixes

* **jellyfin:** :bug: Fixed an issue where Jellyfin content was showing the Play on Emby button ([18b167d](https://github.com/Ombi-app/Ombi/commit/18b167d16a3d682b5060ee36dedbbb069bef09de)), closes [#4542](https://github.com/Ombi-app/Ombi/issues/4542)



## [4.14.4](https://github.com/Ombi-app/Ombi/compare/v4.14.3...v4.14.4) (2022-03-10)


### Bug Fixes

* :bug: Fixed the Request On Behalf autocomplete not filtering correctly ([a8ba2f3](https://github.com/Ombi-app/Ombi/commit/a8ba2f3544a1c01c57f217c4036a277ab0e67a09)), closes [#4539](https://github.com/Ombi-app/Ombi/issues/4539)
* **translations:** 🌐 New translations from Crowdin [skip ci] ([356c742](https://github.com/Ombi-app/Ombi/commit/356c7424e0ce8c1c5063b04bc6ed9b809f214d65))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([6fcaecf](https://github.com/Ombi-app/Ombi/commit/6fcaecf80b766f2d43ac7082d74364238e1638b7))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([132f4d4](https://github.com/Ombi-app/Ombi/commit/132f4d4e609b7fb7e37f38ee2f395926e2911abe))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([f292006](https://github.com/Ombi-app/Ombi/commit/f292006a08894a8d0ba899c8c6e9fe863e558dda))



## [4.14.3](https://github.com/Ombi-app/Ombi/compare/v4.14.2...v4.14.3) (2022-03-06)


### Bug Fixes

* **availability:** :bug: Fixed an issue where with 4k content, we could repeat notifications ([f9ebc1c](https://github.com/Ombi-app/Ombi/commit/f9ebc1cc2e13c7cd335121cd86295b10eda529ba))



## [4.14.2](https://github.com/Ombi-app/Ombi/compare/v4.14.1...v4.14.2) (2022-03-05)


### Bug Fixes

* **Sonarr:** :bug: Fixed an issue where some seasons were not being monitored correctly in sonarr ([60cfd41](https://github.com/Ombi-app/Ombi/commit/60cfd41f68e9006555c1a419dcff1aaa24b3e09f)), closes [#4506](https://github.com/Ombi-app/Ombi/issues/4506)



## [4.14.1](https://github.com/Ombi-app/Ombi/compare/v4.14.0...v4.14.1) (2022-03-03)



# [4.14.0](https://github.com/Ombi-app/Ombi/compare/v4.13.2...v4.14.0) (2022-03-02)



## [4.13.2](https://github.com/Ombi-app/Ombi/compare/v4.13.1...v4.13.2) (2022-03-01)


### Bug Fixes

* **requests:** :bug: Fixed an issue where you couldn't approve movies from the request list ([1611ef9](https://github.com/Ombi-app/Ombi/commit/1611ef9198befbb7a4db50a4f0953e50f29a788f))



## [4.13.1](https://github.com/Ombi-app/Ombi/compare/v4.13.0...v4.13.1) (2022-03-01)


### Bug Fixes

* **details:** :bug: Fixed the missing Play on Media server button for 4k content [#4529](https://github.com/Ombi-app/Ombi/issues/4529) ([68600f3](https://github.com/Ombi-app/Ombi/commit/68600f3b45376e12dd2ef263d81ca4040c84cbca))
* **discover:** :bug: Fixed the issue where there was an option on the discover to request 4k shows (that's not supported currently) ([dcfd688](https://github.com/Ombi-app/Ombi/commit/dcfd688c8d2337e55fa9c6c33b7c3e80fc560cda))
* **requests:** :bug: Fixed the issue where we could no longer approve TV Requests from the requests list ([19fe4e3](https://github.com/Ombi-app/Ombi/commit/19fe4e342efe5578c26ab8ba7ee2f2e64bbc9418))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([#4526](https://github.com/Ombi-app/Ombi/issues/4526)) ([7e9f54f](https://github.com/Ombi-app/Ombi/commit/7e9f54fc80a09c938184e6be40ce5f49ce9673ef))



# [4.13.0](https://github.com/Ombi-app/Ombi/compare/v4.12.7...v4.13.0) (2022-02-25)


### Bug Fixes

* **4k:** Hide 'Has 4K Request' column list if 4k feature is disabled ([#4521](https://github.com/Ombi-app/Ombi/issues/4521)) ([a9a6067](https://github.com/Ombi-app/Ombi/commit/a9a60678e74d22fa7ba34051a2645db86b600b4a))
* **issues:** Fix label ID in chatbox page ([#4520](https://github.com/Ombi-app/Ombi/issues/4520)) ([76882ad](https://github.com/Ombi-app/Ombi/commit/76882adf231f92e1cdd396239933c13467c112b3))
* **localisation:** Localize request types in notifications ([#4516](https://github.com/Ombi-app/Ombi/issues/4516)) ([e09435d](https://github.com/Ombi-app/Ombi/commit/e09435da455b12fc429f129372de31e0654da797))
* **notifications:** Remove generic admin email in favour of admins' email ([#4519](https://github.com/Ombi-app/Ombi/issues/4519)) ([b90fc5f](https://github.com/Ombi-app/Ombi/commit/b90fc5fea771a83e6cf576c71a307066efd59ea4))
* **tv:** Display TV show as requested if all episodes are requested ([#4518](https://github.com/Ombi-app/Ombi/issues/4518)) ([2ed8c48](https://github.com/Ombi-app/Ombi/commit/2ed8c48d128a69f0d144c5d332286dbf3b0bdf28))


### Features

* **email-notifications:** Add a link to Ombi details page in email notifications ([#4517](https://github.com/Ombi-app/Ombi/issues/4517)) ([a3e97b3](https://github.com/Ombi-app/Ombi/commit/a3e97b31e2298d95e7deebd71268095b8ed5e9dc))
* **media-details:** Add Trakt to social icons ([#4522](https://github.com/Ombi-app/Ombi/issues/4522)) ([d6ae79c](https://github.com/Ombi-app/Ombi/commit/d6ae79ce9eddbd5b7b888ab1b9f7e342d9d9ff9e))



## [4.12.7](https://github.com/Ombi-app/Ombi/compare/v4.12.6...v4.12.7) (2022-02-23)



## [4.12.6](https://github.com/Ombi-app/Ombi/compare/v4.12.5...v4.12.6) (2022-02-22)


### Bug Fixes

* **emby/jellyfin:** :bug: Fixed another issue where we were not correctly displaying the correct status' for movies ([5c0556e](https://github.com/Ombi-app/Ombi/commit/5c0556e6f44b8997a611f3a4d8e9e4e05d08bd13))
* **mediaserver:** fixed some more issues in the media server sync and availability checks ([f3ea979](https://github.com/Ombi-app/Ombi/commit/f3ea979b8bd77842780ce8e6928b16237dd779cf))



## [4.12.5](https://github.com/Ombi-app/Ombi/compare/v4.12.4...v4.12.5) (2022-02-21)


### Bug Fixes

* **emby:** :bug: Fixed the emby content sync [#4513](https://github.com/Ombi-app/Ombi/issues/4513) ([2927504](https://github.com/Ombi-app/Ombi/commit/2927504f0e0b4e7251e69b44e0e30c7ec9519980))
* **emby:** :bug: Fixed the emby content sync [#4513](https://github.com/Ombi-app/Ombi/issues/4513) ([bd441cb](https://github.com/Ombi-app/Ombi/commit/bd441cb54fd77d6befb03fae321dc36c29f0de2e))



## [4.12.4](https://github.com/Ombi-app/Ombi/compare/v4.12.3...v4.12.4) (2022-02-17)



## [4.12.3](https://github.com/Ombi-app/Ombi/compare/v4.12.2...v4.12.3) (2022-02-16)



## [4.12.2](https://github.com/Ombi-app/Ombi/compare/v4.12.1...v4.12.2) (2022-02-16)


### Bug Fixes

* **requests:** :bug: Fixed the approve 4k option on the requests list not working as expected ([c0189da](https://github.com/Ombi-app/Ombi/commit/c0189dad478ea375beda61ba3bee3f029a39b8e5))



## [4.12.1](https://github.com/Ombi-app/Ombi/compare/v4.12.0...v4.12.1) (2022-02-16)


### Bug Fixes

* **requests:** :bug: Fixed the issue where Approving a 4K Request wouldn't send it to the correct 4K radarr instance ([87cb990](https://github.com/Ombi-app/Ombi/commit/87cb9903db30e1dead25ee8c5ea34305eb084a03)), closes [#4509](https://github.com/Ombi-app/Ombi/issues/4509)



# [4.12.0](https://github.com/Ombi-app/Ombi/compare/v4.11.8...v4.12.0) (2022-02-14)


### Features

* **radarr:** 4K Requests and Radarr 4K support  ([ba88848](https://github.com/Ombi-app/Ombi/commit/ba88848866b0a9dedb1e79b55c4d81a0fd453843))



## [4.11.8](https://github.com/Ombi-app/Ombi/compare/v4.11.7...v4.11.8) (2022-02-13)


### Bug Fixes

* **settings:** :bug: Fixed an issue where we were not displaying the excluded keyworks correctly in the TheMovieDbSettings page ([d3b3316](https://github.com/Ombi-app/Ombi/commit/d3b3316cbac18356b2f6b0912a3deb2c183e6534))



## [4.11.7](https://github.com/Ombi-app/Ombi/compare/v4.11.6...v4.11.7) (2022-02-12)


### Bug Fixes

* **notifications:** :bug: This is a fix for some of the duplicate notification issues [#3825](https://github.com/Ombi-app/Ombi/issues/3825) ([22bb422](https://github.com/Ombi-app/Ombi/commit/22bb4226ead2d62e8c2c2c05be47d7da621402e2))



## [4.11.6](https://github.com/Ombi-app/Ombi/compare/v4.11.5...v4.11.6) (2022-02-10)


### Bug Fixes

* **plex:** Fixed an issue where in a rare case we couldn't sync the data [#4502](https://github.com/Ombi-app/Ombi/issues/4502) ([191318d](https://github.com/Ombi-app/Ombi/commit/191318ddad5a8148422955bf928f1c49b890e3eb))



## [4.11.5](https://github.com/Ombi-app/Ombi/compare/v4.11.4...v4.11.5) (2022-02-05)


### Bug Fixes

* **sonarr:** Fixed where requesting all seasons would only mark the latest as monitored [#4496](https://github.com/Ombi-app/Ombi/issues/4496) ([cfb85c2](https://github.com/Ombi-app/Ombi/commit/cfb85c23d77626b9ec1d99a6cf76497c438d0338))



## [4.11.4](https://github.com/Ombi-app/Ombi/compare/v4.11.3...v4.11.4) (2022-02-05)


### Bug Fixes

* **media-sync:** Add sanity checks upon media server sync ([#4493](https://github.com/Ombi-app/Ombi/issues/4493)) ([9915234](https://github.com/Ombi-app/Ombi/commit/9915234d38d4701c527081ccc21b566980375331)), closes [#4472](https://github.com/Ombi-app/Ombi/issues/4472)
* **newsletter:** Fix newsletter not publishing double episodes ([#4495](https://github.com/Ombi-app/Ombi/issues/4495)) ([ddf63fb](https://github.com/Ombi-app/Ombi/commit/ddf63fbed0b9cbe458aec37318758c76b99b2de9))



## [4.11.3](https://github.com/Ombi-app/Ombi/compare/v4.11.2...v4.11.3) (2022-02-03)


### Bug Fixes

* **API:** Fixed an issue where the API key couldn't delete a request [#4489](https://github.com/Ombi-app/Ombi/issues/4489) ([8e42dbf](https://github.com/Ombi-app/Ombi/commit/8e42dbf8f78caa51ca891bf3d702c6b0ac401f9c))



## [4.11.2](https://github.com/Ombi-app/Ombi/compare/v4.11.1...v4.11.2) (2022-02-01)


### Bug Fixes

* :globe_with_meridians: Added Czech and Chinese Simplified to the language list ([68ef366](https://github.com/Ombi-app/Ombi/commit/68ef366e8525e2c349b9e81704ad8bcca6c347a0))



## [4.11.1](https://github.com/Ombi-app/Ombi/compare/v4.11.0...v4.11.1) (2022-02-01)



# [4.11.0](https://github.com/Ombi-app/Ombi/compare/v4.10.4...v4.11.0) (2022-02-01)


### Features

* **newsletter:** Started to localize the newsletter ([#4485](https://github.com/Ombi-app/Ombi/issues/4485)) ([b5ec556](https://github.com/Ombi-app/Ombi/commit/b5ec5562435021ea4b8af07c9b64a3f7249b570a))



