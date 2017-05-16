FROM microsoft/dotnet:1.1-sdk-msbuild
ARG source=./bin/Release/netcoreapp1.1/publish
WORKDIR /app
COPY $source .
ENV ASPNETCORE_URLS http://*:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "Ombi.dll"]
COPY . /app