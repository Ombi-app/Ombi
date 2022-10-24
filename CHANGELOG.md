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



# [4.23.0](https://github.com/Ombi-app/Ombi/compare/v4.22.4...v4.23.0) (2022-08-09)


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


### Features

* **discover:** Add original language filter ([ef7ec86](https://github.com/Ombi-app/Ombi/commit/ef7ec861d8aede2a4817752c990617f583805391))



## [4.16.17](https://github.com/Ombi-app/Ombi/compare/v4.16.16...v4.16.17) (2022-04-25)



## [4.16.16](https://github.com/Ombi-app/Ombi/compare/v4.16.15...v4.16.16) (2022-04-25)


### Bug Fixes

* **4616:** :bug: fixed mandatory fields ([d8f2260](https://github.com/Ombi-app/Ombi/commit/d8f2260c7ae3ed48386743b7adbd06e284487034))



## [4.16.15](https://github.com/Ombi-app/Ombi/compare/v4.16.14...v4.16.15) (2022-04-24)



