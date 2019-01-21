
#tool "nuget:?package=GitVersion.CommandLine"
#addin "Cake.Gulp"
#addin "SharpZipLib"
#addin nuget:?package=Cake.Compression&version=0.1.4
#addin "Cake.Incubator"
#addin "Cake.Yarn"

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

var frameworkVer = "netcoreapp2.2";

var buildSettings = new DotNetCoreBuildSettings
{
    Framework = frameworkVer,
    Configuration = "Release",
    OutputDirectory = Directory(buildDir),
};

var publishSettings = new DotNetCorePublishSettings
{
    Framework = frameworkVer,
    Configuration = "Release",
    OutputDirectory = Directory(buildDir),
};

var artifactsFolder = buildDir + "/"+frameworkVer+"/";
var windowsArtifactsFolder = artifactsFolder + "win10-x64/published";
var windows32BitArtifactsFolder = artifactsFolder + "win10-x86/published";
var osxArtifactsFolder = artifactsFolder + "osx-x64/published";
var linuxArtifactsFolder = artifactsFolder + "linux-x64/published";
var linuxArmArtifactsFolder = artifactsFolder + "linux-arm/published";
var linuxArm64BitArtifactsFolder = artifactsFolder + "linux-arm64/published";




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

	if(versionInfo.BranchName.Contains("_"))
	{
		versionInfo.BranchName = versionInfo.BranchName.Replace("_","-");
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
	    if(fullVer.Contains("/"))
    {
        fullVer = fullVer.Replace("/","");
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
	Yarn.FromPath(webProjDir).Install();
});

Task("Gulp Publish")
	.IsDependentOn("NPM")
	.Does(() => { 	
	Yarn.FromPath(webProjDir).RunScript("publish");
 });

Task("TSLint")
    .Does(() =>
{
	Yarn.FromPath(webProjDir).RunScript("lint");
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
	GZipCompress(linuxArmArtifactsFolder, artifactsFolder + "linux-arm.tar.gz");
	GZipCompress(linuxArm64BitArtifactsFolder, artifactsFolder + "linux-arm64.tar.gz");
});

Task("Publish")
    .IsDependentOn("PrePublish")
    .IsDependentOn("Publish-Windows")
    .IsDependentOn("Publish-Windows-32bit")
    .IsDependentOn("Publish-OSX")
    .IsDependentOn("Publish-Linux")
    .IsDependentOn("Publish-Linux-ARM")
    .IsDependentOn("Publish-Linux-ARM-64Bit")
    .IsDependentOn("Package");

Task("Publish-Windows")
    .Does(() =>
{
    publishSettings.Runtime = "win10-x64";
    publishSettings.OutputDirectory = Directory(buildDir) + Directory(frameworkVer +"/win10-x64/published");

    DotNetCorePublish("./src/Ombi/Ombi.csproj", publishSettings);
    CopyFile(buildDir + "/"+frameworkVer+"/win10-x64/Swagger.xml", buildDir + "/"+frameworkVer+"/win10-x64/published/Swagger.xml");
	
    publishSettings.OutputDirectory = Directory(buildDir) + Directory(frameworkVer +"/win10-x64/published/updater");
    DotNetCorePublish("./src/Ombi.Updater/Ombi.Updater.csproj", publishSettings);
});

Task("Publish-Windows-32bit")
    .Does(() =>
{
    publishSettings.Runtime = "win10-x86";
    publishSettings.OutputDirectory = Directory(buildDir) + Directory(frameworkVer+"/win10-x86/published");

    DotNetCorePublish("./src/Ombi/Ombi.csproj", publishSettings);
    CopyFile(buildDir + "/"+frameworkVer+"/win10-x86/Swagger.xml", buildDir + "/"+frameworkVer+"/win10-x86/published/Swagger.xml");

	
    publishSettings.OutputDirectory = Directory(buildDir) + Directory(frameworkVer +"/win10-x86/published/updater");
    DotNetCorePublish("./src/Ombi.Updater/Ombi.Updater.csproj", publishSettings);
});

Task("Publish-OSX")
    .Does(() =>
{
    publishSettings.Runtime = "osx-x64";
    publishSettings.OutputDirectory = Directory(buildDir) + Directory(frameworkVer+"/osx-x64/published");

    DotNetCorePublish("./src/Ombi/Ombi.csproj", publishSettings);
    CopyFile(buildDir + "/"+frameworkVer+"/osx-x64/Swagger.xml", buildDir + "/"+frameworkVer+"/osx-x64/published/Swagger.xml");

    publishSettings.OutputDirectory = Directory(buildDir) + Directory(frameworkVer +"/osx-x64/published/updater");
    DotNetCorePublish("./src/Ombi.Updater/Ombi.Updater.csproj", publishSettings);
});

Task("Publish-Linux")
    .Does(() =>
{
    publishSettings.Runtime = "linux-x64";
    publishSettings.OutputDirectory = Directory(buildDir) + Directory(frameworkVer+"/linux-x64/published");

    DotNetCorePublish("./src/Ombi/Ombi.csproj", publishSettings);
    CopyFile(buildDir + "/"+frameworkVer+"/linux-x64/Swagger.xml", buildDir + "/"+frameworkVer+"/linux-x64/published/Swagger.xml");
	
    publishSettings.OutputDirectory = Directory(buildDir) + Directory(frameworkVer +"/linux-x64/published/updater");
    DotNetCorePublish("./src/Ombi.Updater/Ombi.Updater.csproj", publishSettings);
});

Task("Publish-Linux-ARM")
    .Does(() =>
{
    publishSettings.Runtime = "linux-arm";
    publishSettings.OutputDirectory = Directory(buildDir) + Directory(frameworkVer+"/linux-arm/published");

    DotNetCorePublish("./src/Ombi/Ombi.csproj", publishSettings);
    CopyFile(
      buildDir + "/"+frameworkVer+"/linux-arm/Swagger.xml",
      buildDir + "/"+frameworkVer+"/linux-arm/published/Swagger.xml");

    publishSettings.OutputDirectory = Directory(buildDir) + Directory(frameworkVer +"/linux-arm/published/updater");
    DotNetCorePublish("./src/Ombi.Updater/Ombi.Updater.csproj", publishSettings);
});

Task("Publish-Linux-ARM-64Bit")
    .Does(() =>
{
    publishSettings.Runtime = "linux-arm64";
    publishSettings.OutputDirectory = Directory(buildDir) + Directory(frameworkVer+"/linux-arm64/published");

    DotNetCorePublish("./src/Ombi/Ombi.csproj", publishSettings);
    CopyFile(
      buildDir + "/"+frameworkVer+"/linux-arm64/Swagger.xml",
      buildDir + "/"+frameworkVer+"/linux-arm64/published/Swagger.xml");
	  
    publishSettings.OutputDirectory = Directory(buildDir) + Directory(frameworkVer +"/linux-arm64/published/updater");
    DotNetCorePublish("./src/Ombi.Updater/Ombi.Updater.csproj", publishSettings);
});

Task("Run-Unit-Tests")
    .Does(() =>
{  
	DotNetCoreBuild(csProj, buildSettings);
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
