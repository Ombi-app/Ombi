;https://docs.microsoft.com/en-us/dotnet/articles/core/deploying/ 
cd ..
dotnet restore
dotnet publish -c Release /p:AppRuntimeIdentifier=win10-x64
dotnet publish -c Release /p:AppRuntimeIdentifier=osx.10.12-x64
dotnet publish -c Release /p:AppRuntimeIdentifier=ubuntu.16.10-x64
dotnet publish -c Release /p:AppRuntimeIdentifier=debian.8-x64

exit