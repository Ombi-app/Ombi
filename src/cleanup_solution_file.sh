#!/bin/bash

echo "Cleaning up solution file by removing old API project references..."

# Create a backup of the original solution file
cp Ombi.sln Ombi.sln.backup
echo "Created backup: Ombi.sln.backup"

# Remove all the old API project references from the solution file
echo "Removing old API project references..."

# Remove each project entry and its corresponding build configurations
# We'll remove them one by one to be precise

# Remove Ombi.Api.TheMovieDb
sed -i '' '/Ombi\.Api\.TheMovieDb/d' Ombi.sln
sed -i '' '/{132DA282-5894-4570-8916-D8C18ED2CE84}/d' Ombi.sln

# Remove Ombi.Api
sed -i '' '/Ombi\.Api.*Ombi\.Api\\Ombi\.Api\.csproj/d' Ombi.sln
sed -i '' '/{EA31F915-31F9-4318-B521-1500CDF40DDF}/d' Ombi.sln

# Remove Ombi.Api.Plex
sed -i '' '/Ombi\.Api\.Plex/d' Ombi.sln
sed -i '' '/{2E1A7B91-F29B-42BC-8F1E-1CF2DCC389BA}/d' Ombi.sln

# Remove Ombi.Api.Emby
sed -i '' '/Ombi\.Api\.Emby/d' Ombi.sln
sed -i '' '/{08FF107D-31E1-470D-AF86-E09B015CEE06}/d' Ombi.sln

# Remove Ombi.Api.Jellyfin
sed -i '' '/Ombi\.Api\.Jellyfin/d' Ombi.sln
sed -i '' '/{F03757C7-5145-45C9-AFFF-B4E946755779}/d' Ombi.sln

# Remove Ombi.Api.Sonarr
sed -i '' '/Ombi\.Api\.Sonarr/d' Ombi.sln
sed -i '' '/{CFB5E008-D0D0-43C0-AA06-89E49D17F384}/d' Ombi.sln

# Remove Ombi.Api.TvMaze
sed -i '' '/Ombi\.Api\.TvMaze/d' Ombi.sln
sed -i '' '/{0E8EF835-E4F0-4EE5-A2B6-678DEE973721}/d' Ombi.sln

# Remove Ombi.Api.Trakt
sed -i '' '/Ombi\.Api\.Trakt/d' Ombi.sln
sed -i '' '/{3880375C-1A7E-4D75-96EC-63B954C42FEA}/d' Ombi.sln

# Remove Ombi.Api.Radarr
sed -i '' '/Ombi\.Api\.Radarr/d' Ombi.sln
sed -i '' '/{94D04C1F-E35A-499C-B0A0-9FADEBDF8336}/d' Ombi.sln

# Remove Ombi.Api.Discord
sed -i '' '/Ombi\.Api\.Discord/d' Ombi.sln
sed -i '' '/{5AF2B6D2-5CC6-49FE-928A-BA27AF52B194}/d' Ombi.sln

# Remove Ombi.Api.Service
sed -i '' '/Ombi\.Api\.Service/d' Ombi.sln
sed -i '' '/{A0892896-F5BD-47E2-823E-DFCE82514EEC}/d' Ombi.sln

# Remove Ombi.Api.FanartTv
sed -i '' '/Ombi\.Api\.FanartTv/d' Ombi.sln
sed -i '' '/{FD947E63-A0D2-4878-8378-2005D5E9AB8A}/d' Ombi.sln

# Remove Ombi.Api.Pushbullet
sed -i '' '/Ombi\.Api\.Pushbullet/d' Ombi.sln
sed -i '' '/{E237CDF6-D044-437D-B157-E9A3CC0BCF53}/d' Ombi.sln

# Remove Ombi.Api.Slack
sed -i '' '/Ombi\.Api\.Slack/d' Ombi.sln
sed -i '' '/{71708256-9152-4E81-9FCA-E3181A185806}/d' Ombi.sln

# Remove Ombi.Api.Mattermost
sed -i '' '/Ombi\.Api\.Mattermost/d' Ombi.sln
sed -i '' '/{737B2620-FE5A-4135-A017-79C269A7D36C}/d' Ombi.sln

# Remove Ombi.Api.Pushover
sed -i '' '/Ombi\.Api\.Pushover/d' Ombi.sln
sed -i '' '/{CA55DD4F-4EFF-4906-A848-35FCC7BD5654}/d' Ombi.sln

# Remove Ombi.Api.CouchPotato
sed -i '' '/Ombi\.Api\.CouchPotato/d' Ombi.sln
sed -i '' '/{87D7897D-7C73-4856-A0AA-FF5948F4EA86}/d' Ombi.sln

# Remove Ombi.Api.DogNzb
sed -i '' '/Ombi\.Api\.DogNzb/d' Ombi.sln
sed -i '' '/{4F3BF03A-6AAC-4960-A2CD-1EAD7273115E}/d' Ombi.sln

# Remove Ombi.Api.Telegram
sed -i '' '/Ombi\.Api\.Telegram/d' Ombi.sln
sed -i '' '/{CB9DD209-8E09-4E01-983E-C77C89592D36}/d' Ombi.sln

# Remove Ombi.Api.Github
sed -i '' '/Ombi\.Api\.Github/d' Ombi.sln
sed -i '' '/{55866DEE-46D1-4AF7-B1A2-62F6190C8EC7}/d' Ombi.sln

# Remove Ombi.Api.SickRage
sed -i '' '/Ombi\.Api\.SickRage/d' Ombi.sln
sed -i '' '/{94C9A366-2595-45EA-AABB-8E4A2E90EC5B}/d' Ombi.sln

# Remove Ombi.Api.Notifications
sed -i '' '/Ombi\.Api\.Notifications/d' Ombi.sln
sed -i '' '/{10D1FE9D-9124-42B7-B1E1-CEB99B832618}/d' Ombi.sln

# Remove Ombi.Api.Lidarr
sed -i '' '/Ombi\.Api\.Lidarr/d' Ombi.sln
sed -i '' '/{4FA21A20-92F4-462C-B929-2C517A88CC56}/d' Ombi.sln

# Remove Ombi.Api.Gotify
sed -i '' '/Ombi\.Api\.Gotify/d' Ombi.sln
sed -i '' '/{105EA346-766E-45B8-928B-DE6991DCB7EB}/d' Ombi.sln

# Remove Ombi.Api.GroupMe
sed -i '' '/Ombi\.Api\.GroupMe/d' Ombi.sln
sed -i '' '/{9266403C-B04D-4C0F-AC39-82F12C781949}/d' Ombi.sln

# Remove Ombi.Api.MusicBrainz
sed -i '' '/Ombi\.Api\.MusicBrainz/d' Ombi.sln
sed -i '' '/{C5C1769B-4197-4410-A160-0EEF39EDDC98}/d' Ombi.sln

# Remove Ombi.Api.Twilio
sed -i '' '/Ombi\.Api\.Twilio/d' Ombi.sln
sed -i '' '/{34E5DD1A-6A90-448B-9E71-64D1ACD6C1A3}/d' Ombi.sln

# Remove Ombi.Api.Webhook
sed -i '' '/Ombi\.Api\.Webhook/d' Ombi.sln
sed -i '' '/{E2186FDA-D827-4781-8663-130AC382F12C}/d' Ombi.sln

# Remove Ombi.Api.CloudService
sed -i '' '/Ombi\.Api\.CloudService/d' Ombi.sln
sed -i '' '/{5DE40A66-B369-469E-8626-ECE23D9D8034}/d' Ombi.sln

# Remove Ombi.Api.RottenTomatoes
sed -i '' '/Ombi\.Api\.RottenTomatoes/d' Ombi.sln
sed -i '' '/{8F19C701-7881-4BC7-8BBA-B068A6B954AD}/d' Ombi.sln

# Remove Ombi.Api.MediaServer
sed -i '' '/Ombi\.Api\.MediaServer/d' Ombi.sln
sed -i '' '/{AFC0BA9B-E38D-479F-825A-2F94EE4D6120}/d' Ombi.sln

echo "Solution file cleanup complete!"
echo "All old API project references have been removed."
echo "The Ombi.Api.External project remains as the consolidated API project."
