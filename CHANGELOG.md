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



## [4.6.3](https://github.com/Ombi-app/Ombi/compare/v4.6.2...v4.6.3) (2021-11-11)


### Bug Fixes

* **discover:** :bug: Display TV + movies on actor page in user language ([#4395](https://github.com/Ombi-app/Ombi/issues/4395)) ([fe635c7](https://github.com/Ombi-app/Ombi/commit/fe635c7106bc487ff879bdc8a73bab16cb389b97))
* **permissions:** :bug: Improved the security around the role "Manage Own Requests" ([#4397](https://github.com/Ombi-app/Ombi/issues/4397)) ([334a32b](https://github.com/Ombi-app/Ombi/commit/334a32bca42f90198d9b720d2bdb710a583be47f)), closes [#4391](https://github.com/Ombi-app/Ombi/issues/4391)
* **search:** Fixed some cases where search wouldn't work correctly ([#4398](https://github.com/Ombi-app/Ombi/issues/4398)) ([4410790](https://github.com/Ombi-app/Ombi/commit/4410790bc096826bc11554098f846e3acb59589a))



## [4.6.2](https://github.com/Ombi-app/Ombi/compare/v4.6.1...v4.6.2) (2021-11-10)


### Bug Fixes

* **discover:** TV shows now display on the Actor Pages ([#4388](https://github.com/Ombi-app/Ombi/issues/4388)) ([6b716e7](https://github.com/Ombi-app/Ombi/commit/6b716e722076e3d1e6bf2097c5263645d5ea9edf))



## [4.6.1](https://github.com/Ombi-app/Ombi/compare/v4.6.0...v4.6.1) (2021-11-10)


### Bug Fixes

* :bug: Fixed the MySQL issue after .net 6 upgrade [#4393](https://github.com/Ombi-app/Ombi/issues/4393) ([fea7ff0](https://github.com/Ombi-app/Ombi/commit/fea7ff05139e9ff50c8097fa5389b4ef9ad21a15))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([c6acb45](https://github.com/Ombi-app/Ombi/commit/c6acb45f8d3f371c0b4024c4272849d0d0cc867f))
* **translations:** 🌐 New translations from Crowdin [skip ci] ([18c220a](https://github.com/Ombi-app/Ombi/commit/18c220a0cd0d19e45a07d0c319da2b9512778a8a))



# [4.6.0](https://github.com/Ombi-app/Ombi/compare/v4.4.0...v4.6.0) (2021-11-09)


### Features

* :sparkles: Upgrade Ombi to .NET 6 ([#4390](https://github.com/Ombi-app/Ombi/issues/4390)) ([719eb7d](https://github.com/Ombi-app/Ombi/commit/719eb7dbe37b3a72d264e2f670067518eef70694)), closes [#4392](https://github.com/Ombi-app/Ombi/issues/4392)



# [4.4.0](https://github.com/Ombi-app/Ombi/compare/v4.3.2...v4.4.0) (2021-11-06)


### Bug Fixes

* **request-list:** :bug: Fixed an issue where the request options were not appearing for Music requests ([c0406a2](https://github.com/Ombi-app/Ombi/commit/c0406a2ddebafb03d98ed25cdf7d89dc9a600c7d))


### Features

* **mass-email:** :sparkles: Added the ability to configure the Mass Email, we can now send BCC and we are less likely to be rate limited when not using bcc [#4377](https://github.com/Ombi-app/Ombi/issues/4377) ([ca655ae](https://github.com/Ombi-app/Ombi/commit/ca655ae57042dec44106a2f2ef5ba2e6f1019ee4))



## [4.3.2](https://github.com/Ombi-app/Ombi/compare/v4.3.1...v4.3.2) (2021-11-02)


### Bug Fixes

* **translations:** 🌐 Localization - Ensuring all of the app including backend are localized [#4366](https://github.com/Ombi-app/Ombi/issues/4366) ([5e140ab](https://github.com/Ombi-app/Ombi/commit/5e140ab6183b887a7665f5e870eb0bd05d487ace))



