#!/bin/bash
# Script to update namespaces in newly copied API files
echo "Updating namespaces in newly copied API files..."

# Update OmbiService namespaces
echo "Updating OmbiService namespaces..."
sed -i '' 's/namespace Ombi\.Api\.Service/namespace Ombi.Api.External.ExternalApis.Service/g' Ombi.Api.External/ExternalApis/Service/IOmbiService.cs
sed -i '' 's/namespace Ombi\.Api\.Service/namespace Ombi.Api.External.ExternalApis.Service/g' Ombi.Api.External/ExternalApis/Service/OmbiService.cs

# Update GithubApi namespaces
echo "Updating GithubApi namespaces..."
sed -i '' 's/namespace Ombi\.Api\.Github/namespace Ombi.Api.External.ExternalApis.Github/g' Ombi.Api.External/ExternalApis/Github/IGithubApi.cs
sed -i '' 's/namespace Ombi\.Api\.Github/namespace Ombi.Api.External.ExternalApis.Github/g' Ombi.Api.External/ExternalApis/Github/GithubApi.cs

echo "Namespace updates complete!"
