
param([String]$env='local',
[String]$ver='3.0.0',
[String]$basePath='')

"Environment: " + $env | Write-Output;
"Build Version: " + $ver | Write-Output;
"Base Path: " + $basePath | Write-Output;

$appSettingsPath = $basePath + '\src\Ombi\appsettings.json'
$appSettings = Get-Content $appSettingsPath -raw
$appSettings = $appSettings.Replace("{{VERSIONNUMBER}}",$ver);
Set-Content -Path $appSettingsPath -Value $appSettings 

$configPath = $basePath + '\src\Ombi\wwwroot\app\config.ts';
$config = Get-Content $configPath -raw 

$config = $config.Replace("{{ENVIRONMENT}}",$env);
$config | Write-Output
#Set-Content -Path $configPath -Value $config