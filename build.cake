
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
var windows32BitArtifactsFolder = artifactsFolder + "win10-x32/published";
var osxArtifactsFolder = artifactsFolder + "osx-x64/published";
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

	var buildVersion = string.Empty;
	if(string.IsNullOrEmpty(AppVeyor.Environment.Build.Version))
	{
		buildVersion = "3.0.000";
	} else{
		buildVersion = AppVeyor.Environment.Build.Version;
	}

	var fullVer = buildVersion + "-" + versionInfo.BranchName;

	if(versionInfo.PreReleaseTag.Contains("PullRequest"))
	{
		fullVer = buildVersion + "-PR";
	}
    if(fullVer.Contains("_"))
    {
        fullVer = fullVer.Replace("_","");
    }

	buildSettings.ArgumentCustomization = args => args.Append("/p:SemVer=" + versionInfo.AssemblySemVer);
	buildSettings.ArgumentCustomization = args => args.Append("/p:FullVer=" + fullVer);
	publishSettings.ArgumentCustomization = args => args.Append("/p:SemVer=" + versionInfo.AssemblySemVer);
	publishSettings.ArgumentCustomization = args => args.Append("/p:FullVer=" + fullVer);
	//buildSettings.VersionSuffix = versionInfo.BranchName;
	//publishSettings.VersionSuffix = versionInfo.BranchName;
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

Task("TSLint")
    .Does(() =>
{
	var settings = new NpmRunScriptSettings {
		WorkingDirectory = webProjDir,
		ScriptName = "lint"
	};
	
    NpmRunScript(settings);
});

Task("PrePublish")
    .IsDependentOn("SetVersionInfo")
	.IsDependentOn("Gulp Publish")
	.IsDependentOn("TSLint");


Task("Package")
    .Does(() =>
{	
    Zip(windowsArtifactsFolder +"/",artifactsFolder + "windows.zip");
    Zip(windows32BitArtifactsFolder +"/",artifactsFolder + "windows-32bit.zip");
	GZipCompress(osxArtifactsFolder, artifactsFolder + "osx.tar.gz");
	GZipCompress(linuxArtifactsFolder, artifactsFolder + "linux.tar.gz");
});

Task("Publish")
    .IsDependentOn("PrePublish")
    .IsDependentOn("Publish-Windows")
    .IsDependentOn("Publish-OSX").IsDependentOn("Publish-Linux")
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

Task("Publish-Windows-32bit")
    .Does(() =>
{
    publishSettings.Runtime = "win10-x32";
    publishSettings.OutputDirectory = Directory(buildDir) + Directory("netcoreapp2.0/win10-x32/published");

    DotNetCorePublish("./src/Ombi/Ombi.csproj", publishSettings);
    CopyFile(buildDir + "/netcoreapp2.0/win10-x32/Swagger.xml", buildDir + "/netcoreapp2.0/win10-x32/published/Swagger.xml");
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

Task("Publish-Linux")
    .Does(() =>
{
    publishSettings.Runtime = "linux-x64";
    publishSettings.OutputDirectory = Directory(buildDir) + Directory("netcoreapp2.0/linux-x64/published");

    DotNetCorePublish("./src/Ombi/Ombi.csproj", publishSettings);
    CopyFile(buildDir + "/netcoreapp2.0/linux-x64/Swagger.xml", buildDir + "/netcoreapp2.0/linux-x64/published/Swagger.xml");
    DotNetCorePublish("./src/Ombi.Updater/Ombi.Updater.csproj", publishSettings);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////
 
Task("Default")
    .IsDependentOn("Publish");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
