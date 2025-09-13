#!/bin/bash

# Script to update all namespaces in the consolidated Ombi.Api.External project

echo "Updating namespaces in Ombi.Api.External project..."

# Update MediaServers namespaces
echo "Updating MediaServers namespaces..."

# Emby
find MediaServers/Emby -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Emby/Ombi.Api.External.MediaServers.Emby/g' {} \;

# Jellyfin  
find MediaServers/Jellyfin -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Jellyfin/Ombi.Api.External.MediaServers.Jellyfin/g' {} \;

# Plex
find MediaServers/Plex -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Plex/Ombi.Api.External.MediaServers.Plex/g' {} \;

echo "Updating ExternalApis namespaces..."

# External APIs
find ExternalApis -name "*.cs" -exec sed -i '' 's/Ombi\.Api\./Ombi.Api.External.ExternalApis./g' {} \;

echo "Updating NotificationServices namespaces..."

# Notification Services
find NotificationServices -name "*.cs" -exec sed -i '' 's/Ombi\.Api\./Ombi.Api.External.NotificationServices./g' {} \;

echo "Namespace update complete!"
