// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace JoltPhysicsSharp;

internal static unsafe partial class JoltApi
{
    private const string LibName = "joltc";
    private const string LibDoubleName = "joltc_double";

    public static bool DoublePrecision { get; set; }

#if NET6_0_OR_GREATER
    static JoltApi()
    {
        NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), OnDllImport);
    }

    private static IntPtr OnDllImport(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (TryResolveLibrary(libraryName, assembly, searchPath, out IntPtr nativeLibrary))
        {
            return nativeLibrary;
        }

        return NativeLibrary.Load(libraryName, assembly, searchPath);
    }

    private static bool TryResolveLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath, out IntPtr nativeLibrary)
    {
        nativeLibrary = IntPtr.Zero;
        if (libraryName is not LibName)
            return false;

        string rid = RuntimeInformation.RuntimeIdentifier;

        string nugetNativeLibsPath = Path.Combine(AppContext.BaseDirectory, "runtimes", rid, "native");
        bool isNuGetRuntimeLibrariesDirectoryPresent = Directory.Exists(nugetNativeLibsPath);
        string dllName = LibName;

        if (OperatingSystem.IsWindows())
        {
            dllName = DoublePrecision ? $"{LibDoubleName}.dll" : $"{LibName}.dll";

            if (!isNuGetRuntimeLibrariesDirectoryPresent)
            {
                rid = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X64 => "win-x64",
                    Architecture.Arm64 => "win-arm64",
                    _ => "win-x64"
                };

                nugetNativeLibsPath = Path.Combine(AppContext.BaseDirectory, "runtimes", rid, "native");
                isNuGetRuntimeLibrariesDirectoryPresent = Directory.Exists(nugetNativeLibsPath);
            }
        }
        else if (OperatingSystem.IsLinux())
        {
            dllName = DoublePrecision ? $"lib{LibDoubleName}.so" : $"lib{LibName}.so";
        }
        else if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst())
        {
            dllName = DoublePrecision ? $"lib{LibDoubleName}.dylib" : $"lib{LibName}.dylib";
        }

        if (isNuGetRuntimeLibrariesDirectoryPresent)
        {
            string joltcPath = Path.Combine(AppContext.BaseDirectory, "runtimes", rid, "native", dllName);

            if (NativeLibrary.TryLoad(joltcPath, out nativeLibrary))
            {
                return true;
            }
        }
        else
        {
            if (NativeLibrary.TryLoad(LibName, assembly, searchPath, out nativeLibrary))
            {
                return true;
            }
        }

        nativeLibrary = IntPtr.Zero;
        return false;
    }
#else
    private static readonly ILibraryLoader s_loader = GetPlatformLoader();

    public static void LoadNativeLibrary()
    {
        string dllName = LibName;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            dllName = DoublePrecision ? $"{LibDoubleName}.dll" : $"{LibName}.dll";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.OSDescription.ToUpper().Contains("BSD"))
        {
            dllName = DoublePrecision ? $"lib{LibDoubleName}.dylib" : $"lib{LibName}.dylib";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            dllName = DoublePrecision ? $"lib{LibDoubleName}.so" : $"lib{LibName}.so";
        }

        foreach (string rid in RuntimeIdentifiers)
        {
            string nugetNativeLibsPath = Path.Combine(AppContext.BaseDirectory, "runtimes", rid, "native");
            bool isNuGetRuntimeLibrariesDirectoryPresent = Directory.Exists(nugetNativeLibsPath);

            if (isNuGetRuntimeLibrariesDirectoryPresent)
            {
                string joltcPath = Path.Combine(AppContext.BaseDirectory, "runtimes", rid, "native", dllName);

                nint handle = s_loader.LoadNativeLibrary(joltcPath);
                if (handle != 0)
                {
                    break;
                }
            }
            else
            {
                nint handle = s_loader.LoadNativeLibrary(dllName);
                if (handle != 0)
                {
                    break;
                }

                string joltcPath = Path.Combine(Directory.GetCurrentDirectory(), "runtimes", rid, "native", dllName);

                handle = s_loader.LoadNativeLibrary(joltcPath);
                if (handle != 0)
                {
                    break;
                }
            }
        }
    }
#endif

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_Init();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Shutdown();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_TempAllocatorMalloc_Create();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_TempAllocator_Create(uint size);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_TempAllocator_Destroy(IntPtr handle);


    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_JobSystemThreadPool_Create(uint maxJobs, uint maxBarriers, int inNumThreads);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_JobSystemThreadPool_Destroy(IntPtr handle);

    //  BroadPhaseLayerInterface
#if NET6_0_OR_GREATER
    public struct JPH_BroadPhaseLayerInterface_Procs
    {
        public delegate* unmanaged[Cdecl]<IntPtr, uint> GetNumBroadPhaseLayers;
        public delegate* unmanaged[Cdecl]<IntPtr, ObjectLayer, BroadPhaseLayer> GetBroadPhaseLayer;
        public delegate* unmanaged[Cdecl]<IntPtr, BroadPhaseLayer, IntPtr> GetBroadPhaseLayerName;
    }
#else
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint GetNumBroadPhaseLayersDelegate(IntPtr @this);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate BroadPhaseLayer GetBroadPhaseLayerDelegate(IntPtr @this, ObjectLayer layer);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr GetBroadPhaseLayerNameDelegate(IntPtr @this, BroadPhaseLayer layer);

    [StructLayout(LayoutKind.Sequential)]
    public struct JPH_BroadPhaseLayerInterface_Procs : IEquatable<JPH_BroadPhaseLayerInterface_Procs>
    {
        public GetNumBroadPhaseLayersDelegate GetNumBroadPhaseLayers;
        public GetBroadPhaseLayerDelegate GetBroadPhaseLayer;
        public GetBroadPhaseLayerNameDelegate GetBroadPhaseLayerName;

        public readonly bool Equals(JPH_BroadPhaseLayerInterface_Procs obj)
        {
            return
                GetNumBroadPhaseLayers == obj.GetNumBroadPhaseLayers &&
                GetBroadPhaseLayer == obj.GetBroadPhaseLayer &&
                GetBroadPhaseLayerName == obj.GetBroadPhaseLayerName;
        }

        public readonly override bool Equals(object obj) => obj is JPH_BroadPhaseLayerInterface_Procs f && Equals(f);

        public static bool operator ==(JPH_BroadPhaseLayerInterface_Procs left, JPH_BroadPhaseLayerInterface_Procs right) =>
            left.Equals(right);

        public static bool operator !=(JPH_BroadPhaseLayerInterface_Procs left, JPH_BroadPhaseLayerInterface_Procs right) =>
            !left.Equals(right);

        public override readonly int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(GetNumBroadPhaseLayers);
            hash.Add(GetBroadPhaseLayer);
            hash.Add(GetBroadPhaseLayerName);
            return hash.ToHashCode();
        }
    }
#endif

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BroadPhaseLayerInterface_SetProcs(JPH_BroadPhaseLayerInterface_Procs procs);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BroadPhaseLayerInterface_Create();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BroadPhaseLayerInterface_Destroy(nint handle);

    //  ObjectVsBroadPhaseLayerFilter
#if NET6_0_OR_GREATER
    public struct JPH_ObjectVsBroadPhaseLayerFilter_Procs
    {
        public delegate* unmanaged[Cdecl]<IntPtr, ObjectLayer, BroadPhaseLayer, uint> ShouldCollide;
    }
#else
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint ShouldCollideDelegate(IntPtr @this, ObjectLayer layer, BroadPhaseLayer broadPhaseLayer);

    [StructLayout(LayoutKind.Sequential)]
    public struct JPH_ObjectVsBroadPhaseLayerFilter_Procs : IEquatable<JPH_ObjectVsBroadPhaseLayerFilter_Procs>
    {
        public ShouldCollideDelegate ShouldCollide;

        public readonly bool Equals(JPH_ObjectVsBroadPhaseLayerFilter_Procs obj)
        {
            return ShouldCollide == obj.ShouldCollide;
        }

        public readonly override bool Equals(object obj) => obj is JPH_ObjectVsBroadPhaseLayerFilter_Procs f && Equals(f);

        public static bool operator ==(JPH_ObjectVsBroadPhaseLayerFilter_Procs left, JPH_ObjectVsBroadPhaseLayerFilter_Procs right) =>
            left.Equals(right);

        public static bool operator !=(JPH_ObjectVsBroadPhaseLayerFilter_Procs left, JPH_ObjectVsBroadPhaseLayerFilter_Procs right) =>
            !left.Equals(right);

        public override readonly int GetHashCode()
        {
            return ShouldCollide.GetHashCode();
        }
    }
#endif

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_ObjectVsBroadPhaseLayerFilter_SetProcs(JPH_ObjectVsBroadPhaseLayerFilter_Procs procs);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_ObjectVsBroadPhaseLayerFilter_Create();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_ObjectVsBroadPhaseLayerFilter_Destroy(nint handle);

    //  ObjectLayerPairFilter
#if NET6_0_OR_GREATER
    public struct JPH_ObjectLayerPairFilter_Procs
    {
        public delegate* unmanaged[Cdecl]<IntPtr, ObjectLayer, ObjectLayer, uint> ShouldCollide;
    }
#else
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint ObjectLayerPairFilterShouldCollideDelegate(IntPtr @this, ObjectLayer layer1, ObjectLayer layer2);

    [StructLayout(LayoutKind.Sequential)]
    public struct JPH_ObjectLayerPairFilter_Procs : IEquatable<JPH_ObjectLayerPairFilter_Procs>
    {
        public ObjectLayerPairFilterShouldCollideDelegate ShouldCollide;

        public readonly bool Equals(JPH_ObjectLayerPairFilter_Procs obj)
        {
            return ShouldCollide == obj.ShouldCollide;
        }

        public readonly override bool Equals(object obj) => obj is JPH_ObjectLayerPairFilter_Procs f && Equals(f);

        public static bool operator ==(JPH_ObjectLayerPairFilter_Procs left, JPH_ObjectLayerPairFilter_Procs right) =>
            left.Equals(right);

        public static bool operator !=(JPH_ObjectLayerPairFilter_Procs left, JPH_ObjectLayerPairFilter_Procs right) =>
            !left.Equals(right);

        public override readonly int GetHashCode()
        {
            return ShouldCollide.GetHashCode();
        }
    }
#endif

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_ObjectLayerPairFilter_SetProcs(JPH_ObjectLayerPairFilter_Procs procs);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_ObjectLayerPairFilter_Create();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_ObjectLayerPairFilter_Destroy(nint handle);

    /* ShapeSettings */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_ShapeSettings_Destroy(nint shape);

    /* BoxShapeSettings */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_BoxShapeSettings_Create(in Vector3 halfExtent, float convexRadius);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_BoxShape_Create(in Vector3 halfExtent, float convexRadius);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BoxShape_GetHalfExtent(IntPtr handle, out Vector3 halfExtent);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_BoxShape_GetVolume(IntPtr handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_BoxShape_GetConvexRadius(IntPtr handle);

    /* SphereShapeSettings */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_SphereShapeSettings_Create(float radius);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_SphereShapeSettings_GetRadius(IntPtr shape);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_SphereShapeSettings_SetRadius(IntPtr shape, float radius);

    /* TriangleShapeSettings */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_TriangleShapeSettings_Create(in Vector3 v1, in Vector3 v2, in Vector3 v3, float convexRadius);

    /* CapsuleShapeSettings */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_CapsuleShapeSettings_Create(float halfHeightOfCylinder, float radius);

    /* CylinderShapeSettings */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_CylinderShapeSettings_Create(float halfHeight, float radius, float convexRadius);

    /* ConvexHullShape */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_ConvexHullShapeSettings_Create(Vector3* points, int pointsCount, float maxConvexRadius);

    /* MeshShape  */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_MeshShapeSettings_Create(Triangle* triangle, int triangleCount);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_MeshShapeSettings_Create2(Vector3* vertices, int verticesCount, IndexedTriangle* triangles, int triangleCount);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_MeshShapeSettings_Sanitize(IntPtr shape);

    /* HeightFieldShape  */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_HeightFieldShapeSettings_Create(float* samples, in Vector3 offset, in Vector3 scale, int sampleCount);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_MeshShapeSettings_DetermineMinAndMaxSample(IntPtr settings, out float outMinValue, out float outMaxValue, out float outQuantizationScale);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_MeshShapeSettings_CalculateBitsPerSampleForError(IntPtr settings, float maxError);

    /* TaperedCapsuleShapeSettings */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_TaperedCapsuleShapeSettings_Create(float halfHeightOfTaperedCylinder, float topRadius, float bottomRadius);

    /* CompoundShape */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_CompoundShapeSettings_AddShape(nint handle, in Vector3 position, in Quaternion rotation, nint shapeSettings, uint userData);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_CompoundShapeSettings_AddShape2(nint handle, in Vector3 position, in Quaternion rotation, nint shape, uint userData);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_StaticCompoundShapeSettings_Create();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_MutableCompoundShapeSettings_Create();

    /* Shape */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Shape_Destroy(IntPtr shape);

    /* SphereShape */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_SphereShape_Create(float radius);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_SphereShape_GetRadius(IntPtr shape);

    /* BodyCreationSettings */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BodyCreationSettings_Create();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BodyCreationSettings_Create2(IntPtr shapeSettings, in Vector3 position, in Quaternion rotation, MotionType motionType, ushort objectLayer);

    [DllImport(LibName, EntryPoint = nameof(JPH_BodyCreationSettings_Create2), CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BodyCreationSettings_Create2_Double(IntPtr shapeSettings, in Double3 position, in Quaternion rotation, MotionType motionType, ushort objectLayer);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BodyCreationSettings_Create3(IntPtr shape, in Vector3 position, in Quaternion rotation, MotionType motionType, ushort objectLayer);

    [DllImport(LibName, EntryPoint = nameof(JPH_BodyCreationSettings_Create3), CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BodyCreationSettings_Create3_Double(IntPtr shape, in Double3 position, in Quaternion rotation, MotionType motionType, ushort objectLayer);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyCreationSettings_Destroy(IntPtr settings);

    /* JPH_ConstraintSettings */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_ConstraintSettings_Destroy(IntPtr handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Constraint_Destroy(IntPtr handle);

    /* JPH_PointConstraintSettings */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_PointConstraintSettings_Create();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ConstraintSpace JPH_PointConstraintSettings_GetSpace(IntPtr handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PointConstraintSettings_SetSpace(IntPtr handle, ConstraintSpace value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PointConstraintSettings_GetPoint1(IntPtr handle, out Vector3 velocity);

    [DllImport(LibName, EntryPoint = nameof(JPH_PointConstraintSettings_GetPoint1), CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PointConstraintSettings_GetPoint1_Double(IntPtr handle, out Double3 velocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PointConstraintSettings_SetPoint1(IntPtr handle, in Vector3 velocity);

    [DllImport(LibName, EntryPoint = nameof(JPH_PointConstraintSettings_SetPoint1), CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PointConstraintSettings_SetPoint1_Double(IntPtr handle, in Double3 velocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PointConstraintSettings_GetPoint2(IntPtr handle, out Vector3 velocity);

    [DllImport(LibName, EntryPoint = nameof(JPH_PointConstraintSettings_GetPoint2), CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PointConstraintSettings_GetPoint2_Double(IntPtr handle, out Double3 velocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PointConstraintSettings_SetPoint2(IntPtr handle, in Vector3 velocity);

    [DllImport(LibName, EntryPoint = nameof(JPH_PointConstraintSettings_SetPoint2), CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PointConstraintSettings_SetPoint2_Double(IntPtr handle, in Double3 velocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_PointConstraintSettings_CreateConstraint(IntPtr handle, IntPtr body1, IntPtr body2);

    /* PhysicsSystem */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_PhysicsSystem_Create();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PhysicsSystem_Destroy(IntPtr handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PhysicsSystem_Init(IntPtr handle,
        uint inMaxBodies, uint numBodyMutexes, uint maxBodyPairs, uint maxContactConstraints,
        IntPtr layer,
        IntPtr inObjectVsBroadPhaseLayerFilter,
        IntPtr inObjectLayerPairFilter);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PhysicsSystem_OptimizeBroadPhase(IntPtr handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern PhysicsUpdateError JPH_PhysicsSystem_Update(IntPtr handle,
        float deltaTime, int collisionSteps, int integrationSubSteps,
        IntPtr tempAlocator, IntPtr jobSystem);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PhysicsSystem_SetContactListener(IntPtr system, IntPtr listener);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PhysicsSystem_SetBodyActivationListener(IntPtr system, IntPtr listener);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_PhysicsSystem_GetNumBodies(IntPtr system);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_PhysicsSystem_GetNumActiveBodies(IntPtr system);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_PhysicsSystem_GetMaxBodies(IntPtr system);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PhysicsSystem_GetGravity(IntPtr handle, out Vector3 velocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PhysicsSystem_SetGravity(IntPtr handle, in Vector3 velocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PhysicsSystem_AddConstraint(IntPtr handle, IntPtr constraint);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PhysicsSystem_RemoveConstraint(IntPtr handle, IntPtr constraint);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PhysicsSystem_AddConstraints(IntPtr handle, IntPtr* constraints, uint count);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PhysicsSystem_RemoveConstraints(IntPtr handle, IntPtr* constraints, uint count);


    /* BodyInterface */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_PhysicsSystem_GetBodyInterface(IntPtr system);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BodyInterface_CreateBody(IntPtr handle, IntPtr settings);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_BodyInterface_CreateAndAddBody(IntPtr handle, IntPtr bodyID, ActivationMode activation);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BodyInterface_CreateBodyWithID(IntPtr handle, uint bodyID, IntPtr settings);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BodyInterface_CreateBodyWithoutID(IntPtr handle, IntPtr settings);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_DestroyBody(IntPtr handle, uint bodyID);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_DestroyBodyWithoutID(IntPtr handle, IntPtr body);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_BodyInterface_AssignBodyID(IntPtr handle, IntPtr body);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_BodyInterface_AssignBodyID2(IntPtr handle, IntPtr body, uint bodyID);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BodyInterface_UnassignBodyID(IntPtr handle, uint bodyID);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_AddBody(IntPtr handle, uint bodyID, ActivationMode activation);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_RemoveBody(IntPtr handle, uint bodyID);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_GetLinearVelocity(IntPtr handle, uint bodyID, out Vector3 velocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetLinearVelocity(IntPtr handle, uint bodyID, in Vector3 velocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_GetCenterOfMassPosition(IntPtr handle, uint bodyID, out Vector3 velocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_BodyInterface_IsActive(IntPtr handle, uint bodyID);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_BodyInterface_IsAdded(IntPtr handle, uint bodyID);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern MotionType JPH_BodyInterface_GetMotionType(IntPtr handle, uint bodyID);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetMotionType(IntPtr handle, uint bodyID, MotionType motionType, ActivationMode activationMode);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_BodyInterface_GetRestitution(IntPtr handle, uint bodyID);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetRestitution(IntPtr handle, uint bodyID, float value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_BodyInterface_GetFriction(IntPtr handle, uint bodyID);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetFriction(IntPtr handle, uint bodyID, float value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetPosition(IntPtr handle, uint bodyId, in Vector3 position, ActivationMode activationMode);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_GetPosition(IntPtr handle, uint bodyId, out Vector3 position);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetRotation(IntPtr handle, uint bodyId, in Quaternion rotation, ActivationMode activationMode);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_GetRotation(IntPtr handle, uint bodyId, out Quaternion rotation);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetPositionAndRotation(IntPtr handle, uint bodyID, in Vector3 position, in Quaternion rotation, ActivationMode activationMode);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetPositionAndRotationWhenChanged(IntPtr handle, uint bodyID, in Vector3 position, in Quaternion rotation, ActivationMode activationMode);

[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetPositionRotationAndVelocity(IntPtr handle, uint bodyID, in Vector3 position, in Quaternion rotation, in Vector3 linearVelocity, in Vector3 angularVelocity);

    /* Body */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_Body_GetID(IntPtr body);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_Body_IsActive(IntPtr handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_Body_IsStatic(IntPtr handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_Body_IsKinematic(IntPtr handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_Body_IsDynamic(IntPtr handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_Body_IsSensor(IntPtr handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern MotionType JPH_Body_GetMotionType(IntPtr handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_SetMotionType(IntPtr handle, MotionType motionType);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_Body_GetFriction(IntPtr handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_SetFriction(IntPtr handle, float value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_Body_GetRestitution(IntPtr handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_SetRestitution(IntPtr handle, float value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_GetPosition(IntPtr handle, out Vector3 result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_GetRotation(IntPtr handle, out Quaternion result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_GetCenterOfMassPosition(IntPtr handle, out Vector3 result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_GetWorldTransform(IntPtr handle, out Matrix4x4 result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_GetCenterOfMassTransform(IntPtr handle, out Matrix4x4 result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_GetLinearVelocity(IntPtr handle, out Vector3 result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_SetLinearVelocity(IntPtr handle, in Vector3 velocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_GetAngularVelocity(IntPtr handle, out Vector3 velocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_SetAngularVelocity(IntPtr handle, in Vector3 velocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_AddForce(IntPtr handle, in Vector3 velocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_AddForceAtPosition(IntPtr handle, in Vector3 velocity, in Vector3 position);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_AddTorque(IntPtr handle, in Vector3 value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_GetAccumulatedForce(IntPtr handle, out Vector3 force);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_GetAccumulatedTorque(IntPtr handle, out Vector3 force);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_AddImpulse(IntPtr handle, in Vector3 impulse);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_AddImpulseAtPosition(IntPtr handle, in Vector3 impulse, in Vector3 position);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_AddAngularImpulse(IntPtr handle, in Vector3 angularImpulse);

    // ContactListener
#if NET6_0_OR_GREATER
    public struct JPH_ContactListener_Procs
    {
        public delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, Vector3*, IntPtr, uint> OnContactValidate;
        public delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, void> OnContactAdded;
        public delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, void> OnContactPersisted;
        public delegate* unmanaged[Cdecl]<IntPtr, SubShapeIDPair*, void> OnContactRemoved;
    }
#else
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint OnContactValidateDelegate(IntPtr @this, IntPtr body1, IntPtr body2, Vector3* baseOffset, IntPtr collisionResult);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void OnContactAddedDelegate(IntPtr @this, IntPtr body1, IntPtr body2);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void OnContactPersistedDelegate(IntPtr @this, IntPtr body1, IntPtr body2);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void OnContactRemovedDelegate(IntPtr @this, SubShapeIDPair* subShapePair);

    [StructLayout(LayoutKind.Sequential)]
    public struct JPH_ContactListener_Procs : IEquatable<JPH_ContactListener_Procs>
    {
        public OnContactValidateDelegate OnContactValidate;
        public OnContactAddedDelegate OnContactAdded;
        public OnContactPersistedDelegate OnContactPersisted;
        public OnContactRemovedDelegate OnContactRemoved;

        public readonly bool Equals(JPH_ContactListener_Procs obj)
        {
            return
                OnContactValidate == obj.OnContactValidate &&
                OnContactAdded == obj.OnContactAdded &&
                OnContactPersisted == obj.OnContactPersisted &&
                OnContactRemoved == obj.OnContactRemoved;
        }

        public readonly override bool Equals(object obj) => obj is JPH_ContactListener_Procs f && Equals(f);

        public static bool operator ==(JPH_ContactListener_Procs left, JPH_ContactListener_Procs right) =>
            left.Equals(right);

        public static bool operator !=(JPH_ContactListener_Procs left, JPH_ContactListener_Procs right) =>
            !left.Equals(right);

        public override readonly int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(OnContactValidate);
            hash.Add(OnContactAdded);
            hash.Add(OnContactPersisted);
            hash.Add(OnContactRemoved);
            return hash.ToHashCode();
        }
    }
#endif

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_ContactListener_SetProcs(JPH_ContactListener_Procs procs);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_ContactListener_Create();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_ContactListener_Destroy(IntPtr handle);

    // BodyActivationListener
#if NET6_0_OR_GREATER
    public struct JPH_BodyActivationListener_Procs
    {
        public delegate* unmanaged[Cdecl]<IntPtr, uint, ulong, void> OnBodyActivated;
        public delegate* unmanaged[Cdecl]<IntPtr, uint, ulong, void> OnBodyDeactivated;
    }
#else
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void OnBodyActivatedDelegate(IntPtr @this, uint bodyID, ulong bodyUserData);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void OnBodyDeactivatedDelegate(IntPtr @this, uint bodyID, ulong bodyUserData);

    [StructLayout(LayoutKind.Sequential)]
    public struct JPH_BodyActivationListener_Procs
    {
        public OnBodyActivatedDelegate OnBodyActivated;
        public OnBodyDeactivatedDelegate OnBodyDeactivated;
    }
#endif

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyActivationListener_SetProcs(JPH_BodyActivationListener_Procs procs);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BodyActivationListener_Create();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyActivationListener_Destroy(IntPtr handle);

#if !NET6_0_OR_GREATER
    private static IEnumerable<string> RuntimeIdentifiers
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string rid = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X86 => "win10-x86",
                    Architecture.X64 => "win10-x64",
                    Architecture.Arm => "win10-arm",
                    Architecture.Arm64 => "win10-arm64",
                    _ => "win10-x64"
                };

                yield return rid;

                rid = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X86 => "win-x86",
                    Architecture.X64 => "win-x64",
                    Architecture.Arm => "win-arm",
                    Architecture.Arm64 => "win-arm64",
                    _ => "win-x64"
                };

                yield return rid;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.OSDescription.ToUpper().Contains("BSD"))
            {
                yield return "osx-universal";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string rid = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X86 => "linux-x86",
                    Architecture.X64 => "linux-x64",
                    Architecture.Arm => "linux-arm",
                    Architecture.Arm64 => "linux-arm64",
                    _ => "linux-x64"
                };

                yield return rid;
            }

            yield break;
        }
    }

    private static ILibraryLoader GetPlatformLoader()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new Win32LibraryLoader();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
            RuntimeInformation.OSDescription.ToUpper().Contains("BSD"))
        {
            return new BsdLibraryLoader();
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return new UnixLibraryLoader();
        }

        throw new PlatformNotSupportedException("This platform cannot load native libraries.");
    }

    interface ILibraryLoader
    {
        nint LoadNativeLibrary(string name);
        void FreeNativeLibrary(nint handle);

        nint LoadFunctionPointer(nint handle, string name);
    }

    private class Win32LibraryLoader : ILibraryLoader
    {
        public nint LoadNativeLibrary(string name)
        {
            return LoadLibrary(name);
        }

        public void FreeNativeLibrary(nint handle)
        {
            FreeLibrary(handle);
        }

        public nint LoadFunctionPointer(nint handle, string name)
        {
            return GetProcAddress(handle, name);
        }

        [DllImport("kernel32")]
        private static extern nint LoadLibrary(string fileName);

        [DllImport("kernel32")]
        private static extern int FreeLibrary(nint module);

        [DllImport("kernel32")]
        private static extern nint GetProcAddress(nint module, string procName);
    }

    private class UnixLibraryLoader : ILibraryLoader
    {
        public nint LoadNativeLibrary(string name)
        {
            return Libdl.dlopen(name, Libdl.RTLD_NOW | Libdl.RTLD_LOCAL);
        }

        public void FreeNativeLibrary(nint handle)
        {
            Libdl.dlclose(handle);
        }

        public nint LoadFunctionPointer(nint handle, string name)
        {
            return Libdl.dlsym(handle, name);
        }
    }

    private class BsdLibraryLoader : ILibraryLoader
    {
        public nint LoadNativeLibrary(string name)
        {
            return Libc.dlopen(name, Libc.RTLD_NOW | Libc.RTLD_LOCAL);
        }

        public void FreeNativeLibrary(nint handle)
        {
            Libc.dlclose(handle);
        }

        public nint LoadFunctionPointer(nint handle, string name)
        {
            return Libc.dlsym(handle, name);
        }
    }

    internal static class Libdl
    {
        private const string LibName = "libdl";

        public const int RTLD_LOCAL = 0x000;
        public const int RTLD_NOW = 0x002;

        [DllImport(LibName)]
        public static extern nint dlopen(string fileName, int flags);

        [DllImport(LibName)]
        public static extern nint dlsym(nint handle, string name);

        [DllImport(LibName)]
        public static extern int dlclose(nint handle);

        [DllImport(LibName)]
        public static extern string dlerror();
    }

    internal static class Libc
    {
        private const string LibName = "libc";

        public const int RTLD_LOCAL = 0x000;
        public const int RTLD_NOW = 0x002;

        [DllImport(LibName)]
        public static extern nint dlopen(string fileName, int flags);

        [DllImport(LibName)]
        public static extern nint dlsym(nint handle, string name);

        [DllImport(LibName)]
        public static extern int dlclose(nint handle);

        [DllImport(LibName)]
        public static extern string dlerror();
    }
#endif
}
