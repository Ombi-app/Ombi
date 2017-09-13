#tool "xunit.runner.console"
#tool "nuget:?package=GitVersion.CommandLine"
#addin "Cake.Gulp"
#addin "Cake.Npm"
#addin "SharpZipLib"
#addin "Cake.Compression"
#addin "Cake.Incubator"

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
GitVersion versionInfo = null;

var buildSettings = new DotNetCoreBuildSettings
{
    Framework = "netcoreapp2.0",
    Configuration = "Release",
    OutputDirectory = Directory(buildDir),
};

var publishSettings = new DotNetCorePublishSettings
{
    Framework = "netcoreapp2.0",
    Configuration = "Release",
    OutputDirectory = Directory(buildDir),
};

var artifactsFolder = buildDir + "/netcoreapp2.0/";
var windowsArtifactsFolder = artifactsFolder + "win10-x64/published";
var osxArtifactsFolder = artifactsFolder + "osx-x64/published";
var ubuntuArtifactsFolder = artifactsFolder + "ubuntu-x64/published";
var debianArtifactsFolder = artifactsFolder + "debian.8-x64/published";
var centosArtifactsFolder = artifactsFolder + "centos.7-x64/published";
var linuxArtifactsFolder = artifactsFolder + "linux-x64/published";




//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
    //CleanDirectory(nodeModulesDir);
    CleanDirectory(wwwRootDistDir);
});

Task("SetVersionInfo")
    .IsDependentOn("Clean")
    .Does(() =>
{
	var settings = new GitVersionSettings {
        RepositoryPath = ".",
    };

	if (AppVeyor.IsRunningOnAppVeyor) {
		settings.Branch = AppVeyor.Environment.Repository.Branch;
	} else {
		settings.Branch = "master";
	}

    versionInfo = GitVersion(settings);
	
	Information("GitResults -> {0}", versionInfo.Dump());

    Information(@"Build:{0}",AppVeyor.Environment.Build.Dump());

	var fullVer = AppVeyor.Environment.Build.Version;


	buildSettings.ArgumentCustomization = args => args.Append("/p:SemVer=" + versionInfo.AssemblySemVer);
	buildSettings.ArgumentCustomization = args => args.Append("/p:FullVer=" + fullVer);
	publishSettings.ArgumentCustomization = args => args.Append("/p:SemVer=" + versionInfo.AssemblySemVer);
	publishSettings.ArgumentCustomization = args => args.Append("/p:FullVer=" + fullVer);
	buildSettings.VersionSuffix = versionInfo.BranchName;
	publishSettings.VersionSuffix = versionInfo.BranchName;
});

Task("Restore")
    .IsDependentOn("SetVersionInfo")
	.IsDependentOn("Gulp Publish")
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
});

Task("Gulp Publish")
.IsDependentOn("NPM")
.Does(() => {

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
	GZipCompress(linuxArtifactsFolder, artifactsFolder + "linux.tar.gz");
});

Task("Publish")
    .IsDependentOn("Build")
    .IsDependentOn("Publish-Windows")
    .IsDependentOn("Publish-OSX").IsDependentOn("Publish-Ubuntu").IsDependentOn("Publish-Debian").IsDependentOn("Publish-Centos").IsDependentOn("Publish-Linux")
    .IsDependentOn("Package");

Task("Publish-Windows")
    .Does(() =>
{
    publishSettings.Runtime = "win10-x64";
    publishSettings.OutputDirectory = Directory(buildDir) + Directory("netcoreapp2.0/win10-x64/published");

    DotNetCorePublish("./src/Ombi/Ombi.csproj", publishSettings);
    CopyFile(buildDir + "/netcoreapp2.0/win10-x64/Swagger.xml", buildDir + "/netcoreapp2.0/win10-x64/published/Swagger.xml");
    DotNetCorePublish("./src/Ombi.Updater/Ombi.Updater.csproj", publishSettings);
});

Task("Publish-OSX")
    .Does(() =>
{
    publishSettings.Runtime = "osx-x64";
    publishSettings.OutputDirectory = Directory(buildDir) + Directory("netcoreapp2.0/osx-x64/published");

    DotNetCorePublish("./src/Ombi/Ombi.csproj", publishSettings);
    CopyFile(buildDir + "/netcoreapp2.0/osx-x64/Swagger.xml", buildDir + "/netcoreapp2.0/osx-x64/published/Swagger.xml");
    DotNetCorePublish("./src/Ombi.Updater/Ombi.Updater.csproj", publishSettings);
});

Task("Publish-Ubuntu")
    .Does(() =>
{
     publishSettings.Runtime = "ubuntu-x64";
    publishSettings.OutputDirectory = Directory(buildDir) + Directory("netcoreapp2.0/ubuntu-x64/published");

     DotNetCorePublish("./src/Ombi/Ombi.csproj", publishSettings);
    CopyFile(buildDir + "/netcoreapp2.0/ubuntu-x64/Swagger.xml", buildDir + "/netcoreapp2.0/ubuntu-x64/published/Swagger.xml");
     DotNetCorePublish("./src/Ombi.Updater/Ombi.Updater.csproj", publishSettings);
});
Task("Publish-Debian")
    .Does(() =>
{
     publishSettings.Runtime = "debian.8-x64";
    publishSettings.OutputDirectory = Directory(buildDir) + Directory("netcoreapp2.0/debian.8-x64/published");

     DotNetCorePublish("./src/Ombi/Ombi.csproj", publishSettings);
    CopyFile(buildDir + "/netcoreapp2.0/debian.8-x64/Swagger.xml", buildDir + "/netcoreapp2.0/debian.8-x64/published/Swagger.xml");
     DotNetCorePublish("./src/Ombi.Updater/Ombi.Updater.csproj", publishSettings);
});
Task("Publish-Centos")
    .Does(() =>
{
     publishSettings.Runtime = "centos.7-x64";
    publishSettings.OutputDirectory = Directory(buildDir) + Directory("netcoreapp2.0/centos.7-x64/published");

     DotNetCorePublish("./src/Ombi/Ombi.csproj", publishSettings);
    CopyFile(buildDir + "/netcoreapp2.0/centos.7-x64/Swagger.xml", buildDir + "/netcoreapp2.0/centos.7-x64/published/Swagger.xml");
     DotNetCorePublish("./src/Ombi.Updater/Ombi.Updater.csproj", publishSettings);
});

Task("Publish-Linux")
    .Does(() =>
{
     publishSettings.Runtime = "linux-x64";
    publishSettings.OutputDirectory = Directory(buildDir) + Directory("netcoreapp2.0/linux-x64/published");

     DotNetCorePublish("./src/Ombi/Ombi.csproj", publishSettings);
    CopyFile(buildDir + "/netcoreapp2.0/linux-x64/Swagger.xml", buildDir + "/netcoreapp2.0/linux-x64/published/Swagger.xml");
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
