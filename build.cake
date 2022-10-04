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
    StartProcess("msbuild", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "Native.sln /p:Configuration=Release" });

    // Copy artifact
    CreateDirectory(artifactsDir);
    CopyFile("build/bin/Release/joltc.dll", $"{artifactsDir}/joltc.dll");
});

Task("BuildMacOS")
    .WithCriteria(() => IsRunningOnMacOs())
    .Does(() =>
{
    // Build
    var buildDir = "build";
    CreateDirectory(buildDir);
    StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "../ -DCMAKE_BUILD_TYPE=Release" });
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
    StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "../ -DCMAKE_BUILD_TYPE=Release" });
    StartProcess("make", new ProcessSettings { WorkingDirectory = buildDir });

    // Copy artifact
    CreateDirectory(artifactsDir);
    CopyFile($"build/lib/joltc.so", $"{artifactsDir}/libjoltc.so");
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