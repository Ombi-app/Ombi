#!/bin/bash
# Comprehensive script to update all using statements in the HealthChecks project
echo "Updating using statements in HealthChecks project..."

# Update MediaServer namespaces
echo "Updating MediaServer namespaces..."
# Emby
find Ombi.HealthChecks -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Emby/Ombi.Api.External.MediaServers.Emby/g' {} \;
# Jellyfin
find Ombi.HealthChecks -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Jellyfin/Ombi.Api.External.MediaServers.Jellyfin/g' {} \;
# Plex
find Ombi.HealthChecks -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Plex/Ombi.Api.External.MediaServers.Plex/g' {} \;

# Update ExternalApis namespaces
echo "Updating ExternalApis namespaces..."
# CouchPotato
find Ombi.HealthChecks -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.CouchPotato/Ombi.Api.External.ExternalApis.CouchPotato/g' {} \;
# Lidarr
find Ombi.HealthChecks -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Lidarr/Ombi.Api.External.ExternalApis.Lidarr/g' {} \;
# Radarr
find Ombi.HealthChecks -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Radarr/Ombi.Api.External.ExternalApis.Radarr/g' {} \;
# SickRage
find Ombi.HealthChecks -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.SickRage/Ombi.Api.External.ExternalApis.SickRage/g' {} \;
# Sonarr
find Ombi.HealthChecks -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Sonarr/Ombi.Api.External.ExternalApis.Sonarr/g' {} \;

echo "Using statement updates complete!"
