# Plex Requests .NET!

[![Gitter](https://badges.gitter.im/tidusjar/PlexRequest.NET.svg)](https://gitter.im/tidusjar/PlexRequests.Net?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)
[![Build status](https://ci.appveyor.com/api/projects/status/hgj8j6lcea7j0yhn?svg=true)](https://ci.appveyor.com/project/tidusjar/requestplex)
[![Linux Status](https://travis-ci.org/tidusjar/PlexRequests.Net.svg)](https://travis-ci.org/tidusjar/PlexRequests.Net)
[![Average time to resolve an issue](http://isitmaintained.com/badge/resolution/tidusjar/plexrequests.net.svg)](http://isitmaintained.com/project/tidusjar/plexrequests.net "Average time to resolve an issue")
[![Percentage of issues still open](http://isitmaintained.com/badge/open/tidusjar/plexrequests.net.svg)](http://isitmaintained.com/project/tidusjar/plexrequests.net "Percentage of issues still open")
[![Github All Releases](https://img.shields.io/github/downloads/tidusjar/PlexRequests.net/total.svg)](https://github.com/tidusjar/PlexRequests.Net)

This is based off [Plex Requests by lokenx](https://github.com/lokenx/plexrequests-meteor) so big props to that guy!
I wanted to write a similar application in .Net!

# Features

* Movie and TV Show searching, can't find something on Plex? Just request it! 
* Notifications! Get notified via Email, Pushbullet and Pushover for new requests and issue reports!
* Send your TV Shows to either [Sonarr](https://sonarr.tv/) or [SickRage](http://www.sickrage.ca/)!
* Secure authentication so you don't have to worry about those script kiddies
* We check to see if the request is already in Plex, if it's already in Plex then why you requesting it?!
* We have allowed the ability for a user to add a custom note on a request
* It automatically update the status of requests when they are available on Plex
* Sick, responsive and mobile friendly UI
* Headphones integration will be comming soon!

# Preview (Needs updating)

![Preview](http://i.imgur.com/ucCFUvd.gif)

#Installation
Download the latest [Release](https://github.com/tidusjar/PlexRequests.Net/releases).
Extract the .zip file (Unblock if on Windows! Right Click > Properties > Unblock).
Just run `PlexRequests.exe`! (Mono compatible `mono PlexRequests.exe`)

# FAQ
Do you have an issue or a question? if so check out our [FAQ!](https://github.com/tidusjar/PlexRequests.Net/wiki/FAQ)

# Docker

Looking for a Docker Image? Well [rogueosb](https://github.com/rogueosb/) has created a docker image for us, You can find it [here](https://github.com/rogueosb/docker-plexrequestsnet) :smile:

#Debian/Ubuntu

To configure PlexRequests to run on debian/ubuntu and set it to start up with the system, do the following (via terminal):

####Create a location to drop the files (up to you, we'll use /opt/PlexRequests as an example)

```sudo mkdir /opt/PlexRequests```

####Download the release zip
```
sudo wget {release zip file url}
sudo unzip PlexRequests.zip -d /opt/PlexRequests
```

####Install Mono (must be on v4.x or above for compatibility)

```sudo apt-get install mono-devel```

####Check your Mono version

```sudo mono --version```

if you don't see v4.x or above, uninstall it, and check here for instructions:
http://www.mono-project.com/docs/getting-started/install/linux/

####Verify Mono properly runs PlexRequests

```sudo /usr/bin/mono /opt/PlexRequests/Release/PlexRequests.exe```

####Create an upstart script to auto-start PlexRequests with your system (using port 80 in this example)

```sudo nano /etc/init/plexrequests.conf```

#####Paste in the following:

```
start on runlevel [2345]
stop on runlevel [016]

respawn
expect fork

pre-start script
    # echo ""
end script

script
    exec /usr/bin/mono /opt/PlexRequests/Release/PlexRequests.exe 80
end script
```

####Reboot, then open up your browser to check that it's running!

```
sudo shutdown -r 00
```

# Contributors

We are looking for any contributions to the project! Just pick up a task, if you have any questions ask and i'll get straight on it!

Please feed free to submit a pull request!

# Donation
If you feel like donating you can [here!](https://paypal.me/PlexRequestsNet)

## A massive thanks to everyone below for all their help!

[heartisall](https://github.com/heartisall), [Stuke00](https://github.com/Stuke00), [shiitake](https://github.com/shiitake), [Drewster727](https://github.com/Drewster727), Majawat, [EddiYo](https://github.com/EddiYo)
