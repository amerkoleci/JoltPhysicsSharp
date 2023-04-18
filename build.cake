var target = Argument("target", "Build");
var artifactsDir = "artifacts";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("BuildWindows")
    .WithCriteria(() => IsRunningOnWindows())
    .Does(() =>
{
    CreateDirectory(artifactsDir);
    CreateDirectory($"{artifactsDir}/win-x64/native");
    CreateDirectory($"{artifactsDir}/win-arm64/native");

    // win-x64
    var buildDir = "build_winx64";
    CreateDirectory(buildDir);
    StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "-G \"Visual Studio 17 2022\" -A x64 ../" });
    StartProcess("msbuild", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "JoltC.sln /p:Configuration=Distribution" });
    CopyFile("build_winx64/bin/Distribution/joltc.dll", $"{artifactsDir}/win-x64/native/joltc.dll");

    // win-x64 double
    buildDir = "build_winx64_double";
    CreateDirectory(buildDir);
    StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "-G \"Visual Studio 17 2022\" -A x64 -DDOUBLE_PRECISION=ON ../" });
    StartProcess("msbuild", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "JoltC.sln /p:Configuration=Distribution" });
    CopyFile("build_winx64_double/bin/Distribution/joltc.dll", $"{artifactsDir}/win-x64/native/joltc_double.dll");

    // ARM64
    buildDir = "build_arm64";
    CreateDirectory(buildDir);
    StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "-G \"Visual Studio 17 2022\" -A ARM64 ../" });
    StartProcess("msbuild", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "JoltC.sln /p:Configuration=Distribution" });
    CopyFile("build_arm64/bin/Distribution/joltc.dll", $"{artifactsDir}/win-arm64/native/joltc.dll");

    // ARM64
    buildDir = "build_arm64_double";
    CreateDirectory(buildDir);
    StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "-G \"Visual Studio 17 2022\" -A ARM64 -DDOUBLE_PRECISION=ON ../" });
    StartProcess("msbuild", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "JoltC.sln /p:Configuration=Distribution" });
    CopyFile("build_arm64_double/bin/Distribution/joltc.dll", $"{artifactsDir}/win-arm64/native/joltc_double.dll");
});

Task("BuildMacOS")
    .WithCriteria(() => IsRunningOnMacOs())
    .Does(() =>
{
    CreateDirectory(artifactsDir);
    
    // Build
    var buildDir = "build";
    CreateDirectory(buildDir);
    StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "../ -DCMAKE_BUILD_TYPE=Distribution" });
    StartProcess("make", new ProcessSettings { WorkingDirectory = buildDir });
    CopyFile("build/lib/libjoltc.dylib", $"{artifactsDir}/libjoltc.dylib");

    buildDir = "build_double";
    CreateDirectory(buildDir);
    StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "../ -DCMAKE_BUILD_TYPE=Distribution" -DDOUBLE_PRECISION=ON });
    StartProcess("make", new ProcessSettings { WorkingDirectory = buildDir });
    CopyFile("build_double/lib/libjoltc.dylib", $"{artifactsDir}/libjoltc_double.dylib");
});

Task("BuildLinux")
    .WithCriteria(() => IsRunningOnLinux())
    .Does(() =>
{
    CreateDirectory(artifactsDir);

    // Build
    var buildDir = "build";
    CreateDirectory(buildDir);
    StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "../ -DCMAKE_BUILD_TYPE=Distribution" });
    StartProcess("make", new ProcessSettings { WorkingDirectory = buildDir });
    CopyFile($"build/lib/libjoltc.so", $"{artifactsDir}/libjoltc.so");

    buildDir = "build_double";
    CreateDirectory(buildDir);
    StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "../ -DCMAKE_BUILD_TYPE=Distribution" });
    StartProcess("make", new ProcessSettings { WorkingDirectory = buildDir });
    CopyFile($"build_double/lib/libjoltc.so", $"{artifactsDir}/libjoltc_double.so");
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