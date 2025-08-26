#!/bin/bash
# Script to update AppVeyor API namespaces
echo "Updating AppVeyor API namespaces..."

# Update AppVeyorApi.cs
echo "Updating AppVeyorApi.cs..."
sed -i '' 's/namespace Ombi\.Api\.Service/namespace Ombi.Api.External.ExternalApis.Service/g' Ombi.Api.External/ExternalApis/Service/AppVeyorApi.cs

# Update IAppVeyorApi.cs
echo "Updating IAppVeyorApi.cs..."
sed -i '' 's/namespace Ombi\.Api\.Service/namespace Ombi.Api.External.ExternalApis.Service/g' Ombi.Api.External/ExternalApis/Service/IAppVeyorApi.cs

echo "AppVeyor API namespace updates complete!"
