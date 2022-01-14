# [4.10.0](https://github.com/Ombi-app/Ombi/compare/v4.9.2...v4.10.0) (2022-01-14)


### Features

* **notifications:** :sparkles: Send new request email notifications to power users ([#4462](https://github.com/Ombi-app/Ombi/issues/4462)) ([10cc0c0](https://github.com/Ombi-app/Ombi/commit/10cc0c0951f13221179516f8ff5c44dbecc9a0fd))



## [4.9.2](https://github.com/Ombi-app/Ombi/compare/v4.9.1...v4.9.2) (2022-01-14)


### Bug Fixes

* :bug: Add UI for Emby recently added cronjob settings ([#4469](https://github.com/Ombi-app/Ombi/issues/4469)) ([7d47bbe](https://github.com/Ombi-app/Ombi/commit/7d47bbe92204855bf75d70b8fa548f9c3f3612bc))
* **sonarr:** :bug: Fixed an issue where we could attempt to add a series to sonarr before sonarr has got all the metadata [#4459](https://github.com/Ombi-app/Ombi/issues/4459) ([5c691dc](https://github.com/Ombi-app/Ombi/commit/5c691dc98437a4cd24560ff625414fe05dd22f89))
* **wizard:** :bug: Fixed the issue where the Application Url wasn't validated in the wizard ([33b8d11](https://github.com/Ombi-app/Ombi/commit/33b8d1111a1c6663d8c0bbd912be4660da7d013f)), closes [#4417](https://github.com/Ombi-app/Ombi/issues/4417)



## [4.9.1](https://github.com/Ombi-app/Ombi/compare/v4.9.0...v4.9.1) (2022-01-14)


### Bug Fixes

* **discover:** 🌐 Localize episodes names in TV details ([#4467](https://github.com/Ombi-app/Ombi/issues/4467)) [skip ci] ([35806ea](https://github.com/Ombi-app/Ombi/commit/35806ea2d2c866d628cf08577026a02ab04e49d9))
* **email-notifications:** :bug: Fixed the issue where legacy requests were showing broken poster images [#4452](https://github.com/Ombi-app/Ombi/issues/4452) ([0ece2fd](https://github.com/Ombi-app/Ombi/commit/0ece2fd6e0eb01e0b7d4d2a01e1a276c7a9c5a51))
* **emby/jellyfin:** :bug: A more reliable Emby and Jellyfin sync [skip ci] ([ad677fa](https://github.com/Ombi-app/Ombi/commit/ad677fa02eb75633014e9c9791c21ed2d6a23229))



# [4.9.0](https://github.com/Ombi-app/Ombi/compare/v4.8.1...v4.9.0) (2022-01-05)


### Features

* **customization:** :sparkles: Added possibility for custom favicons ([40af659](https://github.com/Ombi-app/Ombi/commit/40af6593b668d4712327c18f92f5b7b5a0a65e26))



## [4.8.1](https://github.com/Ombi-app/Ombi/compare/v4.8.0...v4.8.1) (2022-01-04)



# [4.8.0](https://github.com/Ombi-app/Ombi/compare/v4.7.11...v4.8.0) (2021-12-22)


### Bug Fixes

* **auto-delete:** :bug: We now also auto delete music requests, this was previously missing ([9fe1f8e](https://github.com/Ombi-app/Ombi/commit/9fe1f8e988aa31d36e7a685ae19f72d9c8414dc0))


### Features

* **details:** :sparkles: Added the notify button back into the details pages for requests ([8b33cdc](https://github.com/Ombi-app/Ombi/commit/8b33cdccef83db8794414b247438214b00860fac))



## [4.7.11](https://github.com/Ombi-app/Ombi/compare/v4.7.10...v4.7.11) (2021-12-17)


### Bug Fixes

* **availability-rules:** :bug: Show the 'Requested' button when a show has all of the episodes marked as requested ([cb7ecf6](https://github.com/Ombi-app/Ombi/commit/cb7ecf684ac3ab204f329a28baecfd4f6cd408f7))



## [4.7.10](https://github.com/Ombi-app/Ombi/compare/v4.7.9...v4.7.10) (2021-12-16)


### Bug Fixes

* **discover:** :bug: Fixed an issue where monitored movies in radarr were not correctly represented on the search results ([75b15bc](https://github.com/Ombi-app/Ombi/commit/75b15bc7cba21f0a14a18c8e64fd52482f5c6325))



## [4.7.9](https://github.com/Ombi-app/Ombi/compare/v4.7.8...v4.7.9) (2021-12-16)


### Bug Fixes

* **sonarr:** :bug: Fixed an issue where we were sometimes incorrectly setting the state of episodes that are already monitored in sonarr ([fd1acb8](https://github.com/Ombi-app/Ombi/commit/fd1acb88cbc5e73f91b7f81e6e28ee06d66b277e))



## [4.7.8](https://github.com/Ombi-app/Ombi/compare/v4.7.7...v4.7.8) (2021-12-11)


### Bug Fixes

* **notifications:** :bug: Fixed the DenyReason sometimes not appearing in the notification message [#4409](https://github.com/Ombi-app/Ombi/issues/4409) ([209e311](https://github.com/Ombi-app/Ombi/commit/209e31175c95f6ee8909d878d45bf8269a9842d9))
* **oauth:** :lock: Fixed the issue where some users running their browsers in a different language could not open the Plex OAuth window ([d5404ea](https://github.com/Ombi-app/Ombi/commit/d5404eaad7837010d6e4563cd8f7a1009231d362)), closes [#4408](https://github.com/Ombi-app/Ombi/issues/4408)
* **translations:** 🌐 New translations from Crowdin ([5cfb76c](https://github.com/Ombi-app/Ombi/commit/5cfb76cad7a25eed8b452bf9c01cef8c32804369))



## [4.7.7](https://github.com/Ombi-app/Ombi/compare/v4.7.6...v4.7.7) (2021-12-08)


### Bug Fixes

* **notifications:** 🐛 Do not notify user upon auto approval of a TV show ([#4432](https://github.com/Ombi-app/Ombi/issues/4432)) ([3ad3bdd](https://github.com/Ombi-app/Ombi/commit/3ad3bddd8313d607ee2a39a51a92e61a3673082c)), closes [#4431](https://github.com/Ombi-app/Ombi/issues/4431)
* **translations:** 🌐 New translations from Crowdin  ([8943a97](https://github.com/Ombi-app/Ombi/commit/8943a978bf459eaeb496d50c61c4d1506c727366))



## [4.7.6](https://github.com/Ombi-app/Ombi/compare/v4.7.5...v4.7.6) (2021-12-02)


### Bug Fixes

* **user-management:** :bug: Fixed an issue where you couldn't 'unset' a users custom quality and root folders ([bae96af](https://github.com/Ombi-app/Ombi/commit/bae96af17f50a80ae3ade235a5ef68d5d2dc12ba))



## [4.7.5](https://github.com/Ombi-app/Ombi/compare/v4.7.4...v4.7.5) (2021-11-28)


### Bug Fixes

* **notifications:** fixed an error that could happen when Ombi sends out a issue notification ([7442dcf](https://github.com/Ombi-app/Ombi/commit/7442dcf59da5d2190cc3087b10402e85bcfcf83b))
* **translations:** 🌐 Fix incorrect text translation reference RequestedByOn ([#4420](https://github.com/Ombi-app/Ombi/issues/4420)) ([202d155](https://github.com/Ombi-app/Ombi/commit/202d155493c29a6ddd4c5507186bf376a28f4c1d))
* **translations:** 🌐 New translations from Crowdin  ([473c172](https://github.com/Ombi-app/Ombi/commit/473c1724922515fe376e0b2058ac391807c923f2))



## [4.7.4](https://github.com/Ombi-app/Ombi/compare/v4.7.3...v4.7.4) (2021-11-25)


### Bug Fixes

* **availability-rules:** :bug: Fixed a small issue where some shows would not appear as Available even know they had no future unaired episodes listed ([914b096](https://github.com/Ombi-app/Ombi/commit/914b096781c9b73292a533a010a5dd05ecfd0aac))
* **emby:** :bug: Fixed an issue where we were not properly syncing episodes ([75529dd](https://github.com/Ombi-app/Ombi/commit/75529dd972c5102f3c5234a2acf6fe664a1bcfad))



## [4.7.3](https://github.com/Ombi-app/Ombi/compare/v4.7.2...v4.7.3) (2021-11-23)



## [4.7.2](https://github.com/Ombi-app/Ombi/compare/v4.7.1...v4.7.2) (2021-11-22)


### Bug Fixes

* **request-list:** :bug: Fixed an issue where the bulk delete would not work for movie requests ([4b540fb](https://github.com/Ombi-app/Ombi/commit/4b540fb45bcc389664f0953159802288d005db9f))



## [4.7.1](https://github.com/Ombi-app/Ombi/compare/v4.7.0...v4.7.1) (2021-11-22)


### Bug Fixes

* **emby:** :bug: Fixed an issue where we slightly broke the full sync ([332d934](https://github.com/Ombi-app/Ombi/commit/332d9344d002a5ffd5aeac516c7441dcdec52248))



# [4.7.0](https://github.com/Ombi-app/Ombi/compare/v4.6.5...v4.7.0) (2021-11-19)


### Features

* **emby:** :sparkles: Added a emby recently added sync! ([a0e1406](https://github.com/Ombi-app/Ombi/commit/a0e14068f4bc457f8a4a565de71707a8f16c803c))



## [4.6.5](https://github.com/Ombi-app/Ombi/compare/v4.6.4...v4.6.5) (2021-11-15)


### Bug Fixes

* **issues:** :bug: Added the issue category to the issue 'cards' [#4403](https://github.com/Ombi-app/Ombi/issues/4403) ([a3739f3](https://github.com/Ombi-app/Ombi/commit/a3739f375c49f48e34da12f0a74e4e068f12ab40))
* **issues:** :bug: Added the issues back to the details page for TV Shows ([0225000](https://github.com/Ombi-app/Ombi/commit/02250000c08a253e57d8a0a855c2d30b8a1e5baa))
* **issues:** :bug: Fixed an issue where you couldn't navigate to the details page from TV issues ([1a2825b](https://github.com/Ombi-app/Ombi/commit/1a2825bf3839b891b16e1dde4030afe53efe090e))
* **issues:** :bug: Fixed where we did not show the poster when an issue is raised for media we do not have a request for [#4402](https://github.com/Ombi-app/Ombi/issues/4402) ([15e37b5](https://github.com/Ombi-app/Ombi/commit/15e37b532a83097dbdf1a9fea3eead7d0e211898))



## [4.6.4](https://github.com/Ombi-app/Ombi/compare/v4.6.3...v4.6.4) (2021-11-12)



