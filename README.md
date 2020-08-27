![](http://i.imgur.com/qQsN78U.png)   

**Please note: The new plex movie agent in version 1.20.1.3252 and higher isn't supported yet by Ombi. A Change is planned, but for now we can't sync the availability. **

____ 
[![Discord](https://img.shields.io/discord/270828201473736705.svg)](https://discord.gg/Sa7wNWb)
[![Docker Pulls](https://img.shields.io/docker/pulls/linuxserver/ombi.svg)](https://hub.docker.com/r/linuxserver/ombi/)
[![Github All Releases](https://img.shields.io/github/downloads/tidusjar/Ombi/total.svg)](https://github.com/tidusjar/Ombi)
[![firsttimersonly](http://img.shields.io/badge/first--timers--only-friendly-blue.svg)](http://www.firsttimersonly.com/)
[![Crowdin](https://d322cqt584bo4o.cloudfront.net/ombi/localized.svg)](https://crowdin.com/project/ombi)

[![Patreon](https://img.shields.io/badge/patreon-donate-yellow.svg)](https://patreon.com/tidusjar/Ombi)
[![Paypal](https://img.shields.io/badge/paypal-donate-yellow.svg)](https://paypal.me/PlexRequestsNet)

# Feature Requests
Feature requests are handled on Feature Upvote.

Search the existing requests to see if your suggestion has already been submitted.
(If a similar request exists, please vote, or add additional comments to the request)

#### [![Feature Requests](https://cloud.githubusercontent.com/assets/390379/10127973/045b3a96-6560-11e5-9b20-31a2032956b2.png)](https://features.ombi.io)
___


[![Twitter](https://img.shields.io/twitter/follow/tidusjar.svg?style=social)](https://twitter.com/intent/follow?screen_name=tidusjar)

Follow me developing Ombi!

[![Twitch](https://img.shields.io/badge/Twitch-Watch-blue.svg?style=flat-square&logo=twitch)](https://www.twitch.tv/tidusjar)


___
<a href='https://play.google.com/store/apps/details?id=com.tidusjar.Ombi&pcampaignid=MKT-Other-global-all-co-prtnr-py-PartBadge-Mar2515-1'><img width="150"   alt='Get it on Google Play' src='https://play.google.com/intl/en_gb/badges/images/generic/en_badge_web_generic.png'/></a>
<br>
_**Note:** There is no longer an iOS app due to complications outside of our control._

___

We also now have merch up on Teespring!

[EU Store](https://teespring.com/stores/ombi-eu)   
[US Store](https://teespring.com/stores/ombi-us)

___


| Service  | Stable         | Develop          | V4 |
|----------|:---------------------------:|:----------------------------:|:----------------------------:|
| Build Status | [![Build status](https://ci.appveyor.com/api/projects/status/hgj8j6lcea7j0yhn/branch/master?svg=true)](https://ci.appveyor.com/project/tidusjar/requestplex/branch/master) | [![Build status](https://ci.appveyor.com/api/projects/status/hgj8j6lcea7j0yhn/branch/develop?svg=true)](https://ci.appveyor.com/project/tidusjar/requestplex/branch/develop) | [![Build Status](https://dev.azure.com/tidusjar/Ombi/_apis/build/status/Ombi%20CI?branchName=feature%2Fv4)](https://dev.azure.com/tidusjar/Ombi/_build/latest?definitionId=18&branchName=feature%2Fv4)
| Download |[![Download](http://i.imgur.com/odToka3.png)](https://github.com/tidusjar/Ombi/releases)            |      [![Download](http://i.imgur.com/odToka3.png)](https://ci.appveyor.com/project/tidusjar/requestplex/branch/develop/artifacts)       | [![Download](http://i.imgur.com/odToka3.png)](https://github.com/tidusjar/ombi.releases/releases)       | 
# Features
Here are some of the features Ombi V3 has:
* Now working without crashes on Linux.
* Lets users request Movies, Music, and TV Shows (whether it being the entire series, an entire season, or even single episodes.)
* Easily manage your requests
* Allows you to set specific users to automatically have requests approved and added to the relevant service (Sonarr/Radarr/Lidarr/Couchpotato etc)
* User management system (supports plex.tv, Emby and local accounts)
* A landing page that will give you the availability of your Plex/Emby server and also add custom notification text to inform your users of downtime.
* Allows your users to get custom notifications!
* Secure authentication using best practises
* Will show if the request is already on plex or even if it's already monitored.
* Automatically updates the status of requests when they are available on Plex/Emby
* Slick, responsive and mobile friendly UI
* Ombi will automatically update itself :) (YMMV)
* Very fast!

### Integration 
We integrate with the following applications:
* Plex Media Server
* Emby
* Jellyfin
* Sonarr V2 and V3
* Radarr V2
* Lidarr
* DogNzb
* Couch Potato


### Notifications
Supported notifications:
* Mobile
* SMTP Notifications (Email)
* Discord
* Slack
* Pushbullet
* Pushover
* Mattermost
* Telegram
* Gotify
* Twilio
* Webhook

### The difference between Version 4 and 3

Over the last year, we focused on the main functions on Ombi, a complete rewrite while making it better, faster and more stable.
We have already done most of the work, but some features are still be missing in this first version.
We are planning to bring back these features in V3 but for now you can find a list below with a quick comparison of features between v4 and v3.


| Service  | Version 4 (Beta) | Version 3 (Stable)|
|----------|:----------:|:----------:|
| Multiple Plex/Emby/Jellyfin Servers | Yes | Yes |
| Emby/Jellyfin & Plex support | Yes | Yes |
| Mono dependency | No | No |
| Plex OAuth support | Yes | Yes |
| Login page | Yes (brand new) | Yes |
| Discovery page | Yes (brand new) | No |
| Request a movie collection | Yes (brand new) | No |
| Auto Delete Available Requests | Yes (brand new) | No |
| Report issues | Yes | Yes |
| Notifications support | Yes | Yes |
| Custom Notification Messages | Yes | Yes |
| Sending newsletters | Yes | Yes |
| Send a Mass Email | Yes | Yes |
| SickRage | Yes | Yes |
| CouchPotato | Yes | Yes |
| DogNzb | Yes | Yes |
| Headphones | No | Yes |
| Lidarr | Yes | Yes |

# Preview

![Preview](http://i.imgur.com/Nn1BwAM.gif)

# Installation

[Installation Guide](https://github.com/tidusjar/Ombi/wiki/Installation)  
[Here for Reverse Proxy Config Examples](https://github.com/tidusjar/Ombi/wiki/Reverse-Proxy-Examples)  
[PlexGuide.com - Ombi Deployment & 101 Demonstration!](https://www.youtube.com/watch?v=QPNlqqkjNJw&feature=youtu.be)  

# Contributors

We are looking for any contributions to the project! Just pick up a task, if you have any questions ask and i'll get straight on it!

Please feel free to submit a pull request!

# Donation
If you feel like donating you can donate with the below buttons!


[![Patreon](https://img.shields.io/badge/patreon-donate-yellow.svg)](https://patreon.com/tidusjar/Ombi)
[![Paypal](https://img.shields.io/badge/paypal-donate-yellow.svg)](https://paypal.me/PlexRequestsNet)

### A massive thanks to everyone for all their help!


### Sponsors ###
- [JetBrains](http://www.jetbrains.com/) for providing us with free licenses to their great tools
    - [ReSharper](http://www.jetbrains.com/resharper/)
- [BrowserStack](https://www.browserstack.com) for allowing us to use their platform for testing
