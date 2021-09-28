## [4.1.2](https://github.com/Ombi-app/Ombi/compare/v4.1.1...v4.1.2) (2021-09-28)


### Bug Fixes

* :bug: Pretending to fix a bug ([5351c14](https://github.com/Ombi-app/Ombi/commit/5351c14cb087f9ecbb37b784724bb35107d17cb8))



## [4.1.1](https://github.com/Ombi-app/Ombi/compare/v4.0.1506...v4.1.1) (2021-09-28)



## [4.0.1506](https://github.com/Ombi-app/Ombi/compare/v4.0.1499...v4.0.1506) (2021-09-28)


### Bug Fixes

* :bug: Fixed the issue where we were not generating the newsletter plex mediaserver link correctly ([b6064e9](https://github.com/Ombi-app/Ombi/commit/b6064e9308ee1218517f54d331c9bd7953ca631e))
* **request-limits:** :bug: Fixed the issue where we were calculating Tv Request limit reset date incorrectly ([ceaec3f](https://github.com/Ombi-app/Ombi/commit/ceaec3feb0c9fbdab48595d7e425930a39d87ad5))
* :bug: Fixed the issue where the user management login dates were not local time ([97be373](https://github.com/Ombi-app/Ombi/commit/97be3737700ed7b1ee915dbcd9f44103665d472c)), closes [#2925](https://github.com/Ombi-app/Ombi/issues/2925)


### Features

* **request-limits:** :card_file_box: Added new user field migrations to mysql and sqlite ([f73bccb](https://github.com/Ombi-app/Ombi/commit/f73bccbea759fb4aeadc32f94b1ef6c9aecc5e94))
* **request-limits:** :sparkles: Added in the main logic for the new request limits ([70d5bf5](https://github.com/Ombi-app/Ombi/commit/70d5bf52bff2e321fb1f3d00fd1cd1121a2717b7))
* **request-limits:** :sparkles: Added the UI portion to set the new limits ([978d4ea](https://github.com/Ombi-app/Ombi/commit/978d4ea33b32d5a8333c75e29d4cd702e434c5f0))
* **request-limits:** :sparkles: Request limits are no longer a rolling date. But reset at the start of the week or month depending on the preference ([364b9f1](https://github.com/Ombi-app/Ombi/commit/364b9f11afcd470cc2b112cf81cd840316ddc80e))
* **request-limits:** :sparkles: Updated the RequestLimit Rules to use the new refactored service ([e31ee8d](https://github.com/Ombi-app/Ombi/commit/e31ee8d89213a8fc179db56cc51d3f02648b51ec))
* **request-limits:** :tada: Started on the request limits, applied to the user model ([f5310b7](https://github.com/Ombi-app/Ombi/commit/f5310b786b43b3c00d392da977c2b3367a5e4e11))



## [4.0.1488](https://github.com/Ombi-app/Ombi/compare/v4.0.1487...v4.0.1488) (2021-09-18)


### Features

* **discover:** :lipstick: Moved the advanced search to the nav bar for better discovery ([f83abaf](https://github.com/Ombi-app/Ombi/commit/f83abafdd9fc416c8ca4d99a52b5fc94e0b781fd))



