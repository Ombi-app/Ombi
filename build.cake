#tool "xunit.runner.console"
#addin "Cake.Gulp"
#addin "Cake.Npm"
#addin "SharpZipLib"
#addin "Cake.Compression"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var buildDir = "./src/Ombi/bin/" + configuration;
var nodeModulesDir ="./src/Ombi/node_modules/";
var wwwRootDistDir = "./src/Ombi/wwwroot/dist/";
var projDir = "./src/";                         //  Project Directory
var webProjDir = "./src/Ombi";
var csProj = "./src/Ombi/Ombi.csproj";          // Path to the project.csproj
var solutionFile = "Ombi.sln";                  // Solution file if needed

var buildSettings = new DotNetCoreBuildSettings
{
    Framework = "netcoreapp1.1",
    Configuration = "Release",
    OutputDirectory = Directory(buildDir),
    VersionSuffix = GetVersion(),
};

var publishSettings = new DotNetCorePublishSettings
{
    Framework = "netcoreapp1.1",
    Configuration = "Release",
    OutputDirectory = Directory(buildDir),
    VersionSuffix = GetVersion(),
};

var artifactsFolder = buildDir + "/netcoreapp1.1/";
var windowsArtifactsFolder = artifactsFolder + "win10-x64/published";
var osxArtifactsFolder = artifactsFolder + "osx.10.12-x64/published";
var ubuntuArtifactsFolder = artifactsFolder + "ubuntu.16.04-x64/published";
var debianArtifactsFolder = artifactsFolder + "debian.8-x64/published";
var centosArtifactsFolder = artifactsFolder + "centos.7-x64/published";


//////////////////////////////////////////////////////////////////////
// Helper
//////////////////////////////////////////////////////////////////////

public string GetVersion()
{
    if (AppVeyor.IsRunningOnAppVeyor) {
        // Update the version		
		return AppVeyor.Environment.Build.Version;
        //AppVeyor.Environment.Repository.Branch;
	} 
    return "3.0.100";
}

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
    CleanDirectory(nodeModulesDir);
    CleanDirectory(wwwRootDistDir);
});

Task("Restore")
    .IsDependentOn("Clean")
	.IsDependentOn("NPM")
    .Does(() =>
{
    DotNetCoreRestore(projDir);
});

Task("NPM")
.Does(() => {
var settings = new NpmInstallSettings {
		LogLevel = NpmLogLevel.Silent,
		WorkingDirectory = webProjDir,
		Production = true
	};

	NpmInstall(settings);

	var runScriptSettings = new NpmRunScriptSettings {
		ScriptName="publish",
		WorkingDirectory = webProjDir,
	};
	
	NpmRunScript(runScriptSettings);
});


Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
{
        DotNetCoreBuild(csProj, buildSettings);
});

Task("Package")
    .Does(() =>
{	
    Zip(windowsArtifactsFolder +"/",artifactsFolder + "windows.zip");
	GZipCompress(osxArtifactsFolder, artifactsFolder + "osx.tar.gz");
	GZipCompress(ubuntuArtifactsFolder, artifactsFolder + "ubuntu.tar.gz");
	GZipCompress(debianArtifactsFolder, artifactsFolder + "debian.tar.gz");
	GZipCompress(centosArtifactsFolder, artifactsFolder + "centos.tar.gz");
});

Task("Publish")
    .IsDependentOn("Build")
    .IsDependentOn("Publish-Windows")
    .IsDependentOn("Publish-OSX").IsDependentOn("Publish-Ubuntu").IsDependentOn("Publish-Debian").IsDependentOn("Publish-Centos")
    .IsDependentOn("Package");

Task("Publish-Windows")
    .Does(() =>
{
    publishSettings.Runtime = "win10-x64";
    publishSettings.OutputDirectory = Directory(buildDir) + Directory("netcoreapp1.1/win10-x64/published");

    DotNetCorePublish("./src/Ombi/Ombi.csproj", publishSettings);
    DotNetCorePublish("./src/Ombi.Updater/Ombi.Updater.csproj", publishSettings);
});

Task("Publish-OSX")
    .Does(() =>
{
    publishSettings.Runtime = "osx.10.12-x64";
    publishSettings.OutputDirectory = Directory(buildDir) + Directory("netcoreapp1.1/osx.10.12-x64/published");

    DotNetCorePublish("./src/Ombi/Ombi.csproj", publishSettings);
    DotNetCorePublish("./src/Ombi.Updater/Ombi.Updater.csproj", publishSettings);
});

Task("Publish-Ubuntu")
    .Does(() =>
{
     publishSettings.Runtime = "ubuntu.16.04-x64";
    publishSettings.OutputDirectory = Directory(buildDir) + Directory("netcoreapp1.1/ubuntu.16.04-x64/published");

     DotNetCorePublish("./src/Ombi/Ombi.csproj", publishSettings);
     DotNetCorePublish("./src/Ombi.Updater/Ombi.Updater.csproj", publishSettings);
});
Task("Publish-Debian")
    .Does(() =>
{
     publishSettings.Runtime = "debian.8-x64";
    publishSettings.OutputDirectory = Directory(buildDir) + Directory("netcoreapp1.1/debian.8-x64/published");

     DotNetCorePublish("./src/Ombi/Ombi.csproj", publishSettings);
     DotNetCorePublish("./src/Ombi.Updater/Ombi.Updater.csproj", publishSettings);
});
Task("Publish-Centos")
    .Does(() =>
{
     publishSettings.Runtime = "centos.7-x64";
    publishSettings.OutputDirectory = Directory(buildDir) + Directory("netcoreapp1.1/centos.7-x64/published");

     DotNetCorePublish("./src/Ombi/Ombi.csproj", publishSettings);
     DotNetCorePublish("./src/Ombi.Updater/Ombi.Updater.csproj", publishSettings);
});

Task("Run-Unit-Tests")
    .IsDependentOn("Publish")
    .Does(() =>
{
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Run-Unit-Tests");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
