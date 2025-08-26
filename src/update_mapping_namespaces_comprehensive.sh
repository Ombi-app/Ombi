#!/bin/bash
# Comprehensive script to update all using statements in the Mapping project
echo "Updating using statements in Mapping project comprehensively..."

# Update TheMovieDb namespaces (full and shorthand)
echo "Updating TheMovieDb namespaces..."
find Ombi.Mapping -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.TheMovieDb/Ombi.Api.External.ExternalApis.TheMovieDb/g' {} \;
find Ombi.Mapping -name "*.cs" -exec sed -i '' 's/Api\.TheMovieDb/Ombi.Api.External.ExternalApis.TheMovieDb/g' {} \;

# Update TvMaze namespaces (full and shorthand)
echo "Updating TvMaze namespaces..."
find Ombi.Mapping -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.TvMaze/Ombi.Api.External.ExternalApis.TvMaze/g' {} \;
find Ombi.Mapping -name "*.cs" -exec sed -i '' 's/Api\.TvMaze/Ombi.Api.External.ExternalApis.TvMaze/g' {} \;

echo "Comprehensive namespace update complete!"
