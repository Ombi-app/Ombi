FROM node:20-bookworm
RUN apt-get update && apt-get install -y python3 build-essential git && rm -rf /var/lib/apt/lists/*
RUN corepack enable
