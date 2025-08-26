#!/bin/bash
# Comprehensive script to update all using statements in the Schedule project
echo "Updating using statements in Schedule project..."

# Update MediaServer namespaces
echo "Updating MediaServer namespaces..."
# Emby
find Ombi.Schedule -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Emby/Ombi.Api.External.MediaServers.Emby/g' {} \;
# Jellyfin
find Ombi.Schedule -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Jellyfin/Ombi.Api.External.MediaServers.Jellyfin/g' {} \;
# Plex
find Ombi.Schedule -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Plex/Ombi.Api.External.MediaServers.Plex/g' {} \;

# Update ExternalApis namespaces
echo "Updating ExternalApis namespaces..."
# CouchPotato
find Ombi.Schedule -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.CouchPotato/Ombi.Api.External.ExternalApis.CouchPotato/g' {} \;
# Lidarr
find Ombi.Schedule -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Lidarr/Ombi.Api.External.ExternalApis.Lidarr/g' {} \;
# Radarr
find Ombi.Schedule -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Radarr/Ombi.Api.External.ExternalApis.Radarr/g' {} \;
# SickRage
find Ombi.Schedule -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.SickRage/Ombi.Api.External.ExternalApis.SickRage/g' {} \;
# Sonarr
find Ombi.Schedule -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Sonarr/Ombi.Api.External.ExternalApis.Sonarr/g' {} \;
# TheMovieDb
find Ombi.Schedule -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.TheMovieDb/Ombi.Api.External.ExternalApis.TheMovieDb/g' {} \;
# TvMaze
find Ombi.Schedule -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.TvMaze/Ombi.Api.External.ExternalApis.TvMaze/g' {} \;

# Update Service namespaces
echo "Updating Service namespaces..."
# Service
find Ombi.Schedule -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Service/Ombi.Api.External.ExternalApis.Service/g' {} \;

echo "Using statement updates complete!"
