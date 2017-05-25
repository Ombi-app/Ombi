
param([String]$env='local')

"Environment: " + $env | Write-Output;
"Build Version: " + $env:APPVEYOR_BUILD_VERSION | Write-Output;
"Base Path: " + $env:APPVEYOR_BUILD_FOLDER  | Write-Output;

$appSettingsPath = $env:APPVEYOR_BUILD_FOLDER + '\src\Ombi\appsettings.json'
$appSettings = Get-Content $appSettingsPath -raw
$appSettings = $appSettings.Replace("{{VERSIONNUMBER}}",$ver);
Set-Content -Path $appSettingsPath -Value $appSettings 

$configPath = $env:APPVEYOR_BUILD_FOLDER + '\src\Ombi\wwwroot\app\config.ts';
$config = Get-Content $configPath -raw 

$config = $config.Replace("{{ENVIRONMENT}}",$env);
$config | Write-Output
Set-Content -Path $configPath -Value $config