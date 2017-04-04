;https://docs.microsoft.com/en-us/dotnet/articles/core/deploying/ 
cd ..
dotnet restore
dotnet publish -c Release -r win10-x64
dotnet publish -c Release -r osx.10.12-x64
dotnet publish -c Release -r ubuntu.16.10-x64
dotnet publish -c Release -r debian.8-x64

exit