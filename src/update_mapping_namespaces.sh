#!/bin/bash
# Script to update all using statements in the Mapping project to use the new consolidated namespaces
echo "Updating using statements in Mapping project..."

# Update TheMovieDb namespaces
echo "Updating TheMovieDb namespaces..."
find Ombi.Mapping -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.TheMovieDb/Ombi.Api.External.ExternalApis.TheMovieDb/g' {} \;

# Update TvMaze namespaces
echo "Updating TvMaze namespaces..."
find Ombi.Mapping -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.TvMaze/Ombi.Api.External.ExternalApis.TvMaze/g' {} \;

echo "Using statement updates complete!"
