# Changelog

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

