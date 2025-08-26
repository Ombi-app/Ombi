#!/bin/bash

# Script to update all using statements in the Core project to use the new consolidated namespaces

echo "Updating using statements in Core project..."

# Update MediaServer namespaces
echo "Updating MediaServer namespaces..."

# Emby
find Ombi.Core -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Emby/Ombi.Api.External.MediaServers.Emby/g' {} \;

# Jellyfin  
find Ombi.Core -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Jellyfin/Ombi.Api.External.MediaServers.Jellyfin/g' {} \;

# Plex
find Ombi.Core -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Plex/Ombi.Api.External.MediaServers.Plex/g' {} \;

echo "Updating ExternalApis namespaces..."

# External APIs
find Ombi.Core -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.TheMovieDb/Ombi.Api.External.ExternalApis.TheMovieDb/g' {} \;
find Ombi.Core -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.TvMaze/Ombi.Api.External.ExternalApis.TvMaze/g' {} \;
find Ombi.Core -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Trakt/Ombi.Api.External.ExternalApis.Trakt/g' {} \;
find Ombi.Core -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.FanartTv/Ombi.Api.External.ExternalApis.FanartTv/g' {} \;
find Ombi.Core -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Lidarr/Ombi.Api.External.ExternalApis.Lidarr/g' {} \;
find Ombi.Core -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.MusicBrainz/Ombi.Api.External.ExternalApis.MusicBrainz/g' {} \;
find Ombi.Core -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Radarr/Ombi.Api.External.ExternalApis.Radarr/g' {} \;
find Ombi.Core -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Sonarr/Ombi.Api.External.ExternalApis.Sonarr/g' {} \;
find Ombi.Core -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.SickRage/Ombi.Api.External.ExternalApis.SickRage/g' {} \;
find Ombi.Core -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.CouchPotato/Ombi.Api.External.ExternalApis.CouchPotato/g' {} \;
find Ombi.Core -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.DogNzb/Ombi.Api.External.ExternalApis.DogNzb/g' {} \;

echo "Using statement updates complete!"
