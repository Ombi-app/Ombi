#!/bin/bash
# Comprehensive script to update all using statements in test projects
echo "Updating using statements in test projects..."

# Update MediaServer namespaces
echo "Updating MediaServer namespaces..."
# Emby
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Emby/Ombi.Api.External.MediaServers.Emby/g' {} \;
# Jellyfin
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Jellyfin/Ombi.Api.External.MediaServers.Jellyfin/g' {} \;
# Plex
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Plex/Ombi.Api.External.MediaServers.Plex/g' {} \;

# Update ExternalApis namespaces
echo "Updating ExternalApis namespaces..."
# CouchPotato
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.CouchPotato/Ombi.Api.External.ExternalApis.CouchPotato/g' {} \;
# DogNzb
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.DogNzb/Ombi.Api.External.ExternalApis.DogNzb/g' {} \;
# FanartTv
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.FanartTv/Ombi.Api.External.ExternalApis.FanartTv/g' {} \;
# Github
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Github/Ombi.Api.External.ExternalApis.Github/g' {} \;
# Gotify
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Gotify/Ombi.Api.External.ExternalApis.Gotify/g' {} \;
# GroupMe
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.GroupMe/Ombi.Api.External.ExternalApis.GroupMe/g' {} \;
# Webhook
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Webhook/Ombi.Api.External.ExternalApis.Webhook/g' {} \;
# Lidarr
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Lidarr/Ombi.Api.External.ExternalApis.Lidarr/g' {} \;
# SickRage
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.SickRage/Ombi.Api.External.ExternalApis.SickRage/g' {} \;
# Slack
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Slack/Ombi.Api.External.ExternalApis.Slack/g' {} \;
# Telegram
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Telegram/Ombi.Api.External.ExternalApis.Telegram/g' {} \;
# MusicBrainz
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.MusicBrainz/Ombi.Api.External.ExternalApis.MusicBrainz/g' {} \;
# Twilio
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Twilio/Ombi.Api.External.ExternalApis.Twilio/g' {} \;
# CloudService
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.CloudService/Ombi.Api.External.ExternalApis.CloudService/g' {} \;
# RottenTomatoes
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.RottenTomatoes/Ombi.Api.External.ExternalApis.RottenTomatoes/g' {} \;
# TheMovieDb
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.TheMovieDb/Ombi.Api.External.ExternalApis.TheMovieDb/g' {} \;
# Trakt
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Trakt/Ombi.Api.External.ExternalApis.Trakt/g' {} \;
# TvMaze
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.TvMaze/Ombi.Api.External.ExternalApis.TvMaze/g' {} \;
# Radarr
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Radarr/Ombi.Api.External.ExternalApis.Radarr/g' {} \;
# Sonarr
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Sonarr/Ombi.Api.External.ExternalApis.Sonarr/g' {} \;
# Service
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Service/Ombi.Api.External.ExternalApis.Service/g' {} \;

# Update NotificationServices namespaces
echo "Updating NotificationServices namespaces..."
# Discord
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Discord/Ombi.Api.External.NotificationServices.Discord/g' {} \;
# Mattermost
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Mattermost/Ombi.Api.External.NotificationServices.Mattermost/g' {} \;
# Notifications
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Notifications/Ombi.Api.External.NotificationServices.Notifications/g' {} \;
# Pushbullet
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Pushbullet/Ombi.Api.External.NotificationServices.Pushbullet/g' {} \;
# Pushover
find Ombi.Core.Tests Ombi.Schedule.Tests -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Pushover/Ombi.Api.External.NotificationServices.Pushover/g' {} \;

echo "Test project namespace updates complete!"
