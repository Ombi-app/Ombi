#!/bin/bash

# Script to update all project references from individual API projects to the consolidated Ombi.Api.External project

echo "Updating project references to use consolidated Ombi.Api.External project..."

# Update Ombi.Core.csproj
echo "Updating Ombi.Core.csproj..."
sed -i '' '/Ombi\.Api\./d' Ombi.Core/Ombi.Core.csproj
sed -i '' '/Ombi\.TheMovieDbApi/d' Ombi.Core/Ombi.Core.csproj
sed -i '' 's|    <ProjectReference Include="..\\Ombi.Helpers\\Ombi.Helpers.csproj" />|    <ProjectReference Include="..\\Ombi.Api.External\\Ombi.Api.External.csproj" />\n    <ProjectReference Include="..\\Ombi.Helpers\\Ombi.Helpers.csproj" />|' Ombi.Core/Ombi.Core.csproj

# Update Ombi.HealthChecks.csproj
echo "Updating Ombi.HealthChecks.csproj..."
sed -i '' '/Ombi\.Api\./d' Ombi.HealthChecks/Ombi.HealthChecks.csproj
sed -i '' 's|    <ProjectReference Include="..\\Ombi.Helpers\\Ombi.Helpers.csproj" />|    <ProjectReference Include="..\\Ombi.Api.External\\Ombi.Api.External.csproj" />\n    <ProjectReference Include="..\\Ombi.Helpers\\Ombi.Helpers.csproj" />|' Ombi.HealthChecks/Ombi.HealthChecks.csproj

# Update Ombi.Mapping.csproj
echo "Updating Ombi.Mapping.csproj..."
sed -i '' '/Ombi\.Api\./d' Ombi.Mapping/Ombi.Mapping.csproj

# Update Ombi.Notifications.csproj
echo "Updating Ombi.Notifications.csproj..."
sed -i '' '/Ombi\.Api\./d' Ombi.Notifications/Ombi.Notifications.csproj
sed -i '' 's|    <ProjectReference Include="..\\Ombi.Core\\Ombi.Core.csproj" />|    <ProjectReference Include="..\\Ombi.Api.External\\Ombi.Api.External.csproj" />\n    <ProjectReference Include="..\\Ombi.Core\\Ombi.Core.csproj" />|' Ombi.Notifications/Ombi.Notifications.csproj

# Update Ombi.Schedule.csproj
echo "Updating Ombi.Schedule.csproj..."
sed -i '' '/Ombi\.Api\./d' Ombi.Schedule/Ombi.Schedule.csproj
sed -i '' 's|    <ProjectReference Include="..\\Ombi.Core\\Ombi.Core.csproj" />|    <ProjectReference Include="..\\Ombi.Api.External\\Ombi.Api.External.csproj" />\n    <ProjectReference Include="..\\Ombi.Core\\Ombi.Core.csproj" />|' Ombi.Schedule/Ombi.Schedule.csproj

echo "Project reference updates complete!"
