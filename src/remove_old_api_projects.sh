#!/bin/bash

echo "Removing old individual API projects that have been consolidated..."

# Remove all the old API projects
echo "Removing Ombi.Api.* projects..."
rm -rf Ombi.Api.Plex
rm -rf Ombi.Api.RottenTomatoes
rm -rf Ombi.Api.Telegram
rm -rf Ombi.Api.Discord
rm -rf Ombi.Api.Github
rm -rf Ombi.Api.MediaServer
rm -rf Ombi.Api.Service
rm -rf Ombi.Api.FanartTv
rm -rf Ombi.Api.Notifications
rm -rf Ombi.Api.SickRage
rm -rf Ombi.Api.Lidarr
rm -rf Ombi.Api.Slack
rm -rf Ombi.Api.Pushbullet
rm -rf Ombi.Api.Twilio
rm -rf Ombi.Api.Gotify
rm -rf Ombi.Api.Mattermost
rm -rf Ombi.Api.GroupMe
rm -rf Ombi.Api.Pushover
rm -rf Ombi.Api.CouchPotato
rm -rf Ombi.Api.Webhook
rm -rf Ombi.Api
rm -rf Ombi.Api.MusicBrainz
rm -rf Ombi.Api.Emby
rm -rf Ombi.Api.DogNzb
rm -rf Ombi.Api.TvMaze
rm -rf Ombi.Api.Sonarr
rm -rf Ombi.Api.Trakt
rm -rf Ombi.Api.Jellyfin
rm -rf Ombi.Api.CloudService
rm -rf Ombi.Api.Radarr

# Remove the old TheMovieDbApi project (consolidated into ExternalApis)
rm -rf Ombi.TheMovieDbApi

echo "Old API projects removed successfully!"
echo "Note: Ombi.Api.External now contains all the consolidated API code."
