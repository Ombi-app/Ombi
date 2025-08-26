#!/bin/bash
# Corrected script to update all using statements in the DependencyInjection project
echo "Updating using statements in DependencyInjection project with correct categorization..."

# Update MediaServer namespaces
echo "Updating MediaServer namespaces..."
# Emby
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Emby/Ombi.Api.External.MediaServers.Emby/g' {} \;
# Jellyfin
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Jellyfin/Ombi.Api.External.MediaServers.Jellyfin/g' {} \;
# Plex
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Plex/Ombi.Api.External.MediaServers.Plex/g' {} \;

# Update ExternalApis namespaces
echo "Updating ExternalApis namespaces..."
# CouchPotato
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.CouchPotato/Ombi.Api.External.ExternalApis.CouchPotato/g' {} \;
# DogNzb
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.DogNzb/Ombi.Api.External.ExternalApis.DogNzb/g' {} \;
# FanartTv
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.FanartTv/Ombi.Api.External.ExternalApis.FanartTv/g' {} \;
# Github
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Github/Ombi.Api.External.ExternalApis.Github/g' {} \;
# Lidarr
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Lidarr/Ombi.Api.External.ExternalApis.Lidarr/g' {} \;
# SickRage
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.SickRage/Ombi.Api.External.ExternalApis.SickRage/g' {} \;
# MusicBrainz
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.MusicBrainz/Ombi.Api.External.ExternalApis.MusicBrainz/g' {} \;
# RottenTomatoes
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.RottenTomatoes/Ombi.Api.External.ExternalApis.RottenTomatoes/g' {} \;
# TheMovieDb
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.TheMovieDb/Ombi.Api.External.ExternalApis.TheMovieDb/g' {} \;
# Trakt
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Trakt/Ombi.Api.External.ExternalApis.Trakt/g' {} \;
# TvMaze
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.TvMaze/Ombi.Api.External.ExternalApis.TvMaze/g' {} \;
# Radarr
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Radarr/Ombi.Api.External.ExternalApis.Radarr/g' {} \;
# Sonarr
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Sonarr/Ombi.Api.External.ExternalApis.Sonarr/g' {} \;
# Service
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Service/Ombi.Api.External.ExternalApis.Service/g' {} \;

# Update NotificationServices namespaces
echo "Updating NotificationServices namespaces..."
# Discord
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Discord/Ombi.Api.External.NotificationServices.Discord/g' {} \;
# Mattermost
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Mattermost/Ombi.Api.External.NotificationServices.Mattermost/g' {} \;
# Notifications
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Notifications/Ombi.Api.External.NotificationServices.Notifications/g' {} \;
# Pushbullet
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Pushbullet/Ombi.Api.External.NotificationServices.Pushbullet/g' {} \;
# Pushover
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Pushover/Ombi.Api.External.NotificationServices.Pushover/g' {} \;
# Gotify
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Gotify/Ombi.Api.External.NotificationServices.Gotify/g' {} \;
# GroupMe
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.GroupMe/Ombi.Api.External.NotificationServices.GroupMe/g' {} \;
# Webhook
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Webhook/Ombi.Api.External.NotificationServices.Webhook/g' {} \;
# Slack
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Slack/Ombi.Api.External.NotificationServices.Slack/g' {} \;
# Telegram
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Telegram/Ombi.Api.External.NotificationServices.Telegram/g' {} \;
# Twilio
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Twilio/Ombi.Api.External.NotificationServices.Twilio/g' {} \;
# CloudService
find Ombi.DependencyInjection -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.CloudService/Ombi.Api.External.NotificationServices.CloudService/g' {} \;

echo "Corrected using statement updates complete!"
