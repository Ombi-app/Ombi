
param([String]$env='local')

"Environment: " + $env | Write-Output;
"Build Version: " + $env:APPVEYOR_BUILD_VERSION | Write-Output;
"Base Path: " + $env:APPVEYOR_BUILD_FOLDER  | Write-Output;

$appSettingsPath = $env:APPVEYOR_BUILD_FOLDER + '\src\Ombi\appsettings.json'
$appSettings = Get-Content $appSettingsPath -raw
$appSettings = $appSettings.Replace("{{VERSIONNUMBER}}",$env:APPVEYOR_BUILD_VERSION);
$appSettings = $appSettings.Replace("{{BRANCH}}",$env:APPVEYOR_REPO_BRANCH);
Set-Content -Path $appSettingsPath -Value $appSettings 
