# Plex Requests .NET!

[![Gitter](https://badges.gitter.im/tidusjar/PlexRequest.NET.svg)](https://gitter.im/tidusjar/PlexRequests.Net?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)
[![Build status](https://ci.appveyor.com/api/projects/status/hgj8j6lcea7j0yhn?svg=true)](https://ci.appveyor.com/project/tidusjar/requestplex)
[![Linux Status](https://travis-ci.org/tidusjar/PlexRequests.Net.svg)](https://travis-ci.org/tidusjar/PlexRequests.Net)
[![Average time to resolve an issue](http://isitmaintained.com/badge/resolution/tidusjar/plexrequests.net.svg)](http://isitmaintained.com/project/tidusjar/plexrequests.net "Average time to resolve an issue")
[![Percentage of issues still open](http://isitmaintained.com/badge/open/tidusjar/plexrequests.net.svg)](http://isitmaintained.com/project/tidusjar/plexrequests.net "Percentage of issues still open")
[![Github All Releases](https://img.shields.io/github/downloads/tidusjar/PlexRequests.net/total.svg)](https://github.com/tidusjar/PlexRequests.Net)

This is based off [Plex Requests by lokenx](https://github.com/lokenx/plexrequests-meteor) so big props to that guy!
I wanted to write a similar application in .Net!

#Features

* Integration with [TheMovieDB](https://www.themoviedb.org/) for all Movies
* Integration with [TVMaze](www.tvmaze.com) for all TV shows!
* Secure authentication
* [Sonarr](https://sonarr.tv/) integration (SickRage/Sickbeard TBD)
* [CouchPotato](https://couchpota.to/) integration
* [SickRage](https://sickrage.github.io/) integration
* Email notifications
* Pushbullet notifications
* Pushover notifications

#Preview

![Preview](http://i.imgur.com/ucCFUvd.gif)

#Installation
Download the latest [Release](https://github.com/tidusjar/PlexRequests.Net/releases).
Extract the .zip file (Unblock if on Windows! Right Click > Properties > Unblock).
Just run `PlexRequests.exe`! (Mono compatible `mono PlexRequests.exe`)

#Configuration

To configure PlexRequests you need to register an admin user by clicking on Admin (top left) and press the Register link.
You will then have a admin menu option once registered where you can setup Sonarr, Couchpotato and any other settings.

Looking for a Docker Image? Well [rogueosb](https://github.com/rogueosb/) has created a docker image for us, You can find it [here](https://github.com/rogueosb/docker-plexrequestsnet) :smile:

#Debian/Ubuntu

To configure PlexRequests to run on debian/ubuntu and set it to start up with the system, do the following (via terminal):

###Create a location to drop the files (up to you, we'll ues /opt/PlexRequests as an example)

```sudo mkdir /opt/PlexRequests```

###Download the release zip
```
sudo wget {release zip file url}
sudo unzip PlexRequests.zip -d /opt/PlexRequests
```

###Install Mono (this app will be used to actually run the .net libraries and executable)

```sudo apt-get install mono-devel```

###Verify Mono properly runs PlexRequests

```sudo /usr/bin/mono /opt/PlexRequests/PlexRequests.exe```

###Create an upstart script to auto-start PlexRequests with your system (using port 80 in this example)

```sudo nano /etc/init/plexrequests.conf```

####Paste in the following:

```
start on runlevel [2345]
stop on runlevel [016]

respawn
expect fork

pre-start script
    # echo ""
end script

script
    exec /usr/bin/mono /opt/PlexRequests/PlexRequests.exe 80
end script
```

###Reboot, then open up your browser to check that it's running!

```sudo shutdown -r 00```

# Contributors

We are looking for any contributions to the project! Just pick up a task, if you have any questions ask and i'll get straight on it!

Please feed free to submit a pull request!

# Donation
If you feel like donating you can [here!](https://paypal.me/PlexRequestsNet)

###### A massive thanks to everyone below!

[heartisall](https://github.com/heartisall), [Stuke00](https://github.com/Stuke00), [shiitake](https://github.com/shiitake)


# Sponsors
- [JetBrains](http://www.jetbrains.com/) for providing us with free licenses to their great tools!!!
    - [ReSharper](http://www.jetbrains.com/resharper/) 
    - [dotTrace] (https://www.jetbrains.com/profiler/) 
    - [dotMemory] (https://www.jetbrains.com/dotmemory/) 
    - [dotCover] (https://www.jetbrains.com/dotcover/) 
