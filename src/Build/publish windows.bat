;https://docs.microsoft.com/en-us/dotnet/articles/core/deploying/ 
cd ..
dotnet restore
dotnet publish -c Release /p:AppRuntimeIdentifier=win10-x64

exit