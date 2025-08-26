#!/bin/bash

# Script to update all using statements in the Notifications project to use the new consolidated namespaces

echo "Updating using statements in Notifications project..."

# Update Discord
find Ombi.Notifications -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Discord/Ombi.Api.External.NotificationServices.Discord/g' {} \;

# Update Gotify
find Ombi.Notifications -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Gotify/Ombi.Api.External.NotificationServices.Gotify/g' {} \;

# Update Mattermost
find Ombi.Notifications -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Mattermost/Ombi.Api.External.NotificationServices.Mattermost/g' {} \;

# Update Pushbullet
find Ombi.Notifications -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Pushbullet/Ombi.Api.External.NotificationServices.Pushbullet/g' {} \;

# Update Pushover
find Ombi.Notifications -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Pushover/Ombi.Api.External.NotificationServices.Pushover/g' {} \;

# Update Slack
find Ombi.Notifications -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Slack/Ombi.Api.External.NotificationServices.Slack/g' {} \;

# Update Telegram
find Ombi.Notifications -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Telegram/Ombi.Api.External.NotificationServices.Telegram/g' {} \;

# Update Webhook
find Ombi.Notifications -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Webhook/Ombi.Api.External.NotificationServices.Webhook/g' {} \;

# Update Twilio
find Ombi.Notifications -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Twilio/Ombi.Api.External.NotificationServices.Twilio/g' {} \;

# Update CloudService
find Ombi.Notifications -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.CloudService/Ombi.Api.External.NotificationServices.CloudService/g' {} \;

# Update Notifications API
find Ombi.Notifications -name "*.cs" -exec sed -i '' 's/Ombi\.Api\.Notifications/Ombi.Api.External.NotificationServices.Notifications/g' {} \;

echo "Using statement updates complete!"
