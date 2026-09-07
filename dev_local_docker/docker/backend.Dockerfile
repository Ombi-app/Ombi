FROM mcr.microsoft.com/dotnet/sdk:8.0
RUN apt-get update && apt-get install -y git curl unzip && rm -rf /var/lib/apt/lists/*
