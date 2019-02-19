# Changelog

## v3.0.4256 (2019-02-18)

### **New Features**

- Update discord link to follow the scheme of other links. [Tom McClellan]

- Update issue templates. [Jamie]

- Update README.md. [Jamie]

- Update CHANGELOG.md. [Jamie]

- Added the functionality to remove a user from getting notifications to their mobile device #2780. [tidusjar]

- Added a demo mode, this will only show movies and shows that are in the public domain. Dam that stupid fruit company. [tidusjar]

- Added Actor Searching for Movies! [TidusJar]

- Added the ability to change where the View on Emby link goes to #2730. [TidusJar]

- Added the request queue to the notifications UI so you can turn it off per notification agent #2747. [TidusJar]

- Added new classes to the posters #2732. [TidusJar]

### **Fixes**

- Fix: src/Ombi/package.json to reduce vulnerabilities. [snyk-bot]

- Fixed #2801 this is when a season is not correctly monitored in sonarr when approved by an admin. [tidusjar]

- Small improvements to try and mitigate #2750. [tidusjar]

- Removed some areas where we clear out the cache. This should help with DB locking #2750. [tidusjar]

- Fixed #2810. [tidusjar]

- Cannot create an issue comment with the API #2811. [tidusjar]

- Set the local domain if the Application URL is set for the HELO or EHLO commands. #2636. [tidusjar]

- New translations en.json (Spanish) [Jamie]

- Delete ISSUE_TEMPLATE.md. [Jamie]

- More minor grammatical edits. [Andrew Metzger]

- Minor grammatical edits. [Andrew Metzger]

- Fixed #2802 the issue where "Issues" were not being deleted correctly. [tidusjar]

- Fixed #2797. [tidusjar]

- New translations en.json (Dutch) [Jamie]

- New translations en.json (Spanish) [Jamie]

- New translations en.json (Portuguese, Brazilian) [Jamie]

- Fixed #2786. [tidusjar]

- Fixed #2756. [tidusjar]

- Ignore the UserName header as part of the Api is the value is an empty string. [tidusjar]

- Fixed #2759. [tidusjar]

- Did #2756. [TidusJar]

- Fixed the exception that sometimes makes ombi fallover. [TidusJar]

- New translations en.json (Swedish) [Jamie]

- New translations en.json (Swedish) [Jamie]

- New translations en.json (Swedish) [Jamie]

- New translations en.json (Swedish) [Jamie]

- New translations en.json (Swedish) [Jamie]

- New translations en.json (Swedish) [Jamie]

- New translations en.json (Swedish) [Jamie]

- New translations en.json (Swedish) [Jamie]

- New translations en.json (Swedish) [Jamie]

- New translations en.json (Swedish) [Jamie]

- Log the error to the ui to figure out what's going on with #2755. [tidusjar]

- Small fix when denying a request with a reason, we wasn't updating the ui. [TidusJar]

- Make sure we can only set the ApiAlias when using the API Key. [tidusjar]

- #2363 Added the ability to pass any username into the API using the ApiAlias header. [tidusjar]

- Removed the Add user to Plex from Ombi. [tidusjar]


## v3.0.4119 (2019-01-09)

### **New Features**

- Update CHANGELOG.md. [Jamie]

- Added a page where the admin can write/style/basically do whatever they want with e.g. FAQ for the users #2715 This needs to be enabled in the Customization Settings and then it's all configured on the page. [TidusJar]

- Updated the AspnetCore.App package to remove the CVE-2019-0564 vulnerability. [TidusJar]

- Added a global language flag that now applies to the search by default. [tidusjar]

- Updated the frontend packages (Using Angular 7 now) [TidusJar]

- Added capture of anonymous analytical data. [tidusjar]

- Added {AvailableDate} as a Notification Variable, this is the date the request was marked as available. See here: https://github.com/tidusjar/Ombi/wiki/Notification-Template-Variables. [tidusjar]

- Added the ability to search movies via the movie db with a different language! [tidusjar]

- Added the ability to specify a year when searching for movies. [tidusjar]

- Update NewsletterTemplate.html. [d1slact0r]

- Update NewsletterTemplate.html. [d1slact0r]

- Update NewsletterTemplate.html. [d1slact0r]

- Update HtmlTemplateGenerator.cs. [d1slact0r]

- Update NewsletterTemplate.html. [d1slact0r]

- Update HtmlTemplateGenerator.cs. [d1slact0r]

- Update NewsletterTemplate.html. [d1slact0r]

- Update NewsletterTemplate.html. [d1slact0r]

- Update NewsletterTemplate.html. [d1slact0r]

- Update HtmlTemplateGenerator.cs. [d1slact0r]

- Updated boostrap #2694. [Jamie]

- Added the ability to deny a request with a reason. [TidusJar]

- Update EmbyEpisodeSync.cs. [Jamie]

- Updated to .net core 2.2 and included a linux-arm64 build. [TidusJar]

### **Fixes**

- There is now a new Job in ombi that will clear out the Plex/Emby data and recache. This will prevent the issues going forward that we have when Ombi and the Media server fall out of sync with deletions/updates #2641 #2362 #1566. [TidusJar]

- Potentially fix #2726. [TidusJar]

- New translations en.json (Spanish) [Jamie]

- New translations en.json (Spanish) [Jamie]

- New translations en.json (Dutch) [Jamie]

- Fixed #2725 and #2721. [TidusJar]

- Made the newsletter use the default lanuage code set in the Ombi settings for movie information. [TidusJar]

- Save the language code against the request so we can use it later e.g. Sending to the DVR apps. [tidusjar]

- Fixed #2716. [tidusjar]

- Make the newsletter BCC the users rather than creating a million newsletters (Hopefully will stop SMTP providers from marking as spam). This does mean that the custom user customization in the newsletter will no longer work. [TidusJar]

- If we don't know the Plex agent, then see if it's a ImdbId, if it's not check the string for any episode and season hints #2695. [tidusjar]

- New translations en.json (Swedish) [Jamie]

- New translations en.json (Spanish) [Jamie]

- New translations en.json (Portuguese, Brazilian) [Jamie]

- New translations en.json (Polish) [Jamie]

- New translations en.json (Norwegian) [Jamie]

- New translations en.json (Italian) [Jamie]

- New translations en.json (German) [Jamie]

- New translations en.json (French) [Jamie]

- New translations en.json (Dutch) [Jamie]

- New translations en.json (Danish) [Jamie]

- New translations en.json (Dutch) [Jamie]

- New translations en.json (Dutch) [Jamie]

- New translations en.json (Dutch) [Jamie]

- Made the search results the language specified in the search refinement. [tidusjar]

- Fixed #2704. [tidusjar]

- Now it is fixed :) [d1slact0r]

- Android please be nice now. [d1slact0r]

- Fixed title bit better. [d1slact0r]

- Fixed titles. [d1slact0r]

- This should fix the build for sure (stupid quotes) [d1slact0r]

- Fixes build. [d1slact0r]

- Rewritten the whole newsletter template. [d1slact0r]

- Fixed #2697. [tidusjar]

- Add linux-arm runtime identifier. [aptalca]

- Add back arm packages. [aptalca]

- Add arm32 package. [aptalca]

- Fixed #2691. [tidusjar]

- Fixed linting. [TidusJar]

- Fixed the Plex OAuth when going through the wizard. [TidusJar]

- Fixed #2678. [TidusJar]

- Deny reason for movie requests. [TidusJar]

- Set the landing and login pages background refresh to 15 seconds rather than 10 and 7. [TidusJar]

- Fixed a bug with us thinking future dated emby episodes are not available, Consoldated the emby and plex search rules (since they have the same logic) [TidusJar]

- Fixed build. [TidusJar]


## v3.0.4036 (2018-12-11)

### **New Features**

- Changelog. [Jamie]

- Added Sonarr v3  #2359. [TidusJar]

### **Fixes**

- !changelog. [Jamie]

- Fixed a missing translation. [Jamie]

- Fixed a potential security vulnerability. [Jamie]

- Sorted out some of the settings pages, trying to make it consistent. [Jamie]

- #2669 Fixed missing translations. [TidusJar]

- Maps alias email variable for welcome emails. [Victor Usoltsev]

- Increased the logo size on the landing page to match the container below it. [Jamie]

- Think the request queue is done! [Jamie]

- Finished off the job. [TidusJar]


## v3.0.3988 (2018-11-23)

### **New Features**

- Updated the emby api since we no longer need the extra parameters to send to emby to log in a local user #2546. [Jamie]

- Added the ability to get the ombi user via a Plex Token #2591. [Jamie]

- Update CHANGELOG.md. [Jamie]

### **Fixes**

- !changelog. [Jamie]

- Made the subscribe/unsubscribe button more obvious on the UI #2309. [Jamie]

- Fixed #2603. [Jamie]

- Fixed the issue with the user overrides #2646. [Jamie]

- Fixed the issue where we could sometimes allow the request of a whole series when the user shouldn't be able to. [Jamie]

- Fixed the issue where we were marking episodes as available with the Emby connection when they have not yet aired #2417 #2623. [TidusJar]

- Fixed the issue where we were marking the whole season as wanted in Sonarr rather than the individual episode #2629. [TidusJar]

- Fixed #2623. [Jamie]

- Fixed #2633. [TidusJar]

- Fixed #2639. [Jamie]

- Show the TV show as available when we have all the episodes but future episodes have not aired. #2585. [Jamie]


## v3.0.3945 (2018-10-25)

### **New Features**

- Update Readme for Lidarr. [Qstick]

- Update CHANGELOG.md. [Jamie]

### **Fixes**

- New translations en.json (French) [Jamie]

- New translations en.json (French) [Jamie]

- New translations en.json (French) [Jamie]

- New translations en.json (Dutch) [Jamie]

- Fixed the issue with mobile notifications. [Jamie]

- Fixed #2514. [Jamie]


## v3.0.3923 (2018-10-19)

### **New Features**

- Update CHANGELOG.md. [Jamie]

### **Fixes**

- Fixed #2601. [Jamie]


## v3.0.3919 (2018-10-17)

### **New Features**

- Added automation tests for the voting feature. [TidusJar]

- Update LidarrAvailabilityChecker.cs. [Jamie]

- Update CHANGELOG.md. [Jamie]

- Changes language selector to always show native language name. [Victor Usoltsev]

- Updated test dependancies. [TidusJar]

- Added in the external repo so we can rip out external stuff. [TidusJar]

- Added the ability to purge/remove issues. [TidusJar]

### **Fixes**

- New translations en.json (French) [Jamie]

- New translations en.json (French) [Jamie]

- New translations en.json (French) [Jamie]

- New translations en.json (French) [Jamie]

- New translations en.json (Swedish) [Jamie]

- New translations en.json (Spanish) [Jamie]

- New translations en.json (Portuguese, Brazilian) [Jamie]

- New translations en.json (Polish) [Jamie]

- New translations en.json (Norwegian) [Jamie]

- New translations en.json (Italian) [Jamie]

- New translations en.json (German) [Jamie]

- New translations en.json (French) [Jamie]

- New translations en.json (Dutch) [Jamie]

- New translations en.json (Danish) [Jamie]

- When a users requests content and the voting is enabled, the user who requested is an automatic +1 vote. [TidusJar]

- Revert, no idea how this happened. [TidusJar]

- Fixed the build. Thanks Matt! [TidusJar]

- Fixes untickable mass email checkboxes in Safari. [Victor Usoltsev]

- [ImgBot] optimizes images. [ImgBotApp]

- Revert "Feature/purge issues" [Jamie]

- Fixed the issue where user preferences was not being inported into some notifications. [TidusJar]

- New role to enable users to remove their own requests. [Anojh]

- Users can now remove their own requests. [Anojh]

- New translations en.json (Danish) [Jamie]

- Fixed lidarr newsletter bug. [Jamie]

- Potentially fix the user profiles issue. [Jamie]

- Hides Radarr options on movie requests page if only 1 option available. [Victor Usoltsev]

- Hides Sonarr options on tv requests page if only 1 option available. [Victor Usoltsev]

- Fixed the issue where we could not delete users #2558. [TidusJar]

- New translations en.json (German) [Jamie]

- New translations en.json (Swedish) [Jamie]

- New translations en.json (Spanish) [Jamie]

- New translations en.json (Portuguese, Brazilian) [Jamie]

- New translations en.json (Polish) [Jamie]

- New translations en.json (Norwegian) [Jamie]

- New translations en.json (Italian) [Jamie]

- New translations en.json (German) [Jamie]

- New translations en.json (French) [Jamie]

- New translations en.json (Dutch) [Jamie]

- New translations en.json (Danish) [Jamie]

- Subscribe the user to the request when they vote on it. [TidusJar]

- Fixed #2555. [Jamie]

- Fixed #2549. [Jamie]

- Removed the pinID from the OAuth url #2548. [Jamie]

- Put the issue purge limit on the issues page. [Jamie]

- Date and times are now in the local users date time. [TidusJar]

- Fixed the migration. [TidusJar]

- ExternalContext migrations. [TidusJar]

- The settings have now been split out of the main db. [TidusJar]

- Search for the Lidarr Album when it's a new artist. [TidusJar]

- The album in Lidarr does not need to be marked as monitored for us to pick up it's available. Fixes #2536. [Jamie]

- Truncate the request title. [Jamie]

- Fixed #2535. [Jamie]


## v3.0.3795 (2018-09-23)

### **New Features**

- Update CHANGELOG.md. [Jamie]

### **Fixes**

- Fixed the issue with notifications not sending. [Jamie]

- Removes Legacy command result variables. [Qstick]


## v3.0.3786 (2018-09-22)

### **New Features**

- Update CHANGELOG.md. [Jamie]

### **Fixes**

- New translations en.json (Swedish) [Jamie]

- New translations en.json (Spanish) [Jamie]

- New translations en.json (Portuguese, Brazilian) [Jamie]

- New translations en.json (Polish) [Jamie]

- New translations en.json (Norwegian) [Jamie]

- New translations en.json (Italian) [Jamie]

- New translations en.json (German) [Jamie]

- New translations en.json (French) [Jamie]

- New translations en.json (Dutch) [Jamie]

- New translations en.json (Danish) [Jamie]

- New translations en.json (Swedish) [Jamie]

- New translations en.json (Spanish) [Jamie]

- New translations en.json (Portuguese, Brazilian) [Jamie]

- New translations en.json (Polish) [Jamie]

- New translations en.json (Norwegian) [Jamie]

- New translations en.json (Italian) [Jamie]

- New translations en.json (German) [Jamie]

- New translations en.json (French) [Jamie]

- New translations en.json (Dutch) [Jamie]

- New translations en.json (Danish) [Jamie]

- New translations en.json (Swedish) [Jamie]

- New translations en.json (Spanish) [Jamie]

- New translations en.json (Portuguese, Brazilian) [Jamie]

- New translations en.json (Polish) [Jamie]

- New translations en.json (Norwegian) [Jamie]

- New translations en.json (Italian) [Jamie]

- New translations en.json (German) [Jamie]

- New translations en.json (French) [Jamie]

- New translations en.json (Dutch) [Jamie]

- New translations en.json (Danish) [Jamie]

- New translations en.json (Swedish) [Jamie]

- New translations en.json (German) [Jamie]

- New translations en.json (German) [Jamie]

- New translations en.json (Swedish) [Jamie]

- New translations en.json (Spanish) [Jamie]

- New translations en.json (Portuguese, Brazilian) [Jamie]

- New translations en.json (Polish) [Jamie]

- New translations en.json (Norwegian) [Jamie]

- New translations en.json (Italian) [Jamie]

- New translations en.json (German) [Jamie]

- New translations en.json (French) [Jamie]

- New translations en.json (Dutch) [Jamie]

- New translations en.json (Danish) [Jamie]

- New translations en.json (Portuguese, Brazilian) [Jamie]

- Fix #2529 - Change data type to long. [Anojh]

- Fix #2527 - Music request not triggering search and failing. [Anojh]


## v3.0.3776 (2018-09-21)

### **New Features**

- Update settingsmenu.component.html. [Jamie]

- Added the request limits in the ui for music. [Jamie]

- Added the root folders and qualities per user! [Jamie]

- Updated all the MS packages. [TidusJar]

- Update the .net core packages to fix "CVE-2018-8409: ASP.NET Core Denial Of Service Vulnerability" [TidusJar]

- Change way remainingrequests component is notified. [Kenton Royal]

- Added the music request limits. [TidusJar]

- Added the Notification Preferences to the user. [TidusJar]

- Added the API to add user notification preferences. [TidusJar]

- Added more logging into the updater. [Jamie]

- Update CHANGELOG.md. [Jamie]

### **Fixes**

- Fixed #2518. [TidusJar]

- Fixed #2522. [TidusJar]

- Fixed #2485. [TidusJar]

- Fixed #2516. [TidusJar]

- Fix bug in which requested TV wasn't logging for some users. [Kenton Royal]

- Add to translations. [Kenton Royal]

- Add html for displaying remaining requests on users page. [Kenton Royal]

- Add quota fields to user view model. [Kenton Royal]

- Users can now see the music search tab #2493. [TidusJar]

- Add href to a tags so that a pointer cursor shows on requests page. [Stephen Panzer]

- Allow Lidarr to specify if we should search for the album. [TidusJar]

- Fixed the issue if in Radarr we only want to add and monitor, if the movie already exists we search for it. [TidusJar]

- Fix bug causing wrong time to be displayed for next request. [Kenton Royal]

- Bodge fix test to prevent compile error. [Kenton Royal]

- Fix displaying year in issue dialog. [Stephen Panzer]

- Add clearfix class. Closes #2486. [Stephen Panzer]

- Correct path of lidarr component import for unix systems. [Kenton Royal]

- Refactor code. [Kenton Royal]

- Fix formatting error. [Kenton Royal]

- Revert "Revert request.service.ts to version on upstream/develop" [Kenton Royal]

- Revert request.service.ts to version on upstream/develop. [Kenton Royal]

- Fix lint errors. [Kenton Royal]

- Move logic for notifying when reuqest is complete. [Kenton Royal]

- Remove import. [Kenton Royal]

- Remove unused module. [Kenton Royal]

- Refactor code. [Kenton Royal]

- Add text to translation file. [Kenton Royal]

- Fix query for fetching requested tv shows. [Kenton Royal]

- Add vscode to gitignore. [Kenton Royal]

- Fix lint errors. [Kenton Royal]

- Remove unused methods from SearchController. [Kenton Royal]

- Remove local vscode files. [Kenton Royal]

- Fix bug when submitting requests for multiple episodes accross multiple seasons. [Kenton Royal]

- Fix bug with TV requests in which requesting a seasion would treat request as single episode. [Kenton Royal]

- Fix issues with remaining count updating. [Kenton Royal]

- Trigger update of request limit on new request. [Kenton Royal]

- Add logic for movie request count. [Kenton Royal]

- Add logic for retriving request information. [Kenton Royal]

- Move to seperate component and display for both TV and movies. [Kenton Royal]

- Add dummy for request counter. [Kenton Royal]

- Fix scss import for unix systems. [Kenton Royal]

- Add methods to interface and add model class. [Kenton Royal]

- !fixed lint. [TidusJar]

- Fixed #2481. [TidusJar]

- New translations en.json (Swedish) [Jamie]

- New translations en.json (Spanish) [Jamie]

- New translations en.json (Portuguese, Brazilian) [Jamie]

- New translations en.json (Polish) [Jamie]

- New translations en.json (Norwegian) [Jamie]

- New translations en.json (Italian) [Jamie]

- New translations en.json (German) [Jamie]

- New translations en.json (French) [Jamie]

- New translations en.json (Dutch) [Jamie]

- New translations en.json (Danish) [Jamie]

- Fixed #2475. [Jamie]

- Stript out certain characters when sending a pushover message #2385. [TidusJar]

- Add default values for Priority and Sound. [David Pooley]

- Allow for the ability to set Pushover notification sound and priority from within Ombi. [David Pooley]

- It works now when we request an album when we do not have the artist in Lidarr. Waiting on https://github.com/lidarr/Lidarr/issues/459 to do when we have the artist. [Jamie]

- Fix non-Windows builds. Fixes #2453. [Joe Groocock]


## v3.0.3587 (2018-08-19)

### **New Features**

- Added the ability to invite Plex Friends from the user management screen. [Jamie]

- Added rich notifications for mobile. [Jamie]

- Updater fixes. [Jamie]

- Added updater test mode. [Jamie Rees]

- Added a new API method to delete issue comments. [TidusJar]

- Updated @ngu/carousel to beta version to remove rxjs-compat dependency. [Matt Jeanes]

- Update to Angular 6/Webpack 4. [Matt Jeanes]

- Update CHANGELOG.md. [Jamie]

- Updated the way we create the wizard user, errors show now be fed back to the user. [Jamie]

- Added Brazillian Portuguese as a language and also Polish. [Jamie]

- Updated swagger. [Jamie]

- Updated to 2.1.1. [Jamie]

### **Fixes**

- Now include the release year in the issue title #2381. [TidusJar]

- Made the OAuth a Popout to work with Org. [Jamie]

- Fixed #2418. [TidusJar]

- #2408 Added the feature to delete comments on issues. [Jamie]

- New translations en.json (Swedish) [Jamie]

- New translations en.json (French) [Jamie]

- Fixed #2440. [TidusJar]

- Delete cake.config. [Chris Pritchard]

- Initial attempt at getting anime seriestype working. [Chris Pritchard]

- Add cake.config. [Chris Pritchard]

- Fixed the issue where we wouldn't correctly mark some shows as available when there was no provider id #2429. [Jamie]

- Fixed the 'loop' in the cacher #2429. [Jamie]

- Fixed #2427. [Jamie]

- Fixed #2424. [Jamie]

- Fixed #2409. [Jamie]

- More updater. [Jamie]

- Humanize the request type enum in notifications e.g. TvShow will now appear as "Tv Show" #2416. [TidusJar]

- Made the quality override and root folder override load when we load the show (It will now appear) [Jamie]

- Fixed #2415  where power users could not set the Sonarr Quality Override or Root Folder Override. [Jamie]

- #2371 Fixed the issue where certain actions would not setup the series correctly in Sonarr. [Jamie]

- Tightened up the security from an API perspecitve. [TidusJar]

- Stop the root folder and profile calls from erroring. [TidusJar]

- New translations en.json (Polish) [Jamie]

- New translations en.json (Polish) [Jamie]

- New translations en.json (Polish) [Jamie]

- New translations en.json (Portuguese, Brazilian) [Jamie]

- New translations en.json (Portuguese, Brazilian) [Jamie]

- New translations en.json (Portuguese, Brazilian) [Jamie]

- New translations en.json (Portuguese, Brazilian) [Jamie]

- New translations en.json (Portuguese, Brazilian) [Jamie]

- Fixed all linting. [TidusJar]

- Comment out envparam stuff. [Matt Jeanes]

- Fixed prod build issue. [Matt Jeanes]

- Missed a tiny bit. [Matt Jeanes]

- Fix test. [Matt Jeanes]

- Fix test build. [Matt Jeanes]

- Linting + remove debug. [Matt Jeanes]

- Switch to Yarn and disable auto publish in release mode. [Matt Jeanes]

- Fix for #2409. [TidusJar]

- New translations en.json (Swedish) [Jamie]

- New translations en.json (Spanish) [Jamie]

- New translations en.json (Portuguese, Brazilian) [Jamie]

- New translations en.json (Polish) [Jamie]

- New translations en.json (Norwegian) [Jamie]

- New translations en.json (Italian) [Jamie]

- New translations en.json (German) [Jamie]

- New translations en.json (French) [Jamie]

- New translations en.json (Dutch) [Jamie]

- New translations en.json (Danish) [Jamie]

- Possible fix for #2298. [D34DC3N73R]

- Fixed the text for #2370. [Jamie]

- Fixed where you couldn't bulk edit the limits to 0 #2318. [Jamie]

- Upgraded to .net 2.1.2 (Includes security fixes) [Jamie]


## v3.0.3477 (2018-07-18)

### **New Features**

- Updated the Emby availability checker to bring it more in line with what we do with Plex. [TidusJar]

- Added the ability to impersonate a user when using the API Key. This allows people to use the API and request as a certain user. #2363. [Jamie Rees]

- Added more background images and it will loop through the available ones. [Jamie Rees]

- Added chunk hashing to resolve #2330. [Jamie Rees]

- Added API at /api/v1/status/info to get branch and version information #2331. [Jamie Rees]

- Update to .net 2.1.1. [Jamie]

### **Fixes**

- Fix #2322 caused by continue statement inside try catch block. [Anojh]

- Fixed #2367. [TidusJar]

- Fixed the issue where you could not delete a user #2365. [TidusJar]

- Another attempt to fix #2366. [Jamie Rees]

- Fixed the Plex OAuth warning. [Jamie]

- Revert "Fixed Plex OAuth, should no longer show Insecure warning" [Jamie Rees]

- Fixed Plex OAuth, should no longer show Insecure warning. [Jamie Rees]

- Fixed the View On Emby URL since the Link changed #2368. [Jamie Rees]

- Fixed the issue where episodes were not being marked as available in the search #2367. [Jamie Rees]

- Fixed #2371. [Jamie Rees]

- Fixed collection issues in Emby #2366. [Jamie Rees]

- Do not delete the Emby Information every time we run, let's keep the content now. [Jamie Rees]

- Emby Improvements: Batch up the amount we get from the server. [Jamie Rees]

- Log errors when they are uncaught. [Jamie Rees]

- Fix unclosed table tags causing overflow #2322. [Anojh]

- This should now fix #2350. [Jamie]

- Improve the validation around the Application URL. [Jamie Rees]

- Fixed #2341. [Jamie Rees]

- Stop spamming errors when FanArt doesn't have the image. [Jamie Rees]

- Fixed #2338. [Jamie Rees]

- Removed some logging statements. [Jamie Rees]

- Fixed the api key being case sensative #2350. [Jamie Rees]

- Improved the Emby API #2230 Thanks Luke! [Jamie Rees]

- Revert. [Jamie Rees]

- Fixed a small error in the Mobile Notification Provider. [Jamie Rees]

- Minor style tweaks. [Randall Bruder]

- Downgrade to .net core 2.0. [Jamie Rees]

- Downgrade Microsoft.AspNetCore.All package back to 2.0.8. [Jamie Rees]

- Removed old code. [Jamie Rees]

- Swap out the old way of validating the API key with a real middlewear this time. [Jamie Rees]


## v3.0.3421 (2018-06-23)

### **New Features**

- Added TVRequestsLite. [Jamie]

- Added a smaller and simplier way of getting TV Request info. [Jamie Rees]

### **Fixes**

- Show the popular movies and tv shows by default. [Jamie]

- Fixed #2348. [Jamie]


## v3.0.3407 (2018-06-18)

### **New Features**

- Update appveyor.yml. [Jamie]

- Update build.cake. [Jamie]

### **Fixes**

- Fixed the issue where when we find an episode for the recently added sync, we don't check if we should run the availbility checker. [Jamie]

- Fixed the API not working due to a bug in .Net Core 2.1. [Jamie]

- Fixed #2321. [Jamie]

- Maybe this will fix #2298. [Jamie]

- Fixed #2312. [Jamie]

- Fixed the SickRage/Medusa Issue where it was always being set as Skipped/Ignore #2084. [Jamie]

- Fixed the sorting and filtering on the Movie Requests page, it all functions correctly now. [Jamie]

- Fixed #2288. [Jamie]

- Upgrade packages. [Jamie]

- Inital Migration. [Jamie]

- Fixed #2317. [Jamie]


## v3.0.3383 (2018-06-07)

### **New Features**

- Update CHANGELOG.md. [Jamie]

### **Fixes**

- Minor improvements. [Jamie]

- Run the availability checker on finish of the recentlty added sync. [Jamie]

- Fixed the issue with the Recently Added Sync sometimes not working as expected. [Jamie]

- The UI looks at the local time to see if the JWT token has expired. Use local time to generate the token. [Jamie Rees]


## v3.0.3368 (2018-06-03)

### **New Features**

- Added the subscribe on the sarch page. [Jamie Rees]

- Added the subscribe button to the search page if we have an existing request. [Jamie Rees]

- Update CHANGELOG.md. [Jamie]

### **Fixes**

- Use selected episodes in submitRequest. [Calvin]

- Fixed where the test button wouldn't work on the mobile notifications page. [Jamie]

- Fixed the sorting and filtering on the Movie Requests page, it all functions correctly now. [Jamie]

- Fixed #2288. [Jamie]

- Fixed the SickRage/Medusa Issue where it was always being set as Skipped/Ignore #2084. [Jamie]

- Fixed UI typo refrencing discord in mattermost notifications #2175. [Anojh]

- Fix #2175. [Anojh]

- Fixed #2013. [Jamie Rees]

- Fixed #2147. [Jamie Rees]


## v3.0.3346 (2018-05-26)

### **New Features**

- Added a default set of root folders and qualities for Anime in Sonarr. [Jamie Rees]

### **Fixes**

- Made the Open on Mobile link less hidden. [Jamie Rees]

- Fixed #2263. [Jamie Rees]

- !changelog. [Jamie Rees]

- Fixed #2243 The refresh metadata was being run everytime we launched Ombi... [Jamie]

- Fixed a issue where the Plex Content Sync wouldn't pick up new shows #2276 #2244 #2261. [Jamie]

- Sort TvRequests by latest request. [Joe Harvey]

- Fixed build. [Jamie Rees]

- Fix newsletter card background overflow when only one item is available. [Anojh]

- Fix #1745. [Anojh]


## v3.0.3330 (2018-05-17)

### **New Features**

- Added the test button for mobile notifications. [Jamie Rees]

- Added classes to donation html elements. [Anojh]

### **Fixes**

- !changelog. [Jamie Rees]

- Fixed #2257. [Jamie Rees]

- Improved the way we sync the plex content and then get the metadata. #2243. [Jamie Rees]

- Fixed the issue when enabling the Hide Request Users included system users e.g. API key user #2232. [Jamie Rees]

- Removed the test button from the mobile screen since it did nada. [Jamie Rees]

- Finished adding subscriptions for TV Shows. [Jamie Rees]

- Fix #2167. [Anojh]

- Fix #2228. [Anojh]

- Enhanced newsletter styling to support more mail clients. [Anojh]

- Fix #2246. [Anojh]

- Fix #2234. [Anojh]

- Fixed that sometimes there would be a hidden error on the login page. [Jamie Rees]


## v3.0.3304 (2018-05-09)

### **New Features**

- Updated to prevent security vulnerability as noted here: https://github.com/aspnet/Announcements/issues/300. [Jamie Rees]

- Update README.md. [Jamie]

### **Fixes**

- [LC] - Added classes to root/quality override divs. [Anojh]

- Fixed an issue where sometimes the OAuth wouldn't work when loading the login page. [Jamie Rees]

- Alwats enable mobile link. [Jamie]


## v3.0.3293 (2018-05-05)

### **New Features**

- Added a check for long movie descriptions and dealt with accordingly. [Anojh]

- Update jobs.component.html. [D34DC3N73R]

- Added id to emby button to distinguish for UI purposes. [Anojh]

- Changed theme content textarea to use monospace font. [Anojh]

- Added classes and ids to issue status. [Anojh]

- Changed overlay picture to poster pic so we have fallback styling on older clients. [Anojh]

### **Fixes**

- Fixed #2224. [Jamie]

- More robust check for release date. [Anojh]

- Fixed duplicate titles in Plex Newsletter. [Anojh]

- Fixed the filter on the Requests page #2219 and added the default sort to be most recent requests. [Jamie Rees]

- Enable the mobile ntoifications inside Ombi. [Jamie Rees]

- Made the episode list in the newsletter easier to read. Rather than 1,2,3,4,5,10 we will now show 1-5, 10. [Jamie Rees]

- Moved the RecentlyAddedSync into it's own job, it still is calls the regular sync but this should make it easier to start the job from the UI (When I add that) [Jamie Rees]

- Made a massive improvement on the Smaller more frequent Plex Job. This should pick up content a lot quicker now and also get their metadata a lot quicker. [Jamie Rees]

- Trigger a metadata refresh when we finish scanning the libraries. [Jamie Rees]

- Fixed a potential issue in the newsletter where it wouldn't send content due to missing metadata, but would mark it as if it was sent. [Jamie Rees]

- Fixed settings retaining active class when elsewhere in UI. [Anojh]

- Separated user and subject details into spans and fixed styling. [Anojh]

- Fixed linting errors. [Anojh]

- Fixed settings nav item not retaining active class when in other tabs in the settings page. [Anojh]

- Separated reported by and subject and added classes. [Anojh]

- Fix for issue #2152. [Anojh]

- Fix genres being ambigious error. [Anojh]

- Made text style justified. [Anojh]

- V1.0, needs TV background and needs styles for outlook. [Anojh]

- CSS done for the template. [Anojh]

- Fixing some format issues. [Anojh]

- Newsletter template structure done. [Anojh]


## v3.0.3268 (2018-04-28)

### **Fixes**

- Potential fix for #2119. [Jamie Rees]

- Use the Application URL if we have it to fix #2201. [Jamie]


## v3.0.3239 (2018-04-26)

### **New Features**

- Update appveyor.yml. [Jamie]

- Added paging to the TV Requests page. [Jamie Rees]

- Added Paging to the Movie Requests Page. [Jamie Rees]

- Updated Mailkit dependancy. [Jamie Rees]

- Update Hangfire, Newtonsoft and Swagger. [Jamie Rees]

- Added View on Emby Button (#2173) [Anojh Thayaparan]

- Added background property to tvrequests API (#2172) [Anojh Thayaparan]

### **Fixes**

- Clean up the error code when the OAuth user is not authorized. [Jamie]

- More improvements to the Plex OAuth, Added the ability to turn it off if needed. [Jamie]

- Fixed bug #2188 #2134. [Jamie]

- Fixed the bug where only showing API User #2187. [Jamie]

- Detect if baseurl is already set, and reset the link. [Anojh]

- Fixed #2164. [Jamie Rees]

- Fixed #2151. [Jamie Rees]

- Fixed #2170. [Jamie Rees]

- Fixed the newsletter not sending #2134. [Jamie Rees]

- Fix baseurl breaking themes. [Anojh]

- Inject base url if set before theme file url, see issue #1795. [Anojh]

- Sign In rather than Login/Continue. [Avi]

- Fixed #2179. [Jamie Rees]

- Fixed #2169. [Jamie Rees]

- Knocking out LC requirements in issue #2124 (#2125) [Anojh Thayaparan]

- Inject base url if set before theme file url, see issue #1795 (#2148) [Anojh Thayaparan]


## v3.0.3185 (2018-04-16)

### **New Features**

- Added a new Job. Plex Recently Added, this is a slimmed down version of the Plex Sync job, this will just scan the recently added list and not the whole library. I'd reccomend running this very regulary and the full scan not as regular. [Jamie]

### **Fixes**

- Add web-app-capable for IOS and Android. [Thomas]

- Fixed the bug where the newsletter CRON was not appearing on the job settings page. [Jamie]

- Add base url as a startup argument  #2153. [Jamie Rees]

- Fixed a bug with the RefreshMetadata where we would never get TheMovieDBId's if it was missing it. [Jamie]


## v3.0.3173 (2018-04-12)

### **Fixes**

- Removed some early disposition that seemed to be causing errors in the API. [Jamie]


## v3.0.3164 (2018-04-10)

### **New Features**

- Added the ability to send newsletter out to users that are not in Ombi. [Jamie]

- Added the ability to turn off TV or Movies from the newsletter. [Jamie]

- Update about.component.html. [Jamie]

- Update about.component.html. [Jamie]

- Added random versioning prefix to the translations so the users don't have to clear the cache. [Jamie]

- Added more information to the about page. [Jamie]

- Changed let to const to adhere to linting. [Anojh]

- Update _Layout.cshtml. [goldenpipes]

- Update _Layout.cshtml. [goldenpipes]

- Changed the TV Request API. We now only require the TvDbId and the seasons and episodes that you want to request. This should make integration regarding TV a lot easier. [Jamie]

### **Fixes**

- Emby improvments on the way we sync/cache the data. [Jamie]

- Memory improvements. [Jamie]

- Made some improvements to the Sonarr Sync job #2127. [Jamie]

- Turn off Server GC to hopefully help with #2127. [Jamie Rees]

- Fixed #2109. [Jamie]

- Fixed #2101. [Jamie]

- Fixed #2105. [Jamie]

- Fixed some styling on the issues detail page. [Jamie]

- Fixed #2116. [Jamie]

- Limit the amount of FileSystemWatchers being spawned. [Jamie]

- Fixed the issue where Emby connect users could not log in #2115. [Jamie]

- Had to update some base styles since currently some styling does not look right... [Anojh]

- Adding wrappers and classes for LC and toggling active style for UI elements. [Anojh]

- Fixed  a little bug in the newsletter. [Jamie]

- Fixed the issue where movies were not appearing in the newsletter for users with Emby #2111. [Jamie]

- The fact that this button has another style really bothers me. [Louis Laureys]

- Fix discord current user count. [Avi]

- Fix broken images and new discord invite. [Avi]


## v3.0.3111 (2018-03-27)

### **New Features**

- Added the Recently Added Newsletter! You are welcome. [tidusjar]

- Added a new scrollbar to Ombi. [tidusjar]

- Added the ability to automatically generate the API Key on startup if it does not exist #2070. [tidusjar]

- Updated npm dependancies. [Jamie]

- Update README.md. [Jamie]

- Update README.md. [Jamie]

- Update ISSUE_TEMPLATE.md. [Jamie]

- Update appveyor.yml. [Jamie]

- Added recently added stuff. [Jamie]

- Added the recently added engine with some basic methods. [Jamie]

- Added the ability to refresh out backend metadata (#2078) [Jamie]

- Update README.md. [zobe123]

### **Fixes**

- Specific favicons for different platforms. [louis-lau]

- MovieDbId was switched to string fron number so accomodated for change. [Anojh]

- Removing duplicate functions. [Anojh Thayaparan]

- Conflict resolving and adopting Jamie's new method. [Anojh]

- Wrote new calls to just get poster and bg. [Anojh]

- Fix for issue #1907, which is to add content poster and bg to issue details page. [Anojh]

- Dynamic Background Animation. [Anojh]

- Improved the message for #2037. [tidusjar]

- Improved the way we use the notification variables, we have now split out the Username and Alias (Requested User is depricated but not removed) [tidusjar]

- Removed redundant timers. [Anojh]

- More optimizations by reducing requests. [Anojh]

- Improved version. [Anojh]

- Dynamic Background Animation. [Anojh]

- Fixed #2055 and #1903. [Jamie]

- Small changes to the auto updater, let's see how this works. [Jamie]

- Fixed build. [Jamie]

- Fixed the update check for the master build. [Jamie]

- Fixed build. [Jamie]

- Fixed #2074 and #2079. [Jamie]


## v3.0.3030 (2018-03-14)

### **New Features**

- Updated the .Net core dependancies #2072. [Jamie]

### **Fixes**

- Delete Ombi.testdb. [Jamie]


## v3.0.3020 (2018-03-13)

### **Fixes**

- Small memory improvements in the Plex Sync. [Jamie]

- Fixed the sort issue on the user Management page. Also added sorting to the Movie Requests page. [tidusjar]

- Downgraded the angular2-jwt library since it has a bug in it. #2064. [tidusjar]

- Fixed an issue when Plex decideds to reuse the Plex Key for a different media item... #2038. [tidusjar]

- Fixed an issue where we might show the Imdb link when we do not have a imdbid #1797. [tidusjar]

- Fixed the issue where we can no longer select Pending Approval in the filters #2057. [tidusjar]

- Fixed the API key not working when attempting to get requests #2058. [tidusjar]

- Fixed #2056. [tidusjar]

- Experimental, set the Webpack base root to the ombi base path if we have it. This should hopefully fix the reverse proxy issues. [Jamie]

- Fixed #2056. [tidusjar]


## v3.0.3000 (2018-03-09)

### **New Features**

- Added the ability to override root and quality options in Sonarr (#2049) [Jamie]

- Added Pending Approval into the filters list. [tidusjar]

- Added the ability to hide requests that have not been made by that user (#2052) [Jamie]

- Update README.md. [Jamie]

- Update README.md. [Louis Laureys]

### **Fixes**

- Fixed #2042. [Jamie]


## v3.0.0 (2018-03-04)

### **New Features**

- Update build.cake. [Jamie]

- Update README.md. [Jamie]

- Update README.md. [Jamie]

- Added a similar button to the movie searches. Makes movie discoverablility easier. [tidusjar]

- Update README.md. [Jamie]

- Update README.md. [Jamie]

- Update ISSUE_TEMPLATE.md. [Jamie]

- Update appveyor.yml. [Jamie]

- Update ISSUE_TEMPLATE.md. [PotatoQuality]

- Update ISSUE_TEMPLATE.md. [PotatoQuality]

- Update README.md. [Jamie]

- Update README.md. [Jamie]

- Update README.md. [Jamie]

- Update README.md. [PotatoQuality]

- Change the default templates to use {IssueUser} [Jamie]

- Changed the base url validation. [tidusjar]

- Added bulk editing (#1941) [Jamie]

- Change the poster size to w300 #1932. [Jamie]

- Added a default user agent on all API calls. [tidusjar]

- Update request.service.ts. [Jamie]

- Added a filter onto the movies requests page for some inital feedback. [Jamie]

- Added ordering to the User Management screen. [Jamie]

- Update README.md. [Jamie]

- Added custom donation url (#1902) [m4tta]

- Changed the url scheme to make it easier to parse. [Jamie]

- Added Norwegian to the translation code, forgot to check this in. [Jamie]

- Added Norwegian to the language dropdown. [Jamie]

- Added the stuff needed for omBlur. [tidusjar]

- Update README.md (#1872) [xnaas]

- Update README.md. [Jamie]

- Update plex.component.html. [Jamie]

- Change plus to list in menu (#1855) [Louis Laureys]

- Update README.md. [Jamie]

- Update README.md. [Jamie]

- Added user request limits, We can now set the limit for a user. [tidusjar]

- Updated the UI JWT framework. [Jamie]

- Added missing migrations #1744. [Jamie]

- Added the SickRage API integration. [Jamie]

- Update the Emby Connect Username in the user importer. To update the emby connect email address we do it when the user logs in, since the only way to get that information is to use the users Username and Password, since we do not keep this information we cannot do it in the User Importer, but if they have successfully logged in via Emby Connect then we check if we need to update the email address on login. [Jamie]

- Update the connectuserid for emby users. [tidusjar]

- Added the ability to customize job scheudles. [Jamie]

- Update README.md. [Jamie]

- Added the option to import the plex admin. [tidusjar]

- Added the route name as an Id on the container div #1698. [Jamie]

- Updated packages including uglify-es and the package-lock #1683. [Jamie]

- Updated to Angular5 with best practises. [Jamie]

- Update README.md. [Jamie]

- Added the new backgrounds for the requests pages. [tidusjar]

- Added caching to the settings. [tidusjar]

- Added some better handling when adding existing seasons to a tv show in the Plex cacher. [tidusjar]

- Added Telegram Notification support, Not tested. [Jamie]

- Added the new banner background for tv shows. [tidusjar]

- Added a new customization option to provide a css link. [Jamie]

- Update README.md. [Jamie]

- Update README.md. [Jamie]

- Changed to discord link. [tidusjar]

- Added more translations. [Jamie]

- Added more logging for #1643. [tidusjar]

- Update README.md. [Jamie]

- Update Crowdin configuration file. [Jamie]

- Added more translations. [Jamie]

- Update da.json (#1634) [Lixumos]

- Added 32bit version of Ombi. [Jamie]

- Added more translations. [Jamie]

- Added translations. [Jamie]

- Updated the npm packages. [Jamie.Rees]

- Added four new endpoints to change the availability for TV shows and Movies #1601. [Jamie.Rees]

- Added the ability to run a user defined update script #1460. [Jamie.Rees]

- Added logging around creating the wizard user #1604. [tidusjar]

- Added the option to run the content cacher from the settings page, it will no longer get triggered when we press save. [tidusjar]

- Added the ability to specify how many episodes we should cache at a time. #1598. [tidusjar]

- Added usersname and password option for the updater #1460. [Jamie.Rees]

- Changed the way we download the .zip files in the auto updater #1460 This might make a difference to the permissions issue. but not 100% sure. [Jamie.Rees]

- Changed cake. [Jamie.Rees]

- Added feedback when we send a welcome email #1578. [Jamie.Rees]

- Update README.md. [Jamie]

- Added some logging into the PlexCachers and set the log level to informational. [tidusjar]

- Added Couchpotato support and fixed #1548. [tidusjar]

- Update ISSUE_TEMPLATE.md. [PotatoQuality]

- Update ISSUE_TEMPLATE.md. [PotatoQuality]

- Update README.md. [Jamie]

- Added the ability to use a cookie to login. Mainly for Org. [tidusjar]

- Added support for a new TV and Movie provider. DogNZB. [tidusjar]

- Added a PartlyAvailable flag for tv shows. [Jamie.Rees]

- Added some feedback on the request buttons for movies #1536. [Jamie.Rees]

- Added a fix for the poster path issue #1533. [Jamie.Rees]

- Added back the log path writing #865. [Jamie.Rees]

- Update README.md. [Jamie]

- Added support for email login #1513. [tidusjar]

- Added some more logging around the plex user importer fixed the discord notification issue #1513. [tidusjar]

- Update README.md. [PotatoQuality]

- Update README.md. [PotatoQuality]

- Update README.md. [PotatoQuality]

- Update README.md. [PotatoQuality]

- Update README.md. [PotatoQuality]

- Update Readme for V3. [PotatoQuality]

- Added some wiki pages and also made it so we cannot edit Plex Users email address (since they will get overwrote via the Importer) #865. [Jamie.Rees]

- Added transparency to icon files (#1520) [Auwen]

- Added an application URL in the customization settings #1513. [Jamie.Rees]

- Update ISSUE_TEMPLATE.md. [Jamie]

- Added the ability to enable Plex User importing. We also allow you to exclude users #1456. [tidusjar]

- Update Startup.cs. [Jamie]

- Added an about page #865. [Jamie.Rees]

- Changelog. [Jamie.Rees]

- Changed the way we download the updates #865. [Jamie.Rees]

- Updated packages and more logging. [Jamie.Rees]

- Update versioning. [Jamie.Rees]

- Update nuget packages and added logging to the Updater #865. [tidusjar]

- Added the ForwardedHeaders middlewear for Reverse Proxy scenarios #865. [tidusjar]

- Update build.cake. [Jamie]

- Update DiscordApi.cs. [Jamie]

- Update README.md. [PotatoQuality]

- Added a authorization filter so we can see hangfire outisde of the local requests. [TidusJar]

- Added more logging for the updater. [Jamie.Rees]

- Added the emby episode cacher and the job to check if items are available on emby #1464 #865. [tidusjar]

- Added the Emby Cacher, we now cache the Emby data! [tidusjar]

- Updated CHangelog. [Jamie.Rees]

- Updated changelog. [Jamie.Rees]

- Updated assembly versions. [Jamie.Rees]

- Added the logo in the email notifications to use the application image #1459. [Jamie.Rees]

- Change Os to VS2015. [Jamie.Rees]

- Added multiple emby server support and enabled it for Plex #865. [tidusjar]

- Update ISSUE_TEMPLATE.md. [Jamie]

- Update README.md. [Jamie]

- Added slack #1459 #865. [Jamie.Rees]

- Added a checkbox to the usermanagement screen.. Does nothing yet #865 #1456. [Jamie.Rees]

- Update build.cake. [Jamie]

- Added swagger into the .zips. [Jamie.Rees]

- Added Cake build #865. [Jamie.Rees]

- Added Pushbullet notifications #1459 #865. [Jamie.Rees]

- Update README.md. [Jamie]

- Update README.md. [Jamie]

- Update README.md. [Jamie]

- Added the logging endpoint at /api/v1/Logging for the UI logs #1465. [tidusjar]

- Change the RID. [Jamie.Rees]

- Update README.md. [Jamie]

- Update README.md. [Jamie]

- Updated Changelog. [Jamie.Rees]

- Added changelog. [Jamie.Rees]

- Update README.md. [Jamie]

- Updated stuff. [Jamie.Rees]

- Update ISSUE_TEMPLATE.md. [PotatoQuality]

- Update ISSUE_TEMPLATE.md. [PotatoQuality]

- Added the Ombi or customized logo onto the login page #865. [TidusJar]

- Added new login page #865. [Jamie]

- Added Migrations rather than a manual DB Script #865. [Jamie.Rees]

- Updated all the packages. [tidusjar]

- Added a skip options #865. [tidusjar]

- Added the minimumAvailability #865. [tidusjar]

- Updater build script. [Jamie.Rees]

- Added the testing notifications and discord notification #865. [Jamie.Rees]

- Added tests into the build. [Jamie.Rees]

- Added Swagger. [Jamie.Rees]

- Added Discord notification #865. [Jamie.Rees]

- Added the Movie Sender, Movies will be sent to Radarr now #865. [Jamie.Rees]

- Added SQLite storage for Hangfire. [Jamie.Rees]

- Added the Radarr cacher #865. [tidusjar]

- Added Radarr and Sonarr settings pages #865. [Jamie.Rees]

- Update appveyor.yml. [Jamie]

- Update appveyor.yml. [Jamie]

- Update appveyor.yml. [Jamie]

- Update appveyor.yml. [Jamie]

- Updated build script. [tidusjar]

- Added the Rules Engine Pattern and the Auto approve and request rules #865. [tidusjar]

- Update .gitattributes. [Jamie]

- Added the TraktApi back. [tidusjar]

- Changes. [Jamie.Rees]

- Added some sonarr stuff. [Jamie.Rees]

- Added Hangfire #865. [tidusjar]

- Update node again... [Jamie.Rees]

- Update node. [Jamie.Rees]

- Changes. [Jamie.Rees]

- Update StringCipher.cs. [Jamie]

### **Fixes**

- New translations en.json (Norwegian) (#2020) [Jamie]

- Publish 32bit build of windows. [tidusjar]

- Fixing incorrect filter translation targets (#1987) [Jono Cairns]

- New Crowdin translations (#2017) [Jamie]

- Fixed #1997. [tidusjar]

- We now show the digital release date in the search if available #1962. [tidusjar]

- Css fixes (#2014) [Louis Laureys]

- API improvements. [Jamie]

- Fix #1599 (#2008) [Louis Laureys]

- Issue button fix (#2006) [Louis Laureys]

- Fixed #1886 #1865. [Jamie]

- Fixed the outstanding issue on #1995. [Jamie]

- Fixed an issue for #1951. [tidusjar]

- Try and fuzzy match the title and release if we cannot get the tvdb id or imdbid (depends on the media agents in Plex) #1951. [tidusjar]

- Fixed #1989 #1719. [Jamie]

- Small changes that might fix #1985 but doubt it. [Jamie]

- Should fix #1975. [tidusjar]

- Fixed #1789. [tidusjar]

- Fixed #1968. [tidusjar]

- Fixed #1978. [tidusjar]

- Fixed #1954. [tidusjar]

- Small changes to the auto updater, let's see how this works. [Jamie]

- Fixed build. [Jamie]

- Fixed the update check for the master build. [Jamie]

- Removed accidently merged files. [Jamie]

- Create CODE_OF_CONDUCT.md. [Jamie]

- Windows installation guide link update. [PotatoQuality]

- Fixed the issue comment issue #1914 also added another variable for issues {IssueUser} which is the user that reported the issue. [Jamie]

- Fix #1914. [tidusjar]

- Fixed #1914. [tidusjar]

- Fixed build and added logging. [TidusJar]

- New Crowdin translations (#1934) [Jamie]

- Potential fix for #1942. [Jamie]

- Quick change to the Emby Availability rule to make it in line slightly with the Plex one. #1950. [Jamie]

- Turn off mobile notifications. [tidusjar]

- FIXED PLEX!!!!! [tidusjar]

- Batch the PlexContentSync and increase the plex episode batch size. [tidusjar]

- Fixed the migration issue, it's too difficult to migrate the tables. [tidusjar]

- Fixed #1942. [tidusjar]

- Fixed checkboxes style. [Jamie]

- These are not the droids you are looking for. [Jamie]

- Fixed the wrong translation and see if we can VACUUM the db. [tidusjar]

- More translations and added a check on the baseurl to ensure it starts with a '/' [Jamie]

- More translations. [Jamie]

- Fixed #1878 and added a Request all button when selecting episodes. [Jamie]

- New translations en.json (Dutch) [Jamie]

- New translations en.json (German) [Jamie]

- New translations en.json (German) [Jamie]

- New translations en.json (Danish) [Jamie]

- New translations en.json (Norwegian) [Jamie]

- New translations en.json (Norwegian) [Jamie]

- New translations en.json (Norwegian) [Jamie]

- New translations en.json (Italian) [Jamie]

- New translations en.json (German) [Jamie]

- New translations en.json (Dutch) [Jamie]

- New translations en.json (Swedish) [Jamie]

- New translations en.json (Spanish) [Jamie]

- New translations en.json (French) [Jamie]

- New translations en.json (Danish) [Jamie]

- New translations en.json (Norwegian) [Jamie]

- New translations en.json (Italian) [Jamie]

- New translations en.json (German) [Jamie]

- New translations en.json (Dutch) [Jamie]

- New translations en.json (Swedish) [Jamie]

- New translations en.json (Spanish) [Jamie]

- New translations en.json (French) [Jamie]

- New translations en.json (Danish) [Jamie]

- New translations en.json (Danish) [Jamie]

- New translations en.json (Norwegian) [Jamie]

- New translations en.json (Italian) [Jamie]

- New translations en.json (German) [Jamie]

- New translations en.json (Dutch) [Jamie]

- New translations en.json (Swedish) [Jamie]

- New translations en.json (Spanish) [Jamie]

- New translations en.json (French) [Jamie]

- New translations en.json (Danish) [Jamie]

- New translations en.json (Swedish) [Jamie]

- New translations en.json (Norwegian) [Jamie]

- Working on the movie matching. Stop dupes #1869. [tidusjar]

- Delete plex episodes on every run due to a bug, need to spend quite a bit of time on this. [tidusjar]

- Fixed the issue where we were always adding emby episodes. Also fixed #1933. [tidusjar]

- New Crowdin translations (#1906) [Jamie]

- Add plain password for emby login (#1925) [dorian ALKOUM]

- Fixed #1924. [Jamie]

- Fixed the issue where I knocked out the ordering of notifications, oops. [tidusjar]

- #1914 for the issue resolved notification. [Jamie]

- #1916. [Jamie]

- Remove the placeholder. [Jamie]

- Feature arm (#1909) [Jamie]

- New Crowdin translations (#1897) [Jamie]

- Fix logo cut off on login screen (#1896) [Louis Laureys]

- E-Mails: Only add poster table row if img is set (#1899) [Louis Laureys]

- New Crowdin translations (#1884) [Jamie]

- Fix mobile layout (#1888) [Louis Laureys]

- Smal changes to the api. [tidusjar]

- OmBlur. [tidusjar]

- Hide the password field if it's not needed #1815. [Jamie]

- Should fix #1885. [Jamie]

- Make user management table responsive (#1882) [Louis Laureys]

- Fixed some stuff for omBlur. [Jamie]

- Some work... No one take a look at this, it's a suprise. [Jamie]

- New Crowdin translations (#1858) [Jamie]

- When requesting Anime, we now mark it correctly as Anime in Sonarr. [tidusjar]

- Fixed #1879 and added the spans. [tidusjar]

- Some work on the auto updater #1460. [tidusjar]

- Removed the potential locking. [tidusjar]

- Fixed #1863. [tidusjar]

- Moved the update check code from the External azure service into Ombi at /api/v1/update/BRANCH. [Jamie]

- Fixed the UI erroring out, also dont show tv with no externals. [tidusjar]

- More memory management and improvements. [tidusjar]

- These are not needed, added accidentally (#1860) [Louis Laureys]

- Some memory management improvements. [tidusjar]

- Fixed #1857. [tidusjar]

- Delete old v2 ombi from v3 branch. [tidusjar]

- New Crowdin translations (#1840) [Jamie]

- Better login backgrounds! (#1852) [Louis Laureys]

- Fixed #1851. [tidusjar]

- Fixed #1826. [tidusjar]

- Redo change #1848. [tidusjar]

- Fix the issue for welcome emails not sending. [tidusjar]

- Fix typo (#1845) [Kyle Lucy]

- Fix user mentions in Slack notifications (#1846) [Aljosa Asanovic]

- If Radarr/Sonarr has noticed that the media is available, then mark it as available in the UI. [Jamie]

- Fixed #1835. [Jamie]

- Enable Multi MIME and add alt tags to images (#1838) [Louis Laureys]

- New Crowdin translations (#1816) [Jamie]

- Fixed #1832. [tidusjar]

- Switch to use a single HTTPClient rather than a new one every request !dev. [tidusjar]

