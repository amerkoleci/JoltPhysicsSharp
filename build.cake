#addin nuget:?package=Cake.FileHelpers&version=5.0.0

var target = Argument("target", "Build");
var artifactsDir = "artifacts";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("BuildWindows")
    .WithCriteria(() => IsRunningOnWindows())
    .Does(() =>
{
    // Build
    var buildDir = "build";
    CreateDirectory(buildDir);
    StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "-G \"Visual Studio 17 2022\" -A x64 ../" });
    StartProcess("msbuild", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "JoltC.sln /p:Configuration=Distribution" });

    // Copy artifact
    CreateDirectory(artifactsDir);
    CopyFile("build/bin/Distribution/joltc.dll", $"{artifactsDir}/joltc.dll");
});

Task("BuildMacOS")
    .WithCriteria(() => IsRunningOnMacOs())
    .Does(() =>
{
    // Build
    var buildDir = "build";
    CreateDirectory(buildDir);
    StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "../ -DCMAKE_OSX_ARCHITECTURES=arm64;x86_64 -DCMAKE_OSX_DEPLOYMENT_TARGET=10.13 -DCMAKE_BUILD_TYPE=Distribution" });
    StartProcess("make", new ProcessSettings { WorkingDirectory = buildDir });

    // Copy artifact
    CreateDirectory(artifactsDir);
    CopyFile("build/lib/libjoltc.dylib", $"{artifactsDir}/libjoltc.dylib");
});

Task("BuildLinux")
    .WithCriteria(() => IsRunningOnLinux())
    .Does(() =>
{
    // Build
    var buildDir = "build";
    CreateDirectory(buildDir);
    StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "../ -DCMAKE_BUILD_TYPE=Distribution" });
    StartProcess("make", new ProcessSettings { WorkingDirectory = buildDir });

    // Copy artifact
    CreateDirectory(artifactsDir);
    CopyFile($"build/lib/libjoltc.so", $"{artifactsDir}/libjoltc.so");
});

Task("Package")
    .Does(() =>
{
    var dnMsBuildSettings = new DotNetMSBuildSettings();
    var dnPackSettings = new DotNetPackSettings();
    dnPackSettings.MSBuildSettings = dnMsBuildSettings;
    dnPackSettings.Verbosity = DotNetVerbosity.Minimal;
    dnPackSettings.Configuration = "Release";   

    DotNetPack("src/JoltPhysicsSharp/JoltPhysicsSharp.csproj", dnPackSettings);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Build")
    .IsDependentOn("BuildWindows")
    .IsDependentOn("BuildMacOS")
    .IsDependentOn("BuildLinux");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);