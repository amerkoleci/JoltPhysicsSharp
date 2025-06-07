// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace JoltPhysicsSharp;

using JPH_ObjectLayer = uint;
using unsafe JPH_CastRayCollector = delegate* unmanaged<nint, RayCastResult*, float>;
using unsafe JPH_CastRayResultCallback = delegate* unmanaged<nint, RayCastResult*, void>;
using unsafe JPH_CastShapeCollector = delegate* unmanaged<nint, ShapeCastResult*, float>;
using unsafe JPH_CastShapeResultCallback = delegate* unmanaged<nint, ShapeCastResult*, void>;
using unsafe JPH_CollidePointCollector = delegate* unmanaged<nint, CollidePointResult*, float>;
using unsafe JPH_CollidePointResultCallback = delegate* unmanaged<nint, CollidePointResult*, void>;
//public delegate float RayCastBodyCollector(nint userData, in BroadPhaseCastResult result);
using unsafe JPH_CollideShapeBodyCollectorCallback = delegate* unmanaged<nint, BodyID*, float>;
using unsafe JPH_CollideShapeCollector = delegate* unmanaged<nint, CollideShapeResult*, float>;
using unsafe JPH_CollideShapeResultCallback = delegate* unmanaged<nint, CollideShapeResult*, void>;
using unsafe JPH_QueueJobCallback = delegate* unmanaged<nint, /*JPH_JobFunction*/delegate* unmanaged<nint, void>, nint, void>;
using unsafe JPH_QueueJobsCallback = delegate* unmanaged<nint, /*JPH_JobFunction**/delegate* unmanaged<nint, void>, void**, uint, void>;
using unsafe JPH_RayCastBodyCollectorCallback = delegate* unmanaged<nint, BroadPhaseCastResult*, float>;

internal static unsafe partial class JoltApi
{
    private const DllImportSearchPath DefaultDllImportSearchPath = DllImportSearchPath.ApplicationDirectory | DllImportSearchPath.UserDirectories | DllImportSearchPath.UseDllDirectoryForDependencies;

    /// <summary>
    /// Raised whenever a native library is loaded by Jolt.
    /// Handlers can be added to this event to customize how libraries are loaded, and they will be used first whenever a new native library is being resolved.
    /// </summary>
    public static event DllImportResolver? JoltDllImporterResolver;

    private const string LibName = "joltc";
    private const string LibDoubleName = "joltc_double";

    public static bool DoublePrecision { get; set; }

    static JoltApi()
    {
        NativeLibrary.SetDllImportResolver(typeof(JoltApi).Assembly, OnDllImport);
    }

    private static nint OnDllImport(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName != LibName)
        {
            return nint.Zero;
        }

        nint nativeLibrary = nint.Zero;
        DllImportResolver? resolver = JoltDllImporterResolver;
        if (resolver != null)
        {
            nativeLibrary = resolver(libraryName, assembly, searchPath);
        }

        if (nativeLibrary != nint.Zero)
        {
            return nativeLibrary;
        }

        if (OperatingSystem.IsWindows())
        {
            string dllName = DoublePrecision ? $"{LibDoubleName}.dll" : $"{LibName}.dll";

            if (NativeLibrary.TryLoad(dllName, assembly, searchPath, out nativeLibrary))
            {
                return nativeLibrary;
            }
        }
        else if (OperatingSystem.IsLinux())
        {
            string dllName = DoublePrecision ? $"lib{LibDoubleName}.so" : $"lib{LibName}.so";

            if (NativeLibrary.TryLoad(dllName, assembly, searchPath, out nativeLibrary))
            {
                return nativeLibrary;
            }
        }
        else if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst())
        {
            string dllName = DoublePrecision ? $"lib{LibDoubleName}.dylib" : $"lib{LibName}.dylib";

            if (NativeLibrary.TryLoad(dllName, assembly, searchPath, out nativeLibrary))
            {
                return nativeLibrary;
            }
        }

        string libraryLoadName = DoublePrecision ? $"lib{LibDoubleName}" : $"lib{LibName}";

        if (NativeLibrary.TryLoad(libraryLoadName, assembly, searchPath, out nativeLibrary))
        {
            return nativeLibrary;
        }

        libraryLoadName = DoublePrecision ? LibDoubleName : LibName;
        if (NativeLibrary.TryLoad(libraryLoadName, assembly, searchPath, out nativeLibrary))
        {
            return nativeLibrary;
        }

        return nint.Zero;
    }

    /// <summary>Converts an unmanaged string to a managed version.</summary>
    /// <param name="unmanaged">The unmanaged string to convert.</param>
    /// <returns>A managed string.</returns>
    public static string? ConvertToManaged(byte* unmanaged)
    {
        if (unmanaged == null)
            return null;

        return UTF8EncodingRelaxed.Default.GetString(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(unmanaged));
    }

    /// <summary>Converts an unmanaged string to a managed version.</summary>
    /// <param name="unmanaged">The unmanaged string to convert.</param>
    /// <returns>A managed string.</returns>
    public static string? ConvertToManaged(byte* unmanaged, int maxLength)
    {
        if (unmanaged == null)
            return null;

        var span = new ReadOnlySpan<byte>(unmanaged, maxLength);
        var indexOfZero = span.IndexOf((byte)0);
        return indexOfZero < 0 ? UTF8EncodingRelaxed.Default.GetString(span) : UTF8EncodingRelaxed.Default.GetString(span.Slice(0, indexOfZero));
    }


    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Init();

    [LibraryImport(LibName)]
    public static partial void JPH_Shutdown();

    [LibraryImport(LibName)]
    public static partial void JPH_SetTraceHandler(delegate* unmanaged<byte*, void> callback);

    [LibraryImport(LibName)]
    public static partial void JPH_SetAssertFailureHandler(delegate* unmanaged<byte*, byte*, byte*, uint, Bool8> callback);

    // JobSystem
#pragma warning disable CS0649
    public struct JPH_JobSystemConfig
    {
        public nint context;
        public JPH_QueueJobCallback queueJob;
        public JPH_QueueJobsCallback queueJobs;
        public uint maxConcurrency;
        public uint maxBarriers;
    }
#pragma warning restore CS0649

    [LibraryImport(LibName)]
    public static partial nint JPH_JobSystemThreadPool_Create(JobSystemThreadPoolConfig* config);

    [LibraryImport(LibName)]
    public static partial nint JPH_JobSystemThreadPool_Create(in JobSystemThreadPoolConfig config);

    [LibraryImport(LibName)]
    public static partial nint JPH_JobSystemCallback_Create(JPH_JobSystemConfig* config);

    // JobSystem
    [LibraryImport(LibName)]
    public static partial void JPH_JobSystem_Destroy(nint jobSystem);

    // BroadPhaseLayerInterface
    [LibraryImport(LibName)]
    public static partial nint JPH_BroadPhaseLayerInterfaceMask_Create(uint numBroadPhaseLayers);

    [LibraryImport(LibName)]
    public static partial void JPH_BroadPhaseLayerInterfaceMask_ConfigureLayer(nint bpInterface, in BroadPhaseLayer broadPhaseLayer, uint groupsToInclude, uint groupsToExclude);


    [LibraryImport(LibName)]
    public static partial nint JPH_BroadPhaseLayerInterfaceTable_Create(uint numObjectLayers, uint numBroadPhaseLayers);

    [LibraryImport(LibName)]
    public static partial void JPH_BroadPhaseLayerInterfaceTable_MapObjectToBroadPhaseLayer(nint bpInterface, ObjectLayer objectLayer, BroadPhaseLayer broadPhaseLayer);

    //  ObjectVsBroadPhaseLayerFilter
    [LibraryImport(LibName)]
    public static partial nint JPH_ObjectVsBroadPhaseLayerFilterMask_Create(nint broadPhaseLayerInterface);

    [LibraryImport(LibName)]
    public static partial nint JPH_ObjectVsBroadPhaseLayerFilterTable_Create(nint broadPhaseLayerInterface, uint numBroadPhaseLayers, nint objectLayerPairFilter, uint numObjectLayers);

    [LibraryImport(LibName)]
    public static partial void JPH_ObjectVsBroadPhaseLayerFilter_Destroy(nint handle);

    #region JPH_ObjectLayerPairFilter
    [LibraryImport(LibName)]
    public static partial nint JPH_ObjectLayerPairFilterMask_Create();

    [LibraryImport(LibName)]
    public static partial ObjectLayer JPH_ObjectLayerPairFilterMask_GetObjectLayer(uint group, uint mask);

    [LibraryImport(LibName)]
    public static partial uint JPH_ObjectLayerPairFilterMask_GetGroup(in ObjectLayer layer);

    [LibraryImport(LibName)]
    public static partial uint JPH_ObjectLayerPairFilterMask_GetMask(in ObjectLayer layer);

    [LibraryImport(LibName)]
    public static partial nint JPH_ObjectLayerPairFilterTable_Create(uint numObjectLayers);

    [LibraryImport(LibName)]
    public static partial void JPH_ObjectLayerPairFilterTable_DisableCollision(nint objectFilter, ObjectLayer layer1, ObjectLayer layer2);

    [LibraryImport(LibName)]
    public static partial void JPH_ObjectLayerPairFilterTable_EnableCollision(nint objectFilter, ObjectLayer layer1, ObjectLayer layer2);
    #endregion

    //  ObjectLayerPairFilterTable


    //  BroadPhaseLayerFilter
    public struct JPH_BroadPhaseLayerFilter_Procs
    {
        public delegate* unmanaged<nint, BroadPhaseLayer, Bool8> ShouldCollide;
    }

    [LibraryImport(LibName)]
    public static partial void JPH_BroadPhaseLayerFilter_SetProcs(in JPH_BroadPhaseLayerFilter_Procs procs);

    [LibraryImport(LibName)]
    public static partial nint JPH_BroadPhaseLayerFilter_Create(nint userData);

    [LibraryImport(LibName)]
    public static partial void JPH_BroadPhaseLayerFilter_Destroy(nint handle);

    //  ObjectLayerFilter
    public struct JPH_ObjectLayerFilter_Procs
    {
        public delegate* unmanaged<nint, ObjectLayer, Bool8> ShouldCollide;
    }

    [LibraryImport(LibName)]
    public static partial void JPH_ObjectLayerFilter_SetProcs(in JPH_ObjectLayerFilter_Procs procs);

    [LibraryImport(LibName)]
    public static partial nint JPH_ObjectLayerFilter_Create(nint userData);

    [LibraryImport(LibName)]
    public static partial void JPH_ObjectLayerFilter_Destroy(nint handle);

    //  BodyFilter
    public struct JPH_BodyFilter_Procs
    {
        public delegate* unmanaged<nint, BodyID, Bool8> ShouldCollide;
        public delegate* unmanaged<nint, nint, Bool8> ShouldCollideLocked;
    }

    [LibraryImport(LibName)]
    public static partial void JPH_BodyFilter_SetProcs(in JPH_BodyFilter_Procs procs);

    [LibraryImport(LibName)]
    public static partial nint JPH_BodyFilter_Create(nint userData);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyFilter_Destroy(nint handle);

    //  ShapeFilter
    public struct JPH_ShapeFilter_Procs
    {
        public delegate* unmanaged<nint, nint, SubShapeID*, Bool8> ShouldCollide;
        public delegate* unmanaged<nint, nint, SubShapeID*, nint, SubShapeID*, Bool8> ShouldCollide2;
    }

    [LibraryImport(LibName)]
    public static partial void JPH_ShapeFilter_SetProcs(in JPH_ShapeFilter_Procs procs);

    [LibraryImport(LibName)]
    public static partial nint JPH_ShapeFilter_Create(nint userData);

    [LibraryImport(LibName)]
    public static partial void JPH_ShapeFilter_Destroy(nint handle);

    [LibraryImport(LibName)]
    public static partial uint JPH_ShapeFilter_GetBodyID2(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_ShapeFilter_SetBodyID2(nint handle, uint id);

    //  SimShapeFilter
    public struct JPH_SimShapeFilter_Procs
    {
        public delegate* unmanaged<nint, nint, nint, SubShapeID*, nint, nint, SubShapeID*, Bool8> ShouldCollide;
    }

    [LibraryImport(LibName)]
    public static partial void JPH_SimShapeFilter_SetProcs(in JPH_SimShapeFilter_Procs procs);

    [LibraryImport(LibName)]
    public static partial nint JPH_SimShapeFilter_Create(nint userData);

    [LibraryImport(LibName)]
    public static partial void JPH_SimShapeFilter_Destroy(nint handle);

    //  BodyDrawFilter
    public struct JPH_BodyDrawFilter_Procs
    {
        public delegate* unmanaged<nint, nint, Bool8> ShouldDraw;
    }

    [LibraryImport(LibName)]
    public static partial void JPH_BodyDrawFilter_SetProcs(in JPH_BodyDrawFilter_Procs procs);

    [LibraryImport(LibName)]
    public static partial nint JPH_BodyDrawFilter_Create(nint userData);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyDrawFilter_Destroy(nint handle);

    #region GroupFilter
    public struct JPH_CollisionGroup
    {
        public nint groupFilter;
        public CollisionGroupID groupID;
        public CollisionSubGroupID subGroupID;
    }

    [LibraryImport(LibName)]
    public static partial void JPH_GroupFilter_Destroy(nint groupFilter);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_GroupFilter_CanCollide(nint groupFilter, JPH_CollisionGroup* group1, JPH_CollisionGroup* group2);

    [LibraryImport(LibName)]
    public static partial nint JPH_GroupFilterTable_Create(uint numSubGroups/* = 0*/);
    [LibraryImport(LibName)]
    public static partial void JPH_GroupFilterTable_DisableCollision(nint table, CollisionSubGroupID subGroup1, CollisionSubGroupID subGroup2);
    [LibraryImport(LibName)]
    public static partial void JPH_GroupFilterTable_EnableCollision(nint table, CollisionSubGroupID subGroup1, CollisionSubGroupID subGroup2);
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_GroupFilterTable_IsCollisionEnabled(nint table, CollisionSubGroupID subGroup1, CollisionSubGroupID subGroup2);
    #endregion

    #region Shape/ShapeSettings
    [LibraryImport(LibName)]
    public static partial void JPH_ShapeSettings_Destroy(nint settings);

    [LibraryImport(LibName)]
    public static partial ulong JPH_ShapeSettings_GetUserData(nint settings);

    [LibraryImport(LibName)]
    public static partial void JPH_ShapeSettings_SetUserData(nint settings, ulong userData);

    [LibraryImport(LibName)]
    public static partial void JPH_Shape_Destroy(nint shape);

    [LibraryImport(LibName)]
    public static partial ShapeType JPH_Shape_GetType(nint shape);

    [LibraryImport(LibName)]
    public static partial ShapeSubType JPH_Shape_GetSubType(nint shape);

    [LibraryImport(LibName)]
    public static partial ulong JPH_Shape_GetUserData(nint shape);

    [LibraryImport(LibName)]
    public static partial void JPH_Shape_SetUserData(nint shape, ulong userData);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Shape_MustBeStatic(nint shape);

    [LibraryImport(LibName)]
    public static partial void JPH_Shape_GetCenterOfMass(nint handle, out Vector3 result);

    [LibraryImport(LibName)]
    public static partial void JPH_Shape_GetLocalBounds(nint shape, out BoundingBox box);

    [LibraryImport(LibName)]
    public static partial uint JPH_Shape_GetSubShapeIDBitsRecursive(nint shape);

    [LibraryImport(LibName)]
    public static partial float JPH_Shape_GetInnerRadius(nint handle);

    [LibraryImport(LibName)]
    public static partial float JPH_Shape_GetVolume(nint handle);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Shape_IsValidScale(nint handle, in Vector3 scale);

    [LibraryImport(LibName)]
    public static partial void JPH_Shape_MakeScaleValid(nint handle, in Vector3 scale, out Vector3 result);

    [LibraryImport(LibName)]
    public static partial nint JPH_Shape_ScaleShape(nint handle, in Vector3 scale);

    [LibraryImport(LibName)]
    public static partial void JPH_Shape_GetMassProperties(nint shape, out MassProperties properties);

    [LibraryImport(LibName)]
    public static partial void JPH_Shape_GetWorldSpaceBounds(nint shape, in Matrix4x4 centerOfMassTransform, in Vector3 scale, out BoundingBox result);

    [LibraryImport(LibName)]
    public static partial void JPH_Shape_GetWorldSpaceBounds(nint shape, in RMatrix4x4 centerOfMassTransform, in Vector3 scale, out BoundingBox result);

    [LibraryImport(LibName)]
    public static partial nint JPH_Shape_GetMaterial(nint shape, SubShapeID subShapeID);

    [LibraryImport(LibName)]
    public static partial void JPH_Shape_GetSurfaceNormal(nint shape, SubShapeID subShapeID, in Vector3 localPosition, out Vector3 normal);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Shape_CastRay(nint shape, in Vector3 origin, in Vector3 direction, out RayCastResult hit);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Shape_CastRay2(nint shape, in Vector3 origin, in Vector3 direction, in RayCastSettings settings, CollisionCollectorType collectorType, JPH_CastRayResultCallback callback, nint userData, nint shapeFilter);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Shape_CollidePoint(nint shape, in Vector3 point, nint shapeFilter);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Shape_CollidePoint2(nint shape, in Vector3 point, CollisionCollectorType collectorType, JPH_CollidePointResultCallback callback, nint userData, nint shapeFilter);
    #endregion

    #region ConvexShape
    [LibraryImport(LibName)]
    public static partial float JPH_ConvexShape_GetDensity(nint shape);

    [LibraryImport(LibName)]
    public static partial void JPH_ConvexShape_SetDensity(nint shape, float value);
    #endregion

    #region BoxShape/BoxShapeSettings
    [LibraryImport(LibName)]
    public static partial nint JPH_BoxShapeSettings_Create(in Vector3 halfExtent, float convexRadius);

    [LibraryImport(LibName)]
    public static partial nint JPH_BoxShapeSettings_CreateShape(nint settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_BoxShape_Create(in Vector3 halfExtent, float convexRadius);

    [LibraryImport(LibName)]
    public static partial void JPH_BoxShape_GetHalfExtent(nint handle, out Vector3 halfExtent);

    [LibraryImport(LibName)]
    public static partial float JPH_BoxShape_GetConvexRadius(nint handle);
    #endregion

    #region SphereShape/SphereShapeSettings
    [LibraryImport(LibName)]
    public static partial nint JPH_SphereShapeSettings_Create(float radius);

    [LibraryImport(LibName)]
    public static partial nint JPH_SphereShapeSettings_CreateShape(nint settings);

    [LibraryImport(LibName)]
    public static partial float JPH_SphereShapeSettings_GetRadius(nint shape);

    [LibraryImport(LibName)]
    public static partial void JPH_SphereShapeSettings_SetRadius(nint shape, float radius);

    [LibraryImport(LibName)]
    public static partial nint JPH_SphereShape_Create(float radius);

    [LibraryImport(LibName)]
    public static partial float JPH_SphereShape_GetRadius(nint shape);
    #endregion

    #region PlaneShape/PlaneShapeSettings
    [LibraryImport(LibName)]
    public static partial nint JPH_PlaneShapeSettings_Create(in Plane plane, nint material, float halfExtent);

    [LibraryImport(LibName)]
    public static partial nint JPH_PlaneShapeSettings_CreateShape(nint settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_PlaneShape_Create(in Plane plane, nint material, float halfExtent);

    [LibraryImport(LibName)]
    public static partial void JPH_PlaneShape_GetPlane(nint handle, out Plane result);

    [LibraryImport(LibName)]
    public static partial float JPH_PlaneShape_GetHalfExtent(nint handle);
    #endregion

    #region TriangleShape/TriangleShapeSettings
    [LibraryImport(LibName)]
    public static partial nint JPH_TriangleShapeSettings_Create(in Vector3 v1, in Vector3 v2, in Vector3 v3, float convexRadius);

    [LibraryImport(LibName)]
    public static partial nint JPH_TriangleShapeSettings_CreateShape(nint settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_TriangleShape_Create(in Vector3 v1, in Vector3 v2, in Vector3 v3, float convexRadius);

    [LibraryImport(LibName)]
    public static partial float JPH_TriangleShape_GetConvexRadius(nint shape);

    [LibraryImport(LibName)]
    public static partial void JPH_TriangleShape_GetVertex1(nint handle, Vector3* result);

    [LibraryImport(LibName)]
    public static partial void JPH_TriangleShape_GetVertex2(nint handle, Vector3* result);

    [LibraryImport(LibName)]
    public static partial void JPH_TriangleShape_GetVertex3(nint handle, Vector3* result);
    #endregion

    #region CapsuleShape/CapsuleShapeSettings
    [LibraryImport(LibName)]
    public static partial nint JPH_CapsuleShapeSettings_Create(float halfHeightOfCylinder, float radius);

    [LibraryImport(LibName)]
    public static partial nint JPH_CapsuleShapeSettings_CreateShape(nint settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_CapsuleShape_Create(float halfHeightOfCylinder, float radius);

    [LibraryImport(LibName)]
    public static partial float JPH_CapsuleShape_GetRadius(nint handle);

    [LibraryImport(LibName)]
    public static partial float JPH_CapsuleShape_GetHalfHeightOfCylinder(nint handle);
    #endregion

    #region CylinderShape/CylinderShapeSettings
    [LibraryImport(LibName)]
    public static partial nint JPH_CylinderShapeSettings_Create(float halfHeight, float radius, float convexRadius);

    [LibraryImport(LibName)]
    public static partial nint JPH_CylinderShapeSettings_CreateShape(nint settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_CylinderShape_Create(float halfHeight, float radius);

    [LibraryImport(LibName)]
    public static partial float JPH_CylinderShape_GetRadius(nint handle);

    [LibraryImport(LibName)]
    public static partial float JPH_CylinderShape_GetHalfHeight(nint handle);
    #endregion

    #region TaperedCylinderShape/TaperedCylinderShapeSettings
    [LibraryImport(LibName)]
    public static partial nint JPH_TaperedCylinderShapeSettings_Create(float halfHeightOfTaperedCylinder, float topRadius, float bottomRadius, float convexRadius, nint material);

    [LibraryImport(LibName)]
    public static partial nint JPH_TaperedCylinderShapeSettings_CreateShape(nint settings);

    [LibraryImport(LibName)]
    public static partial float JPH_TaperedCylinderShape_GetTopRadius(nint shape);

    [LibraryImport(LibName)]
    public static partial float JPH_TaperedCylinderShape_GetBottomRadius(nint shape);

    [LibraryImport(LibName)]
    public static partial float JPH_TaperedCylinderShape_GetConvexRadius(nint shape);

    [LibraryImport(LibName)]
    public static partial float JPH_TaperedCylinderShape_GetHalfHeight(nint shape);
    #endregion

    #region ConvexHullShape/ConvexHullShapeSettings
    [LibraryImport(LibName)]
    public static partial nint JPH_ConvexHullShapeSettings_Create(Vector3* points, int pointsCount, float maxConvexRadius);

    [LibraryImport(LibName)]
    public static partial nint JPH_ConvexHullShapeSettings_CreateShape(nint settings);

    [LibraryImport(LibName)]
    public static partial uint JPH_ConvexHullShape_GetNumPoints(nint shape);

    [LibraryImport(LibName)]
    public static partial void JPH_ConvexHullShape_GetPoint(nint shape, uint index, Vector3* result);

    [LibraryImport(LibName)]
    public static partial uint JPH_ConvexHullShape_GetNumFaces(nint shape);

    [LibraryImport(LibName)]
    public static partial uint JPH_ConvexHullShape_GetFaceVertices(nint shape, uint faceIndex, uint maxVertices, uint* vertices);

    [LibraryImport(LibName)]
    public static partial uint JPH_ConvexHullShape_GetNumVerticesInFace(nint shape, uint faceIndex);

    /* ConvexShape */
    [LibraryImport(LibName)]
    public static partial float JPH_ConvexShapeSettings_GetDensity(nint shape);

    [LibraryImport(LibName)]
    public static partial void JPH_ConvexShapeSettings_SetDensity(nint shape, float value);
    #endregion

    #region MeshShape/MeshShapeSettings
    [LibraryImport(LibName)]
    public static partial nint JPH_MeshShapeSettings_Create(Triangle* triangle, int triangleCount);

    [LibraryImport(LibName)]
    public static partial nint JPH_MeshShapeSettings_Create2(Vector3* vertices, int verticesCount, IndexedTriangle* triangles, int triangleCount);

    [LibraryImport(LibName)]
    public static partial uint JPH_MeshShapeSettings_GetMaxTrianglesPerLeaf(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_MeshShapeSettings_SetMaxTrianglesPerLeaf(nint settings, uint value);
    [LibraryImport(LibName)]
    public static partial float JPH_MeshShapeSettings_GetActiveEdgeCosThresholdAngle(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_MeshShapeSettings_SetActiveEdgeCosThresholdAngle(nint settings, float value);
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_MeshShapeSettings_GetPerTriangleUserData(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_MeshShapeSettings_SetPerTriangleUserData(nint settings, [MarshalAs(UnmanagedType.U1)] bool value);
    [LibraryImport(LibName)]
    public static partial MeshShapeBuildQuality JPH_MeshShapeSettings_GetBuildQuality(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_MeshShapeSettings_SetBuildQuality(nint settings, MeshShapeBuildQuality value);

    [LibraryImport(LibName)]
    public static partial void JPH_MeshShapeSettings_Sanitize(nint settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_MeshShapeSettings_CreateShape(nint settings);

    [LibraryImport(LibName)]
    public static partial uint JPH_MeshShape_GetTriangleUserData(nint shape, SubShapeID id);
    #endregion

    #region HeightFieldShape/HeightFieldShapeSettings
    [LibraryImport(LibName)]
    public static partial nint JPH_HeightFieldShapeSettings_Create(float* samples, in Vector3 offset, in Vector3 scale, int sampleCount);

    [LibraryImport(LibName)]
    public static partial nint JPH_HeightFieldShapeSettings_CreateShape(nint settings);

    [LibraryImport(LibName)]
    public static partial void JPH_HeightFieldShapeSettings_DetermineMinAndMaxSample(nint settings, out float outMinValue, out float outMaxValue, out float outQuantizationScale);

    [LibraryImport(LibName)]
    public static partial uint JPH_HeightFieldShapeSettings_CalculateBitsPerSampleForError(nint settings, float maxError);

    [LibraryImport(LibName)]
    public static partial uint JPH_HeightFieldShape_GetSampleCount(nint shape);
    [LibraryImport(LibName)]
    public static partial uint JPH_HeightFieldShape_GetBlockSize(nint shape);
    [LibraryImport(LibName)]
    public static partial nint JPH_HeightFieldShape_GetMaterial(nint shape, uint x, uint y);
    [LibraryImport(LibName)]
    public static partial void JPH_HeightFieldShape_GetPosition(nint shape, uint x, uint y, out Vector3 result);
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_HeightFieldShape_IsNoCollision(nint shape, uint x, uint y);
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_HeightFieldShape_ProjectOntoSurface(nint shape, in Vector3 localPosition, out Vector3 surfacePosition, out SubShapeID subShapeID);
    [LibraryImport(LibName)]
    public static partial float JPH_HeightFieldShape_GetMinHeightValue(nint shape);
    [LibraryImport(LibName)]
    public static partial float JPH_HeightFieldShape_GetMaxHeightValue(nint shape);
    #endregion

    #region TaperedCapsuleShape/TaperedCapsuleShapeSettings
    [LibraryImport(LibName)]
    public static partial nint JPH_TaperedCapsuleShapeSettings_Create(float halfHeightOfTaperedCylinder, float topRadius, float bottomRadius);

    [LibraryImport(LibName)]
    public static partial nint JPH_TaperedCapsuleShapeSettings_CreateShape(nint settings);

    [LibraryImport(LibName)]
    public static partial float JPH_TaperedCapsuleShape_GetTopRadius(nint shape);

    [LibraryImport(LibName)]
    public static partial float JPH_TaperedCapsuleShape_GetBottomRadius(nint shape);

    [LibraryImport(LibName)]
    public static partial float JPH_TaperedCapsuleShape_GetHalfHeight(nint shape);
    #endregion

    /* CompoundShape */
    [LibraryImport(LibName)]
    public static partial void JPH_CompoundShapeSettings_AddShape(nint handle, Vector3* position, Quaternion* rotation, nint shapeSettings, uint userData);

    [LibraryImport(LibName)]
    public static partial void JPH_CompoundShapeSettings_AddShape2(nint handle, Vector3* position, Quaternion* rotation, nint shape, uint userData);

    [LibraryImport(LibName)]
    public static partial uint JPH_CompoundShape_GetNumSubShapes(nint handle);

    [LibraryImport(LibName)]
    public static partial nint JPH_StaticCompoundShapeSettings_Create();

    [LibraryImport(LibName)]
    public static partial nint JPH_StaticCompoundShape_Create(nint settings);

    /* MutableCompoundShape */
    [LibraryImport(LibName)]
    public static partial nint JPH_MutableCompoundShapeSettings_Create();

    [LibraryImport(LibName)]
    public static partial nint JPH_MutableCompoundShape_Create(nint settings);

    [LibraryImport(LibName)]
    public static partial uint JPH_MutableCompoundShape_AddShape(nint shape, in Vector3 position, in Quaternion rotation, /*const JPH_Shape**/nint child, uint userData, uint index);
    [LibraryImport(LibName)]
    public static partial void JPH_MutableCompoundShape_RemoveShape(nint shape, uint index);
    [LibraryImport(LibName)]
    public static partial void JPH_MutableCompoundShape_ModifyShape(nint shape, uint index, in Vector3 position, in Quaternion rotation);
    [LibraryImport(LibName)]
    public static partial void JPH_MutableCompoundShape_ModifyShape2(nint shape, uint index, in Vector3 position, in Quaternion rotation, /*const JPH_Shape**/nint newShape);
    [LibraryImport(LibName)]
    public static partial void JPH_MutableCompoundShape_AdjustCenterOfMass(nint shape);

    [LibraryImport(LibName)]
    public static partial nint JPH_DecoratedShape_GetInnerShape(nint settings);

    /* RotatedTranslatedShape */
    [LibraryImport(LibName)]
    public static partial nint JPH_RotatedTranslatedShapeSettings_Create(in Vector3 position, in Quaternion rotation, nint shapeSettings);

    [LibraryImport(LibName)]
    public static partial nint JPH_RotatedTranslatedShapeSettings_Create2(in Vector3 position, in Quaternion rotation, nint shape);

    [LibraryImport(LibName)]
    public static partial nint JPH_RotatedTranslatedShapeSettings_CreateShape(nint settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_RotatedTranslatedShape_Create(in Vector3 position, in Quaternion rotation, nint shape);

    [LibraryImport(LibName)]
    public static partial nint JPH_RotatedTranslatedShape_GetPosition(nint shape, out Vector3 position);

    [LibraryImport(LibName)]
    public static partial nint JPH_RotatedTranslatedShape_GetRotation(nint shape, out Quaternion rotation);

    /* ScaledShape */
    [LibraryImport(LibName)]
    public static partial nint JPH_ScaledShapeSettings_Create(nint shapeSettings, in Vector3 scale);
    [LibraryImport(LibName)]
    public static partial nint JPH_ScaledShapeSettings_Create2(nint shape, in Vector3 scale);
    [LibraryImport(LibName)]
    public static partial nint JPH_ScaledShapeSettings_CreateShape(nint settings);
    [LibraryImport(LibName)]
    public static partial nint JPH_ScaledShape_Create(nint shape, in Vector3 scale);
    [LibraryImport(LibName)]
    public static partial void JPH_ScaledShape_GetScale(nint shape, out Vector3 result);

    /* JPH_OffsetCenterOfMassShape */
    [LibraryImport(LibName)]
    public static partial nint JPH_OffsetCenterOfMassShapeSettings_Create(in Vector3 offset, nint shapeSettings);

    [LibraryImport(LibName)]
    public static partial nint JPH_OffsetCenterOfMassShapeSettings_Create2(in Vector3 offset, nint shape);

    [LibraryImport(LibName)]
    public static partial nint JPH_OffsetCenterOfMassShapeSettings_CreateShape(nint settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_OffsetCenterOfMassShape_Create(in Vector3 offset, nint shape);

    [LibraryImport(LibName)]
    public static partial void JPH_OffsetCenterOfMassShape_GetOffset(nint handle, out Vector3 offset);

    #region EmptyShape/EmptyShapeSettings
    [LibraryImport(LibName)]
    public static partial nint JPH_EmptyShapeSettings_Create(in Vector3 centerOfMass);

    [LibraryImport(LibName)]
    public static partial nint JPH_EmptyShapeSettings_CreateShape(nint settings);
    #endregion

    /* BodyCreationSettings */
    [LibraryImport(LibName)]
    public static partial nint JPH_BodyCreationSettings_Create();

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyCreationSettings_Create2))]
    public static partial nint JPH_BodyCreationSettings_Create2(nint shapeSettings, Vector3* position, Quaternion* rotation, MotionType motionType, JPH_ObjectLayer objectLayer);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyCreationSettings_Create2))]
    public static partial nint JPH_BodyCreationSettings_Create2Double(nint shapeSettings, Double3* position, Quaternion* rotation, MotionType motionType, JPH_ObjectLayer objectLayer);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyCreationSettings_Create3))]
    public static partial nint JPH_BodyCreationSettings_Create3(nint shape, Vector3* position, Quaternion* rotation, MotionType motionType, JPH_ObjectLayer objectLayer);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyCreationSettings_Create3))]
    public static partial nint JPH_BodyCreationSettings_Create3Double(nint shape, Double3* position, Quaternion* rotation, MotionType motionType, JPH_ObjectLayer objectLayer);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_Destroy(nint settings);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_GetPosition(nint settings, Vector3* velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetPosition(nint settings, Vector3* velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_GetRotation(nint settings, Quaternion* velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetRotation(nint settings, Quaternion* velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_GetLinearVelocity(nint settings, Vector3* velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetLinearVelocity(nint settings, Vector3* velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_GetAngularVelocity(nint settings, Vector3* velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetAngularVelocity(nint settings, Vector3* velocity);

    [LibraryImport(LibName)]
    public static partial ulong JPH_BodyCreationSettings_GetUserData(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetUserData(nint settings, ulong value);

    [LibraryImport(LibName)]
    public static partial ObjectLayer JPH_BodyCreationSettings_GetObjectLayer(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetObjectLayer(nint settings, in ObjectLayer value);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_GetCollissionGroup(nint settings, out JPH_CollisionGroup result);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetCollissionGroup(nint settings, in JPH_CollisionGroup value);

    [LibraryImport(LibName)]
    public static partial MotionType JPH_BodyCreationSettings_GetMotionType(nint settings);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetMotionType(nint settings, MotionType value);

    [LibraryImport(LibName)]
    public static partial AllowedDOFs JPH_BodyCreationSettings_GetAllowedDOFs(nint settings);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetAllowedDOFs(nint settings, AllowedDOFs value);

    [LibraryImport(LibName)]

    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BodyCreationSettings_GetAllowDynamicOrKinematic(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetAllowDynamicOrKinematic(nint settings, [MarshalAs(UnmanagedType.U1)] bool value);

    [LibraryImport(LibName)]

    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BodyCreationSettings_GetIsSensor(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetIsSensor(nint settings, [MarshalAs(UnmanagedType.U1)] bool value);

    [LibraryImport(LibName)]

    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BodyCreationSettings_GetCollideKinematicVsNonDynamic(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetCollideKinematicVsNonDynamic(nint settings, [MarshalAs(UnmanagedType.U1)] bool value);

    [LibraryImport(LibName)]

    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BodyCreationSettings_GetUseManifoldReduction(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetUseManifoldReduction(nint settings, [MarshalAs(UnmanagedType.U1)] bool value);

    [LibraryImport(LibName)]

    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BodyCreationSettings_GetApplyGyroscopicForce(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetApplyGyroscopicForce(nint settings, [MarshalAs(UnmanagedType.U1)] bool value);

    [LibraryImport(LibName)]
    public static partial MotionQuality JPH_BodyCreationSettings_GetMotionQuality(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetMotionQuality(nint settings, in MotionQuality value);

    [LibraryImport(LibName)]

    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BodyCreationSettings_GetEnhancedInternalEdgeRemoval(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetEnhancedInternalEdgeRemoval(nint settings, [MarshalAs(UnmanagedType.U1)] bool value);

    [LibraryImport(LibName)]

    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BodyCreationSettings_GetAllowSleeping(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetAllowSleeping(nint settings, [MarshalAs(UnmanagedType.U1)] bool value);

    [LibraryImport(LibName)]
    public static partial float JPH_BodyCreationSettings_GetFriction(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetFriction(nint settings, float value);

    [LibraryImport(LibName)]
    public static partial float JPH_BodyCreationSettings_GetRestitution(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetRestitution(nint settings, float value);

    [LibraryImport(LibName)]
    public static partial float JPH_BodyCreationSettings_GetLinearDamping(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetLinearDamping(nint settings, float value);

    [LibraryImport(LibName)]
    public static partial float JPH_BodyCreationSettings_GetAngularDamping(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetAngularDamping(nint settings, float value);

    [LibraryImport(LibName)]
    public static partial float JPH_BodyCreationSettings_GetMaxLinearVelocity(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetMaxLinearVelocity(nint settings, float value);

    [LibraryImport(LibName)]
    public static partial float JPH_BodyCreationSettings_GetMaxAngularVelocity(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetMaxAngularVelocity(nint settings, float value);

    [LibraryImport(LibName)]
    public static partial float JPH_BodyCreationSettings_GetGravityFactor(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetGravityFactor(nint settings, float value);

    [LibraryImport(LibName)]
    public static partial uint JPH_BodyCreationSettings_GetNumVelocityStepsOverride(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetNumVelocityStepsOverride(nint settings, uint value);

    [LibraryImport(LibName)]
    public static partial uint JPH_BodyCreationSettings_GetNumPositionStepsOverride(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetNumPositionStepsOverride(nint settings, uint value);

    [LibraryImport(LibName)]
    public static partial OverrideMassProperties JPH_BodyCreationSettings_GetOverrideMassProperties(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetOverrideMassProperties(nint settings, OverrideMassProperties value);

    [LibraryImport(LibName)]
    public static partial float JPH_BodyCreationSettings_GetInertiaMultiplier(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetInertiaMultiplier(nint settings, float value);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_GetMassPropertiesOverride(nint settings, out MassProperties result);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyCreationSettings_SetMassPropertiesOverride(nint settings, MassProperties* massProperties);

    /* SoftBodyCreationSettings */
    [LibraryImport(LibName)]
    public static partial nint JPH_SoftBodyCreationSettings_Create();

    [LibraryImport(LibName)]
    public static partial void JPH_SoftBodyCreationSettings_Destroy(nint settings);

    #region JPH_Constraint
    public struct JPH_ConstraintSettings
    {
        public Bool8 enabled;
        public uint constraintPriority;
        public uint numVelocityStepsOverride;
        public uint numPositionStepsOverride;
        public float drawConstraintSize;
        public ulong userData;
    }

    public struct JPH_FixedConstraintSettings
    {
        public JPH_ConstraintSettings baseSettings;    /* Inherics JPH_ConstraintSettings */

        public ConstraintSpace space;
        public Bool8 autoDetectPoint;
        public Vector3 point1; /* JPH_RVec3 */
        public Vector3 axisX1;
        public Vector3 axisY1;
        public Vector3 point2; /* JPH_RVec3 */
        public Vector3 axisX2;
        public Vector3 axisY2;
    }

    public struct JPH_DistanceConstraintSettings
    {
        public JPH_ConstraintSettings baseSettings;    /* Inherics JPH_ConstraintSettings */

        public ConstraintSpace space;
        public Vector3 point1;                /* JPH_RVec3 */
        public Vector3 point2;                /* JPH_RVec3 */
        public float minDistance;
        public float maxDistance;
        public SpringSettings limitsSpringSettings;
    }

    public struct JPH_PointConstraintSettings
    {
        public JPH_ConstraintSettings baseSettings;    /* Inherics JPH_ConstraintSettings */

        public ConstraintSpace space;
        public Vector3 point1;                /* JPH_RVec3 */
        public Vector3 point2;                /* JPH_RVec3 */
    }

    public struct JPH_HingeConstraintSettings
    {
        public JPH_ConstraintSettings baseSettings;    /* Inherics JPH_ConstraintSettings */

        public ConstraintSpace space;
        public Vector3 point1;                /* JPH_RVec3 */
        public Vector3 hingeAxis1;
        public Vector3 normalAxis1;
        public Vector3 point2;                /* JPH_RVec3 */
        public Vector3 hingeAxis2;
        public Vector3 normalAxis2;
        public float limitsMin;
        public float limitsMax;
        public SpringSettings limitsSpringSettings;
        public float maxFrictionTorque;
        public MotorSettings motorSettings;
    }

    public struct JPH_SliderConstraintSettings
    {
        public JPH_ConstraintSettings baseSettings;    /* Inherics JPH_ConstraintSettings */

        public ConstraintSpace space;
        public Bool8 autoDetectPoint;
        public Vector3 point1;                /* JPH_RVec3 */
        public Vector3 sliderAxis1;
        public Vector3 normalAxis1;
        public Vector3 point2;                /* JPH_RVec3 */
        public Vector3 sliderAxis2;
        public Vector3 normalAxis2;
        public float limitsMin;
        public float limitsMax;
        public SpringSettings limitsSpringSettings;
        public float maxFrictionForce;
        public MotorSettings motorSettings;
    }

    public struct JPH_ConeConstraintSettings
    {
        public JPH_ConstraintSettings baseSettings;    /* Inherics JPH_ConstraintSettings */

        public ConstraintSpace space;
        public Vector3 point1;                /* JPH_RVec3 */
        public Vector3 twistAxis1;
        public Vector3 point2;                /* JPH_RVec3 */
        public Vector3 twistAxis2;
        public float halfConeAngle;
    }

    public struct JPH_SwingTwistConstraintSettings
    {
        public JPH_ConstraintSettings baseSettings;    /* Inherics JPH_ConstraintSettings */

        public ConstraintSpace space;
        public Vector3 position1;             /* JPH_RVec3 */
        public Vector3 twistAxis1;
        public Vector3 planeAxis1;
        public Vector3 position2;             /* JPH_RVec3 */
        public Vector3 twistAxis2;
        public Vector3 planeAxis2;
        public SwingType swingType;
        public float normalHalfConeAngle;
        public float planeHalfConeAngle;
        public float twistMinAngle;
        public float twistMaxAngle;
        public float maxFrictionTorque;
        public MotorSettings swingMotorSettings;
        public MotorSettings twistMotorSettings;
    }

    public struct JPH_SixDOFConstraintSettings
    {
        public JPH_ConstraintSettings baseSettings;    /* Inherics JPH_ConstraintSettings */

        public ConstraintSpace space;
        public Vector3 position1;             /* JPH_RVec3 */
        public Vector3 axisX1;
        public Vector3 axisY1;
        public Vector3 position2;             /* JPH_RVec3 */
        public Vector3 axisX2;
        public Vector3 axisY2;
        public fixed float maxFriction[(int)SixDOFConstraintAxis.Count];
        public SwingType swingType;
        public fixed float limitMin[(int)SixDOFConstraintAxis.Count];
        public fixed float limitMax[(int)SixDOFConstraintAxis.Count];

        public limitsSpringSettings__FixedBuffer limitsSpringSettings;
        public motorSettings__FixedBuffer motorSettings;

        [InlineArray((int)SixDOFConstraintAxis.NumTranslation)]
        public partial struct limitsSpringSettings__FixedBuffer
        {
            public SpringSettings e0;
        }

        [InlineArray((int)SixDOFConstraintAxis.Count)]
        public partial struct motorSettings__FixedBuffer
        {
            public MotorSettings e0;
        }
    }

    public struct JPH_GearConstraintSettings
    {
        public JPH_ConstraintSettings baseSettings;    /* Inherics JPH_ConstraintSettings */

        public ConstraintSpace space;
        public Vector3 hingeAxis1;
        public Vector3 hingeAxis2;
        public float ratio;
    }

    /* JPH_Constraint */
    [LibraryImport(LibName)]
    public static partial void JPH_Constraint_Destroy(nint constraint);

    [LibraryImport(LibName)]
    public static partial ConstraintType JPH_Constraint_GetType(nint constraint);

    [LibraryImport(LibName)]
    public static partial ConstraintSubType JPH_Constraint_GetSubType(nint constraint);

    [LibraryImport(LibName)]
    public static partial uint JPH_Constraint_GetConstraintPriority(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_Constraint_SetConstraintPriority(nint handle, uint value);

    [LibraryImport(LibName)]
    public static partial uint JPH_Constraint_GetNumVelocityStepsOverride(nint constraint);
    [LibraryImport(LibName)]
    public static partial void JPH_Constraint_SetNumVelocityStepsOverride(nint constraint, uint value);
    [LibraryImport(LibName)]
    public static partial uint JPH_Constraint_GetNumPositionStepsOverride(nint constraint);
    [LibraryImport(LibName)]
    public static partial void JPH_Constraint_SetNumPositionStepsOverride(nint constraint, uint value);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Constraint_GetEnabled(nint constraint);

    [LibraryImport(LibName)]
    public static partial void JPH_Constraint_SetEnabled(nint constraint, [MarshalAs(UnmanagedType.U1)] bool value);

    [LibraryImport(LibName)]
    public static partial ulong JPH_Constraint_GetUserData(nint constraint);

    [LibraryImport(LibName)]
    public static partial void JPH_Constraint_SetUserData(nint constraint, ulong value);

    [LibraryImport(LibName)]
    public static partial void JPH_Constraint_NotifyShapeChanged(nint constraint, uint bodyID, in Vector3 deltaCOM);

    [LibraryImport(LibName)]
    public static partial void JPH_Constraint_ResetWarmStart(nint constraint);
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Constraint_IsActive(nint constraint);
    [LibraryImport(LibName)]
    public static partial void JPH_Constraint_SetupVelocityConstraint(nint constraint, float deltaTime);
    [LibraryImport(LibName)]
    public static partial void JPH_Constraint_WarmStartVelocityConstraint(nint constraint, float warmStartImpulseRatio);
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Constraint_SolveVelocityConstraint(nint constraint, float deltaTime);
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Constraint_SolvePositionConstraint(nint constraint, float deltaTime, float baumgarte);
    #endregion

    #region JPH_TwoBodyConstraint
    [LibraryImport(LibName)]
    public static partial nint JPH_TwoBodyConstraint_GetBody1(nint constraint);

    [LibraryImport(LibName)]
    public static partial nint JPH_TwoBodyConstraint_GetBody2(nint constraint);

    [LibraryImport(LibName)]
    public static partial void JPH_TwoBodyConstraint_GetConstraintToBody1Matrix(nint constraint, out Matrix4x4 result);

    [LibraryImport(LibName)]
    public static partial void JPH_TwoBodyConstraint_GetConstraintToBody2Matrix(nint constraint, out Matrix4x4 result);
    #endregion

    #region JPH_FixedConstraint
    [LibraryImport(LibName)]
    public static partial void JPH_FixedConstraintSettings_Init(JPH_FixedConstraintSettings* settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_FixedConstraint_Create(JPH_FixedConstraintSettings* settings, nint body1, nint body2);

    [LibraryImport(LibName)]
    public static partial void JPH_FixedConstraint_GetSettings(nint constraint, out JPH_FixedConstraintSettings settings);

    [LibraryImport(LibName)]
    public static partial void JPH_FixedConstraint_GetTotalLambdaPosition(nint handle, Vector3* result);

    [LibraryImport(LibName)]
    public static partial void JPH_FixedConstraint_GetTotalLambdaRotation(nint handle, Vector3* result);
    #endregion

    #region JPH_DistanceConstraint
    /* JPH_DistanceConstraint */
    [LibraryImport(LibName)]
    public static partial void JPH_DistanceConstraintSettings_Init(JPH_DistanceConstraintSettings* settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_DistanceConstraint_Create(JPH_DistanceConstraintSettings* settings, nint body1, nint body2);

    [LibraryImport(LibName)]
    public static partial void JPH_DistanceConstraint_GetSettings(nint constraint, out JPH_DistanceConstraintSettings settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_DistanceConstraint_SetDistance(nint constraint, float minDistance, float maxDistance);

    [LibraryImport(LibName)]
    public static partial float JPH_DistanceConstraint_GetMinDistance(nint constraint);

    [LibraryImport(LibName)]
    public static partial float JPH_DistanceConstraint_GetMaxDistance(nint constraint);
    [LibraryImport(LibName)]
    public static partial void JPH_DistanceConstraint_GetLimitsSpringSettings(nint constraint, SpringSettings* result);
    [LibraryImport(LibName)]
    public static partial void JPH_DistanceConstraint_SetLimitsSpringSettings(nint constraint, SpringSettings* settings);
    [LibraryImport(LibName)]
    public static partial float JPH_DistanceConstraint_GetTotalLambdaPosition(nint constraint);
    #endregion

    #region JPH_PointConstraint
    [LibraryImport(LibName)]
    public static partial void JPH_PointConstraintSettings_Init(JPH_PointConstraintSettings* settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_PointConstraint_Create(JPH_PointConstraintSettings* settings, nint body1, nint body2);

    [LibraryImport(LibName)]
    public static partial void JPH_PointConstraint_GetSettings(nint constraint, out JPH_PointConstraintSettings settings);

    [LibraryImport(LibName)]
    public static partial void JPH_PointConstraint_SetPoint1(nint handle, ConstraintSpace space, Vector3* value); // RVec3

    [LibraryImport(LibName)]
    public static partial void JPH_PointConstraint_SetPoint2(nint handle, ConstraintSpace space, Vector3* value); // RVec3

    [LibraryImport(LibName)]
    public static partial void JPH_PointConstraint_GetTotalLambdaPosition(nint handle, out Vector3 result);
    #endregion

    #region JPH_HingeConstraint
    [LibraryImport(LibName)]
    public static partial void JPH_HingeConstraintSettings_Init(JPH_HingeConstraintSettings* settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_HingeConstraint_Create(JPH_HingeConstraintSettings* settings, nint body1, nint body2);

    [LibraryImport(LibName)]
    public static partial void JPH_HingeConstraint_GetSettings(nint constraint, out JPH_HingeConstraintSettings settings);

    [LibraryImport(LibName)]
    public static partial void JPH_HingeConstraint_GetLocalSpacePoint1(nint constraint, out Vector3 result);
    [LibraryImport(LibName)]
    public static partial void JPH_HingeConstraint_GetLocalSpacePoint2(nint constraint, out Vector3 result);
    [LibraryImport(LibName)]
    public static partial void JPH_HingeConstraint_GetLocalSpaceHingeAxis1(nint constraint, out Vector3 result);
    [LibraryImport(LibName)]
    public static partial void JPH_HingeConstraint_GetLocalSpaceHingeAxis2(nint constraint, out Vector3 result);
    [LibraryImport(LibName)]
    public static partial void JPH_HingeConstraint_GetLocalSpaceNormalAxis1(nint constraint, out Vector3 result);
    [LibraryImport(LibName)]
    public static partial void JPH_HingeConstraint_GetLocalSpaceNormalAxis2(nint constraint, out Vector3 result);

    [LibraryImport(LibName)]
    public static partial float JPH_HingeConstraint_GetCurrentAngle(nint constraint);
    [LibraryImport(LibName)]
    public static partial void JPH_HingeConstraint_SetMaxFrictionTorque(nint constraint, float frictionTorque);
    [LibraryImport(LibName)]
    public static partial float JPH_HingeConstraint_GetMaxFrictionTorque(nint constraint);
    [LibraryImport(LibName)]
    public static partial void JPH_HingeConstraint_SetMotorSettings(nint constraint, MotorSettings* settings);
    [LibraryImport(LibName)]
    public static partial void JPH_HingeConstraint_GetMotorSettings(nint constraint, out MotorSettings result);
    [LibraryImport(LibName)]
    public static partial void JPH_HingeConstraint_SetMotorState(nint constraint, MotorState state);
    [LibraryImport(LibName)]
    public static partial MotorState JPH_HingeConstraint_GetMotorState(nint constraint);
    [LibraryImport(LibName)]
    public static partial void JPH_HingeConstraint_SetTargetAngularVelocity(nint constraint, float angularVelocity);
    [LibraryImport(LibName)]
    public static partial float JPH_HingeConstraint_GetTargetAngularVelocity(nint constraint);
    [LibraryImport(LibName)]
    public static partial void JPH_HingeConstraint_SetTargetAngle(nint constraint, float angle);
    [LibraryImport(LibName)]
    public static partial float JPH_HingeConstraint_GetTargetAngle(nint constraint);
    [LibraryImport(LibName)]
    public static partial void JPH_HingeConstraint_SetLimits(nint constraint, float inLimitsMin, float inLimitsMax);
    [LibraryImport(LibName)]
    public static partial float JPH_HingeConstraint_GetLimitsMin(nint constraint);
    [LibraryImport(LibName)]
    public static partial float JPH_HingeConstraint_GetLimitsMax(nint constraint);
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_HingeConstraint_HasLimits(nint constraint);
    [LibraryImport(LibName)]
    public static partial void JPH_HingeConstraint_GetLimitsSpringSettings(nint constraint, out SpringSettings result);
    [LibraryImport(LibName)]
    public static partial void JPH_HingeConstraint_SetLimitsSpringSettings(nint constraint, SpringSettings* settings);
    [LibraryImport(LibName)]
    public static partial void JPH_HingeConstraint_GetTotalLambdaPosition(nint constraint, out Vector3 result);
    [LibraryImport(LibName)]
    public static partial void JPH_HingeConstraint_GetTotalLambdaRotation(nint constraint, out Vector2 rotation);
    [LibraryImport(LibName)]
    public static partial float JPH_HingeConstraint_GetTotalLambdaRotationLimits(nint constraint);
    [LibraryImport(LibName)]
    public static partial float JPH_HingeConstraint_GetTotalLambdaMotor(nint constraint);
    #endregion

    #region JPH_SliderConstraint
    [LibraryImport(LibName)]
    public static partial void JPH_SliderConstraintSettings_Init(JPH_SliderConstraintSettings* settings);

    [LibraryImport(LibName)]
    public static partial void JPH_SliderConstraintSettings_SetSliderAxis(JPH_SliderConstraintSettings* settings, in Vector3 value);

    [LibraryImport(LibName)]
    public static partial nint JPH_SliderConstraint_Create(JPH_SliderConstraintSettings* settings, nint body1, nint body2);

    [LibraryImport(LibName)]
    public static partial void JPH_SliderConstraint_GetSettings(nint constraint, out JPH_SliderConstraintSettings settings);
    [LibraryImport(LibName)]
    public static partial float JPH_SliderConstraint_GetCurrentPosition(nint constraint);
    [LibraryImport(LibName)]
    public static partial void JPH_SliderConstraint_SetMaxFrictionForce(nint constraint, float frictionForce);
    [LibraryImport(LibName)]
    public static partial float JPH_SliderConstraint_GetMaxFrictionForce(nint constraint);
    [LibraryImport(LibName)]
    public static partial void JPH_SliderConstraint_SetMotorSettings(nint constraint, MotorSettings* settings);
    [LibraryImport(LibName)]
    public static partial void JPH_SliderConstraint_GetMotorSettings(nint constraint, out MotorSettings result);
    [LibraryImport(LibName)]
    public static partial void JPH_SliderConstraint_SetMotorState(nint constraint, MotorState state);
    [LibraryImport(LibName)]
    public static partial MotorState JPH_SliderConstraint_GetMotorState(nint constraint);
    [LibraryImport(LibName)]
    public static partial void JPH_SliderConstraint_SetTargetVelocity(nint constraint, float velocity);
    [LibraryImport(LibName)]
    public static partial float JPH_SliderConstraint_GetTargetVelocity(nint constraint);
    [LibraryImport(LibName)]
    public static partial void JPH_SliderConstraint_SetTargetPosition(nint constraint, float position);
    [LibraryImport(LibName)]
    public static partial float JPH_SliderConstraint_GetTargetPosition(nint constraint);
    [LibraryImport(LibName)]
    public static partial void JPH_SliderConstraint_SetLimits(nint constraint, float inLimitsMin, float inLimitsMax);
    [LibraryImport(LibName)]
    public static partial float JPH_SliderConstraint_GetLimitsMin(nint constraint);
    [LibraryImport(LibName)]
    public static partial float JPH_SliderConstraint_GetLimitsMax(nint constraint);
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_SliderConstraint_HasLimits(nint constraint);
    [LibraryImport(LibName)]
    public static partial void JPH_SliderConstraint_GetLimitsSpringSettings(nint constraint, out SpringSettings result);
    [LibraryImport(LibName)]
    public static partial void JPH_SliderConstraint_SetLimitsSpringSettings(nint constraint, SpringSettings* settings);
    [LibraryImport(LibName)]
    public static partial void JPH_SliderConstraint_GetTotalLambdaPosition(nint constraint, out Vector2 position);
    [LibraryImport(LibName)]
    public static partial float JPH_SliderConstraint_GetTotalLambdaPositionLimits(nint constraint);
    [LibraryImport(LibName)]
    public static partial void JPH_SliderConstraint_GetTotalLambdaRotation(nint constraint, out Vector3 result);
    [LibraryImport(LibName)]
    public static partial float JPH_SliderConstraint_GetTotalLambdaMotor(nint constraint);
    #endregion

    #region JPH_ConeConstraint
    [LibraryImport(LibName)]
    public static partial void JPH_ConeConstraintSettings_Init(JPH_ConeConstraintSettings* settings);
    [LibraryImport(LibName)]
    public static partial nint JPH_ConeConstraint_Create(JPH_ConeConstraintSettings* settings, nint body1, nint body2);

    [LibraryImport(LibName)]
    public static partial void JPH_ConeConstraint_GetSettings(nint constraint, out JPH_ConeConstraintSettings settings);

    [LibraryImport(LibName)]
    public static partial void JPH_ConeConstraint_SetHalfConeAngle(nint constraint, float halfConeAngle);
    [LibraryImport(LibName)]
    public static partial float JPH_ConeConstraint_GetCosHalfConeAngle(nint constraint);
    [LibraryImport(LibName)]
    public static partial void JPH_ConeConstraint_GetTotalLambdaPosition(nint constraint, out Vector3 result);
    [LibraryImport(LibName)]
    public static partial float JPH_ConeConstraint_GetTotalLambdaRotation(nint constraint);
    #endregion

    #region JPH_SwingTwistConstraint
    [LibraryImport(LibName)]
    public static partial void JPH_SwingTwistConstraintSettings_Init(JPH_SwingTwistConstraintSettings* settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_SwingTwistConstraint_Create(JPH_SwingTwistConstraintSettings* settings, nint body1, nint body2);

    [LibraryImport(LibName)]
    public static partial void JPH_SwingTwistConstraint_GetSettings(nint constraint, out JPH_SwingTwistConstraintSettings settings);

    [LibraryImport(LibName)]
    public static partial float JPH_SwingTwistConstraint_GetNormalHalfConeAngle(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_SwingTwistConstraint_GetTotalLambdaPosition(nint constraint, out Vector3 result);
    [LibraryImport(LibName)]
    public static partial float JPH_SwingTwistConstraint_GetTotalLambdaTwist(nint constraint);
    [LibraryImport(LibName)]
    public static partial float JPH_SwingTwistConstraint_GetTotalLambdaSwingY(nint constraint);
    [LibraryImport(LibName)]
    public static partial float JPH_SwingTwistConstraint_GetTotalLambdaSwingZ(nint constraint);
    [LibraryImport(LibName)]
    public static partial void JPH_SwingTwistConstraint_GetTotalLambdaMotor(nint constraint, out Vector3 result);

    #endregion

    #region JPH_SixDOFConstraint
    [LibraryImport(LibName)]
    public static partial void JPH_SixDOFConstraintSettings_Init(JPH_SixDOFConstraintSettings* settings);

    [LibraryImport(LibName)]
    public static partial void JPH_SixDOFConstraintSettings_MakeFreeAxis(JPH_SixDOFConstraintSettings* settings, SixDOFConstraintAxis axis);
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_SixDOFConstraintSettings_IsFreeAxis(JPH_SixDOFConstraintSettings* settings, SixDOFConstraintAxis axis);
    [LibraryImport(LibName)]
    public static partial void JPH_SixDOFConstraintSettings_MakeFixedAxis(JPH_SixDOFConstraintSettings* settings, SixDOFConstraintAxis axis);
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_SixDOFConstraintSettings_IsFixedAxis(JPH_SixDOFConstraintSettings* settings, SixDOFConstraintAxis axis);
    [LibraryImport(LibName)]
    public static partial void JPH_SixDOFConstraintSettings_SetLimitedAxis(JPH_SixDOFConstraintSettings* settings, SixDOFConstraintAxis axis, float min, float max);

    [LibraryImport(LibName)]
    public static partial nint JPH_SixDOFConstraint_Create(JPH_SixDOFConstraintSettings* settings, nint body1, nint body2);

    [LibraryImport(LibName)]
    public static partial void JPH_SixDOFConstraint_GetSettings(nint constraint, out JPH_SixDOFConstraintSettings settings);

    [LibraryImport(LibName)]
    public static partial float JPH_SixDOFConstraint_GetLimitsMin(nint handle, uint axis);

    [LibraryImport(LibName)]
    public static partial float JPH_SixDOFConstraint_GetLimitsMax(nint handle, uint axis);

    [LibraryImport(LibName)]
    public static partial void JPH_SixDOFConstraint_GetTotalLambdaPosition(nint constraint, out Vector3 result);
    [LibraryImport(LibName)]
    public static partial void JPH_SixDOFConstraint_GetTotalLambdaRotation(nint constraint, out Vector3 result);
    [LibraryImport(LibName)]
    public static partial void JPH_SixDOFConstraint_GetTotalLambdaMotorTranslation(nint constraint, out Vector3 result);
    [LibraryImport(LibName)]
    public static partial void JPH_SixDOFConstraint_GetTotalLambdaMotorRotation(nint constraint, out Vector3 result);
    #endregion

    #region JPH_GearConstraint
    [LibraryImport(LibName)]
    public static partial void JPH_GearConstraintSettings_Init(JPH_GearConstraintSettings* settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_GearConstraint_Create(JPH_GearConstraintSettings* settings, nint body1, nint body2);

    [LibraryImport(LibName)]
    public static partial void JPH_GearConstraint_GetSettings(nint constraint, out JPH_GearConstraintSettings settings);

    [LibraryImport(LibName)]
    public static partial void JPH_GearConstraint_SetConstraints(nint handle, nint gear1, nint gear2);

    [LibraryImport(LibName)]
    public static partial float JPH_GearConstraint_GetTotalLambda(nint handle);
    #endregion

    /* PhysicsSystem */
    [StructLayout(LayoutKind.Sequential)]
    public struct NativePhysicsSystemSettings
    {
        public int maxBodies; /* 10240 */
        public int numBodyMutexes; /* 0 */
        public int maxBodyPairs; /* 65536 */
        public int maxContactConstraints; /* 10240 */
        private int _padding;
        public /*BroadPhaseLayerInterfaceTable*/ nint broadPhaseLayerInterface;
        public /*ObjectLayerPairFilterTable**/ nint objectLayerPairFilter;
        public /*ObjectVsBroadPhaseLayerFilterTable* */nint objectVsBroadPhaseLayerFilter;
    }

    [LibraryImport(LibName)]
    public static partial nint JPH_PhysicsSystem_Create(NativePhysicsSystemSettings* settings);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_Destroy(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_GetPhysicsSettings(nint handle, PhysicsSettings* result);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_SetPhysicsSettings(nint handle, PhysicsSettings* result);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_OptimizeBroadPhase(nint handle);

    [LibraryImport(LibName)]
    public static partial PhysicsUpdateError JPH_PhysicsSystem_Update(nint handle, float deltaTime, int collisionSteps, nint jobSystem);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_SetContactListener(nint system, nint listener);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_SetBodyActivationListener(nint system, nint listener);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_SetSimShapeFilter(nint system, nint shapeFilter);

    [LibraryImport(LibName)]
    public static partial uint JPH_PhysicsSystem_GetNumBodies(nint system);

    [LibraryImport(LibName)]
    public static partial uint JPH_PhysicsSystem_GetNumActiveBodies(nint system, BodyType type);

    [LibraryImport(LibName)]
    public static partial uint JPH_PhysicsSystem_GetMaxBodies(nint system);

    [LibraryImport(LibName)]
    public static partial uint JPH_PhysicsSystem_GetNumConstraints(nint system);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_GetGravity(nint handle, out Vector3 velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_SetGravity(nint handle, in Vector3 velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_AddConstraint(nint handle, nint constraint);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_RemoveConstraint(nint handle, nint constraint);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_AddConstraints(nint handle, nint* constraints, uint count);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_RemoveConstraints(nint handle, nint* constraints, uint count);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_AddStepListener(nint handle, nint listener);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_RemoveStepListener(nint handle, nint listener);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_GetBodies(nint handle, BodyID* ids, uint count);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_GetConstraints(nint handle, nint* constraints, uint count);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_DrawBodies(nint system, DrawSettings* settings, nint renderer, nint bodyFilter);
    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_DrawConstraints(nint system, nint renderer);
    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_DrawConstraintLimits(nint system, nint renderer);
    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsSystem_DrawConstraintReferenceFrame(nint system, nint renderer);

    /* Material */
    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial nint JPH_PhysicsMaterial_Create(string name, uint color);

    [LibraryImport(LibName)]
    public static partial void JPH_PhysicsMaterial_Destroy(nint handle);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial string? JPH_PhysicsMaterial_GetDebugName(nint handle);

    [LibraryImport(LibName)]
    public static partial uint JPH_PhysicsMaterial_GetDebugColor(nint handle);

    /* BodyInterface */
    [LibraryImport(LibName)]
    public static partial nint JPH_PhysicsSystem_GetBodyInterface(nint system);

    [LibraryImport(LibName)]
    public static partial nint JPH_PhysicsSystem_GetBodyInterfaceNoLock(nint system);

    [LibraryImport(LibName)]
    public static partial nint JPH_BodyInterface_CreateBody(nint handle, nint settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_BodyInterface_CreateSoftBody(nint handle, nint settings);

    [LibraryImport(LibName)]
    public static partial uint JPH_BodyInterface_CreateAndAddBody(nint handle, nint bodyID, Activation activation);

    [LibraryImport(LibName)]
    public static partial nint JPH_BodyInterface_CreateBodyWithID(nint handle, uint bodyID, nint settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_BodyInterface_CreateBodyWithoutID(nint handle, nint settings);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_DestroyBody(nint handle, uint bodyID);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_DestroyBodyWithoutID(nint handle, nint body);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BodyInterface_AssignBodyID(nint handle, nint body);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BodyInterface_AssignBodyID2(nint handle, nint body, uint bodyID);

    [LibraryImport(LibName)]
    public static partial nint JPH_BodyInterface_UnassignBodyID(nint handle, uint bodyID);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_AddBody(nint handle, uint bodyID, Activation activationMode);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_RemoveBody(nint handle, uint bodyID);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_RemoveAndDestroyBody(nint handle, uint bodyID);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_GetLinearVelocity(nint handle, uint bodyID, Vector3* velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetLinearVelocity(nint handle, uint bodyID, Vector3* velocity);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyInterface_GetCenterOfMassPosition))]
    public static partial void JPH_BodyInterface_GetCenterOfMassPosition(nint handle, uint bodyID, Vector3* position);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyInterface_GetCenterOfMassPosition))]
    public static partial void JPH_BodyInterface_GetCenterOfMassPositionDouble(nint handle, uint bodyID, Double3* position);

    [LibraryImport(LibName)]

    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BodyInterface_IsActive(nint handle, uint bodyID);

    [LibraryImport(LibName)]

    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BodyInterface_IsAdded(nint handle, uint bodyID);

    [LibraryImport(LibName)]
    public static partial BodyType JPH_BodyInterface_GetBodyType(nint handle, uint bodyID);

    [LibraryImport(LibName)]
    public static partial MotionType JPH_BodyInterface_GetMotionType(nint handle, uint bodyID);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetMotionType(nint handle, uint bodyID, MotionType motionType, Activation activationMode);

    [LibraryImport(LibName)]
    public static partial MotionQuality JPH_BodyInterface_GetMotionQuality(nint handle, uint bodyID);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetMotionQuality(nint handle, uint bodyID, MotionQuality quality);

    [LibraryImport(LibName)]
    public static partial float JPH_BodyInterface_GetRestitution(nint handle, uint bodyID);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetRestitution(nint handle, uint bodyID, float value);

    [LibraryImport(LibName)]
    public static partial float JPH_BodyInterface_GetFriction(nint handle, uint bodyID);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetFriction(nint handle, uint bodyID, float value);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyInterface_SetPosition))]
    public static partial void JPH_BodyInterface_SetPosition(nint handle, uint bodyId, in Vector3 position, Activation activationMode);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyInterface_SetPosition))]
    public static partial void JPH_BodyInterface_SetPositionDouble(nint handle, uint bodyId, in Double3 position, Activation activationMode);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyInterface_GetPosition))]
    public static partial void JPH_BodyInterface_GetPosition(nint handle, uint bodyId, out Vector3 position);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyInterface_GetPosition))]
    public static partial void JPH_BodyInterface_GetPositionDouble(nint handle, uint bodyId, out Double3 position);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetRotation(nint handle, uint bodyId, in Quaternion rotation, Activation activationMode);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_GetRotation(nint handle, uint bodyId, out Quaternion rotation);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetPositionAndRotation(nint handle, uint bodyID, in Vector3 position, in Quaternion rotation, Activation activationMode);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetPositionAndRotation(nint handle, uint bodyID, in Double3 position, in Quaternion rotation, Activation activationMode);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetPositionAndRotationWhenChanged(nint handle, uint bodyID, in Vector3 position, in Quaternion rotation, Activation activationMode);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetPositionAndRotationWhenChanged(nint handle, uint bodyID, in Double3 position, in Quaternion rotation, Activation activationMode);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetPositionRotationAndVelocity(nint handle, uint bodyID, in Vector3 position, in Quaternion rotation, in Vector3 linearVelocity, in Vector3 angularVelocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetPositionRotationAndVelocity(nint handle, uint bodyID, in Double3 position, in Quaternion rotation, in Vector3 linearVelocity, in Vector3 angularVelocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_GetCollissionGroup(nint handle, uint bodyId, out JPH_CollisionGroup result);
    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetCollissionGroup(nint handle, uint bodyId, in JPH_CollisionGroup* group);

    [LibraryImport(LibName)]
    public static partial nint JPH_BodyInterface_GetShape(nint handle, uint bodyId);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetShape(nint handle, uint bodyId, nint shape, [MarshalAs(UnmanagedType.U1)] bool updateMassProperties, Activation activationMode);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_NotifyShapeChanged(nint handle, uint bodyId, in Vector3 previousCenterOfMass, [MarshalAs(UnmanagedType.U1)] bool updateMassProperties, Activation activationMode);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_ActivateBody(nint handle, uint bodyId);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_DeactivateBody(nint handle, uint bodyId);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetObjectLayer(nint handle, uint bodyId, JPH_ObjectLayer layer);

    [LibraryImport(LibName)]
    public static partial JPH_ObjectLayer JPH_BodyInterface_GetObjectLayer(nint handle, uint bodyId);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyInterface_GetWorldTransform))]
    public static partial void JPH_BodyInterface_GetWorldTransform(nint handle, uint bodyId, Matrix4x4* result);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyInterface_GetWorldTransform))]
    public static partial void JPH_BodyInterface_GetWorldTransformDouble(nint handle, uint bodyId, RMatrix4x4* result);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyInterface_GetCenterOfMassTransform))]
    public static partial void JPH_BodyInterface_GetCenterOfMassTransform(nint handle, uint bodyId, Matrix4x4* result);

    [LibraryImport(LibName, EntryPoint = nameof(JPH_BodyInterface_GetCenterOfMassTransform))]
    public static partial void JPH_BodyInterface_GetCenterOfMassTransformDouble(nint handle, uint bodyId, RMatrix4x4* result);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_MoveKinematic(nint handle, uint bodyId, in Vector3 targetPosition, in Quaternion targetRotation, float deltaTime);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_MoveKinematic(nint handle, uint bodyId, in Double3 targetPosition, in Quaternion targetRotation, float deltaTime);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BodyInterface_ApplyBuoyancyImpulse(nint handle, in BodyID bodyId, in Vector3 surfacePosition, in Vector3 surfaceNormal, float buoyancy, float linearDrag, float angularDrag, in Vector3 fluidVelocity, in Vector3 gravity, float deltaTime);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BodyInterface_ApplyBuoyancyImpulse(nint handle, in BodyID bodyId, in Double3 surfacePosition, in Vector3 surfaceNormal, float buoyancy, float linearDrag, float angularDrag, in Vector3 fluidVelocity, in Vector3 gravity, float deltaTime);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetLinearAndAngularVelocity(nint handle, uint bodyId, in Vector3 linearVelocity, in Vector3 angularVelocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_GetLinearAndAngularVelocity(nint handle, uint bodyId, out Vector3 linearVelocity, out Vector3 angularVelocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_AddLinearVelocity(nint handle, uint bodyId, in Vector3 linearVelocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_AddLinearAndAngularVelocity(nint handle, uint bodyId, in Vector3 linearVelocity, in Vector3 angularVelocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetAngularVelocity(nint handle, uint bodyId, in Vector3 angularVelocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_GetAngularVelocity(nint handle, uint bodyId, out Vector3 angularVelocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_GetPointVelocity(nint handle, uint bodyId, in /*RVec3*/Vector3* point, Vector3* velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_AddForce(nint handle, uint bodyId, in Vector3 force);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_AddForce2(nint handle, uint bodyId, in Vector3 force, in /*RVec3*/Vector3 point);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_AddTorque(nint handle, uint bodyId, in Vector3 torque);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_AddForceAndTorque(nint handle, uint bodyId, in Vector3 force, in Vector3 torque);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_AddImpulse(nint handle, uint bodyId, in Vector3 impulse);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_AddImpulse2(nint handle, uint bodyId, in Vector3 impulse, in /*RVec3*/Vector3 point);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_AddAngularImpulse(nint handle, uint bodyId, in Vector3 angularImpulse);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_GetInverseInertia(nint handle, uint bodyId, out Matrix4x4 result);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetGravityFactor(nint handle, uint bodyId, float value);

    [LibraryImport(LibName)]
    public static partial float JPH_BodyInterface_GetGravityFactor(nint handle, uint bodyId);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetUseManifoldReduction(nint handle, uint bodyId, [MarshalAs(UnmanagedType.U1)] bool value);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BodyInterface_GetUseManifoldReduction(nint handle, uint bodyId);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_SetUserData(nint handle, uint bodyId, ulong userData);

    [LibraryImport(LibName)]
    public static partial ulong JPH_BodyInterface_GetUserData(nint handle, uint bodyId);

    [LibraryImport(LibName)]
    public static partial nint JPH_BodyInterface_GetMaterial(nint handle, uint bodyId, SubShapeID subShapeID);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyInterface_InvalidateContactCache(nint handle, uint bodyId);

    /* BodyLockInterface */
    [LibraryImport(LibName)]
    public static partial nint JPH_PhysicsSystem_GetBodyLockInterface(nint system);

    [LibraryImport(LibName)]
    public static partial nint JPH_PhysicsSystem_GetBodyLockInterfaceNoLock(nint system);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyLockInterface_LockRead(nint lockInterface, uint bodyID, out BodyLockRead @lock);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyLockInterface_UnlockRead(nint lockInterface, in BodyLockRead @lock);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyLockInterface_LockWrite(nint lockInterface, uint bodyID, out BodyLockWrite @lock);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyLockInterface_UnlockWrite(nint lockInterface, in BodyLockWrite @lock);

    [LibraryImport(LibName)]
    public static partial nint JPH_BodyLockInterface_LockMultiRead(nint lockInterface, uint* bodyIDs, uint count);
    [LibraryImport(LibName)]
    public static partial void JPH_BodyLockMultiRead_Destroy(nint ioLock);
    [LibraryImport(LibName)]
    public static partial nint JPH_BodyLockMultiRead_GetBody(nint ioLock, uint bodyIndex);

    [LibraryImport(LibName)]
    public static partial nint JPH_BodyLockInterface_LockMultiWrite(nint lockInterface, uint* bodyIDs, uint count);
    [LibraryImport(LibName)]
    public static partial void JPH_BodyLockMultiWrite_Destroy(nint ioLock);
    [LibraryImport(LibName)]
    public static partial nint JPH_BodyLockMultiWrite_GetBody(nint ioLock, uint bodyIndex);

    /* JPH_MotionProperties */
    [LibraryImport(LibName)]
    public static partial AllowedDOFs JPH_MotionProperties_GetAllowedDOFs(nint properties);

    [LibraryImport(LibName)]
    public static partial void JPH_MotionProperties_SetLinearDamping(nint properties, float damping);

    [LibraryImport(LibName)]
    public static partial float JPH_MotionProperties_GetLinearDamping(nint properties);

    [LibraryImport(LibName)]
    public static partial void JPH_MotionProperties_SetAngularDamping(nint properties, float damping);

    [LibraryImport(LibName)]
    public static partial float JPH_MotionProperties_GetAngularDamping(nint properties);

    [LibraryImport(LibName)]
    public static partial float JPH_MotionProperties_GetInverseMassUnchecked(nint properties);

    [LibraryImport(LibName)]
    public static partial float JPH_MotionProperties_SetMassProperties(nint properties, AllowedDOFs allowedDOFs, in MassProperties massProperties);

    [LibraryImport(LibName)]
    public static partial void JPH_MotionProperties_SetInverseMass(nint properties, float inverseMass);
    [LibraryImport(LibName)]
    public static partial void JPH_MotionProperties_GetInverseInertiaDiagonal(nint properties, Vector3* result);
    [LibraryImport(LibName)]
    public static partial void JPH_MotionProperties_GetInertiaRotation(nint properties, Quaternion* result);
    [LibraryImport(LibName)]
    public static partial void JPH_MotionProperties_SetInverseInertia(nint properties, Vector3* diagonal, Quaternion* rot);

    [LibraryImport(LibName)]
    public static partial void JPH_MotionProperties_ScaleToMass(nint properties, float mass);

    [LibraryImport(LibName)]
    public static partial void JPH_MassProperties_DecomposePrincipalMomentsOfInertia(in MassProperties properties, out Matrix4x4 rotation, out Vector3 diagonal);

    [LibraryImport(LibName)]
    public static partial void JPH_MassProperties_ScaleToMass(MassProperties* properties, float mass);

    [LibraryImport(LibName)]
    public static partial void JPH_MassProperties_GetEquivalentSolidBoxSize(float mass, in Vector3 inertiaDiagonal, out Vector3 result);


    /* BodyLockInterface */
    [LibraryImport(LibName)]
    public static partial nint JPH_PhysicsSystem_GetBroadPhaseQuery(nint system);

    [LibraryImport(LibName)]
    public static partial nint JPH_PhysicsSystem_GetNarrowPhaseQuery(nint system);

    [LibraryImport(LibName)]
    public static partial nint JPH_PhysicsSystem_GetNarrowPhaseQueryNoLock(nint system);

    public struct JPH_CollideSettingsBase
    {
        public ActiveEdgeMode activeEdgeMode;
        public CollectFacesMode collectFacesMode;
        public float collisionTolerance;
        public float penetrationTolerance;
        public Vector3 activeEdgeMovementDirection;
    }

    public struct JPH_CollideShapeSettings
    {
        public JPH_CollideSettingsBase @base;
        public float maxSeparationDistance;

        /// How backfacing triangles should be treated
        public BackFaceMode backFaceMode;
    }

    [LibraryImport(LibName)]
    public static partial void JPH_CollideShapeSettings_Init(JPH_CollideShapeSettings* settings);

    public struct JPH_ShapeCastSettings
    {
        public JPH_CollideSettingsBase @base;    /* Inherics JPH_CollideSettingsBase */
        public BackFaceMode backFaceModeTriangles;
        public BackFaceMode backFaceModeConvex;
        public bool useShrunkenShapeAndConvexRadius;
        public bool returnDeepestPoint;
    }

    [LibraryImport(LibName)]
    public static partial void JPH_ShapeCastSettings_Init(JPH_ShapeCastSettings* settings);

    #region BroadPhaseQuery
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BroadPhaseQuery_CastRay(nint query,
        in Vector3 origin, in Vector3 direction,
        JPH_RayCastBodyCollectorCallback callback,
        nint userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BroadPhaseQuery_CollideAABox(nint query,
        in BoundingBox box,
        JPH_CollideShapeBodyCollectorCallback callback,
        nint userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BroadPhaseQuery_CollideSphere(nint query,
        in Vector3 center, float radius,
        JPH_CollideShapeBodyCollectorCallback callback,
        nint userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_BroadPhaseQuery_CollidePoint(nint query,
        in Vector3 point,
        JPH_CollideShapeBodyCollectorCallback callback,
        nint userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter);
    #endregion

    #region NarrowPhaseQuery
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CastRay(nint system,
        in Vector3 origin, in Vector3 direction,
        out RayCastResult hit,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter,
        nint bodyFilter);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CastRay(nint system,
        in Double3 origin, in Vector3 direction,
        out RayCastResult hit,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter,
        nint bodyFilter);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CastRay2(nint system,
        in Vector3 origin, in Vector3 direction,
        RayCastSettings* rayCastSettings,
        JPH_CastRayCollector callback,
        nint userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter,
        nint bodyFilter,
        nint shapeFilter);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CastRay2(nint system,
        in Double3 origin, in Vector3 direction,
        RayCastSettings* rayCastSettings,
        JPH_CastRayCollector callback,
        nint userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter,
        nint bodyFilter,
        nint shapeFilter);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CastRay3(nint system,
        in Vector3 origin, in Vector3 direction,
        RayCastSettings* rayCastSettings,
        CollisionCollectorType collectorType,
        JPH_CastRayResultCallback callback, nint userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter,
        nint bodyFilter,
        nint shapeFilter);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CastRay3(nint system,
        in Double3 origin, in Vector3 direction,
        RayCastSettings* rayCastSettings,
        CollisionCollectorType collectorType,
        JPH_CastRayResultCallback callback, nint userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter,
        nint bodyFilter,
        nint shapeFilter);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CollidePoint(nint query,
        in Vector3 point,
        JPH_CollidePointCollector callback,
        nint userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter,
        nint bodyFilter,
        nint shapeFilter
        );

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CollidePoint(nint query,
        in Double3 point,
        JPH_CollidePointCollector callback,
        nint userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter,
        nint bodyFilter,
        nint shapeFilter
        );

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CollidePoint2(nint query,
        in Vector3 point,
        CollisionCollectorType collectorType,
        JPH_CollidePointResultCallback callback,
        nint userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter,
        nint bodyFilter,
        nint shapeFilter
        );

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CollidePoint2(nint query,
        in Double3 point,
        CollisionCollectorType collectorType,
        JPH_CollidePointResultCallback callback,
        nint userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter,
        nint bodyFilter,
        nint shapeFilter
        );

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CollideShape(nint query,
        nint shape, in Vector3 scale, in Matrix4x4 centerOfMassTransform,
        JPH_CollideShapeSettings* settings,
        in Vector3 baseOffset,
        JPH_CollideShapeCollector callback,
        nint userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter,
        nint bodyFilter,
        nint shapeFilter
        );

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CollideShape(nint query,
        nint shape, in Vector3 scale, in RMatrix4x4 centerOfMassTransform,
        JPH_CollideShapeSettings* settings,
        in Double3 baseOffset,
        JPH_CollideShapeCollector callback,
        nint userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter,
        nint bodyFilter,
        nint shapeFilter
        );

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CollideShape2(nint query,
        nint shape, in Vector3 scale, in Matrix4x4 centerOfMassTransform,
        JPH_CollideShapeSettings* settings,
        in Vector3 baseOffset,
        CollisionCollectorType collectorType,
        JPH_CollideShapeResultCallback callback,
        nint userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter,
        nint bodyFilter,
        nint shapeFilter
        );

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CollideShape2(nint query,
        nint shape, in Vector3 scale, in RMatrix4x4 centerOfMassTransform,
        JPH_CollideShapeSettings* settings,
        in Double3 baseOffset,
        CollisionCollectorType collectorType,
        JPH_CollideShapeResultCallback callback,
        nint userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter,
        nint bodyFilter,
        nint shapeFilter
        );

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CastShape(nint query,
        nint shape,
        in Matrix4x4 worldTransform, in Vector3 direction,
        JPH_ShapeCastSettings* settings,
        in Vector3 baseOffset,
        JPH_CastShapeCollector callback, nint userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter,
        nint bodyFilter,
        nint shapeFilter
        );

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CastShape(nint query,
        nint shape,
        in RMatrix4x4 worldTransform, in Vector3 direction,
        JPH_ShapeCastSettings* settings,
        in Double3 baseOffset,
        JPH_CastShapeCollector callback,
        nint userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter,
        nint bodyFilter,
        nint shapeFilter
        );

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CastShape2(nint query,
        nint shape,
        in Matrix4x4 worldTransform, in Vector3 direction,
        JPH_ShapeCastSettings* settings,
        in Vector3 baseOffset,
        CollisionCollectorType collectorType,
        JPH_CastShapeResultCallback callback,
        nint userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter,
        nint bodyFilter,
        nint shapeFilter
        );

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_NarrowPhaseQuery_CastShape2(nint query,
        nint shape,
        in RMatrix4x4 worldTransform, in Vector3 direction,
        JPH_ShapeCastSettings* settings,
        in Double3 baseOffset,
        CollisionCollectorType collectorType,
        JPH_CastShapeResultCallback callback,
        nint userData,
        nint broadPhaseLayerFilter,
        nint objectLayerFilter,
        nint bodyFilter,
        nint shapeFilter
        );
    #endregion

    /* Body */
    [LibraryImport(LibName)]
    public static partial uint JPH_Body_GetID(nint body);

    [LibraryImport(LibName)]
    public static partial BodyType JPH_Body_GetBodyType(nint body);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetWorldSpaceBounds(nint body, out BoundingBox box);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetWorldSpaceSurfaceNormal(nint body, uint subShapeID, in Vector3 position, out Vector3 normal);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetWorldSpaceSurfaceNormal(nint body, uint subShapeID, in Double3 position, out Vector3 normal);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_IsActive(nint handle);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_IsStatic(nint handle);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_IsKinematic(nint handle);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_IsDynamic(nint handle);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_CanBeKinematicOrDynamic(nint handle);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_IsSensor(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetIsSensor(nint handle, [MarshalAs(UnmanagedType.U1)] bool value);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetCollideKinematicVsNonDynamic(nint handle, [MarshalAs(UnmanagedType.U1)] bool value);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_GetCollideKinematicVsNonDynamic(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetUseManifoldReduction(nint handle, [MarshalAs(UnmanagedType.U1)] bool value);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_GetUseManifoldReduction(nint handle);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_GetUseManifoldReductionWithBody(nint handle, nint other);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetEnhancedInternalEdgeRemoval(nint handle, [MarshalAs(UnmanagedType.U1)] bool value);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_GetEnhancedInternalEdgeRemoval(nint handle);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_GetEnhancedInternalEdgeRemovalWithBody(nint handle, nint other);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetApplyGyroscopicForce(nint handle, [MarshalAs(UnmanagedType.U1)] bool value);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_GetApplyGyroscopicForce(nint handle);

    [LibraryImport(LibName)]
    public static partial nint JPH_Body_GetMotionProperties(nint handle);
    [LibraryImport(LibName)]
    public static partial nint JPH_Body_GetMotionPropertiesUnchecked(nint handle);

    [LibraryImport(LibName)]
    public static partial MotionType JPH_Body_GetMotionType(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetMotionType(nint handle, MotionType motionType);

    [LibraryImport(LibName)]
    public static partial BroadPhaseLayer JPH_Body_GetBroadPhaseLayer(nint handle);
    [LibraryImport(LibName)]
    public static partial ObjectLayer JPH_Body_GetObjectLayer(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetCollissionGroup(nint body, out JPH_CollisionGroup result);
    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetCollissionGroup(nint body, in JPH_CollisionGroup value);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_GetAllowSleeping(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetAllowSleeping(nint handle, [MarshalAs(UnmanagedType.U1)] bool motionType);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_ResetSleepTimer(nint handle);

    [LibraryImport(LibName)]
    public static partial float JPH_Body_GetFriction(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetFriction(nint handle, float value);

    [LibraryImport(LibName)]
    public static partial float JPH_Body_GetRestitution(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetRestitution(nint handle, float value);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetPosition(nint handle, out Vector3 result);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetPosition(nint handle, out Double3 result);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetRotation(nint handle, out Quaternion result);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetCenterOfMassPosition(nint handle, out Vector3 result);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetCenterOfMassPosition(nint handle, out Double3 result);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetWorldTransform(nint handle, out Matrix4x4 result);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetCenterOfMassTransform(nint handle, out Matrix4x4 result);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetWorldTransform(nint handle, out RMatrix4x4 result);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetCenterOfMassTransform(nint handle, out RMatrix4x4 result);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetInverseCenterOfMassTransform(nint handle, out Matrix4x4 result);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetInverseCenterOfMassTransform(nint handle, out RMatrix4x4 result);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetLinearVelocity(nint handle, out Vector3 result);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetLinearVelocity(nint handle, in Vector3 velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetLinearVelocityClamped(nint handle, in Vector3 velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetAngularVelocity(nint handle, out Vector3 velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetAngularVelocity(nint handle, in Vector3 velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetAngularVelocityClamped(nint handle, in Vector3 velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetPointVelocityCOM(nint handle, in Vector3 pointRelativeToCOM, out Vector3 result);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetPointVelocity(nint handle, in Vector3 point, out Vector3 result);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetPointVelocity(nint handle, in Double3 point, out Vector3 result);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_AddForce(nint handle, in Vector3 velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_AddForceAtPosition(nint handle, in Vector3 velocity, in Vector3 position);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_AddForceAtPosition(nint handle, in Vector3 velocity, in Double3 position);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_AddTorque(nint handle, in Vector3 value);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetAccumulatedForce(nint handle, out Vector3 force);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetAccumulatedTorque(nint handle, out Vector3 force);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_ResetForce(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_ResetTorque(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_ResetMotion(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_GetInverseInertia(nint handle, out Matrix4x4 result);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_AddImpulse(nint handle, in Vector3 impulse);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_AddImpulseAtPosition(nint handle, in Vector3 impulse, in Vector3 position);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_AddImpulseAtPosition(nint handle, in Vector3 impulse, in Double3 position);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_AddAngularImpulse(nint handle, in Vector3 angularImpulse);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_MoveKinematic(nint handle, in Vector3 targetPosition, in Quaternion targetRotation, float deltaTime);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_MoveKinematic(nint handle, in Double3 targetPosition, in Quaternion targetRotation, float deltaTime);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_ApplyBuoyancyImpulse(nint handle, in Vector3 surfacePosition, in Vector3 surfaceNormal, float buoyancy, float linearDrag, float angularDrag, in Vector3 fluidVelocity, in Vector3 gravity, float deltaTime);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_ApplyBuoyancyImpulse(nint handle, in Double3 surfacePosition, in Vector3 surfaceNormal, float buoyancy, float linearDrag, float angularDrag, in Vector3 fluidVelocity, in Vector3 gravity, float deltaTime);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_IsInBroadPhase(nint handle);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Body_IsCollisionCacheInvalid(nint handle);

    [LibraryImport(LibName)]
    public static partial nint JPH_Body_GetShape(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetUserData(nint handle, ulong userData);

    [LibraryImport(LibName)]
    public static partial ulong JPH_Body_GetUserData(nint handle);

    // ContactListener
    public struct JPH_ContactListener_Procs
    {
        public delegate* unmanaged<nint, nint, nint, Vector3*, CollideShapeResult*, uint> OnContactValidate;
        public delegate* unmanaged<nint, nint, nint, nint, nint, void> OnContactAdded;
        public delegate* unmanaged<nint, nint, nint, nint, nint, void> OnContactPersisted;
        public delegate* unmanaged<nint, SubShapeIDPair*, void> OnContactRemoved;
    }

    public struct JPH_ContactListener_ProcsDouble
    {
        public delegate* unmanaged<nint, nint, nint, Double3*, CollideShapeResult*, uint> OnContactValidate;
        public delegate* unmanaged<nint, nint, nint, nint, nint, void> OnContactAdded;
        public delegate* unmanaged<nint, nint, nint, nint, nint, void> OnContactPersisted;
        public delegate* unmanaged<nint, SubShapeIDPair*, void> OnContactRemoved;
    }

    [LibraryImport(LibName)]
    public static partial void JPH_ContactListener_SetProcs(in JPH_ContactListener_Procs procs);

    [LibraryImport(LibName)]
    public static partial nint JPH_ContactListener_SetProcs(in JPH_ContactListener_ProcsDouble procs);

    [LibraryImport(LibName)]
    public static partial nint JPH_ContactListener_Create(nint userData);

    [LibraryImport(LibName)]
    public static partial void JPH_ContactListener_Destroy(nint handle);

    // BodyActivationListener
    public struct JPH_BodyActivationListener_Procs
    {
        public delegate* unmanaged<nint, uint, ulong, void> OnBodyActivated;
        public delegate* unmanaged<nint, uint, ulong, void> OnBodyDeactivated;
    }

    [LibraryImport(LibName)]
    public static partial void JPH_BodyActivationListener_SetProcs(in JPH_BodyActivationListener_Procs procs);

    [LibraryImport(LibName)]
    public static partial nint JPH_BodyActivationListener_Create(nint userData);

    [LibraryImport(LibName)]
    public static partial void JPH_BodyActivationListener_Destroy(nint handle);

    /* ContactManifold */
    [LibraryImport(LibName)]
    public static partial void JPH_ContactManifold_GetWorldSpaceNormal(nint manifold, out Vector3 result);

    [LibraryImport(LibName)]
    public static partial float JPH_ContactManifold_GetPenetrationDepth(nint manifold);
    [LibraryImport(LibName)]
    public static partial SubShapeID JPH_ContactManifold_GetSubShapeID1(nint manifold);
    [LibraryImport(LibName)]
    public static partial SubShapeID JPH_ContactManifold_GetSubShapeID2(nint manifold);
    [LibraryImport(LibName)]
    public static partial uint JPH_ContactManifold_GetPointCount(nint manifold);
    [LibraryImport(LibName)]
    public static partial void JPH_ContactManifold_GetWorldSpaceContactPointOn1(nint manifold, uint index, Vector3* result); // JPH_RVec3
    [LibraryImport(LibName)]
    public static partial void JPH_ContactManifold_GetWorldSpaceContactPointOn2(nint manifold, uint index, Vector3* result); // JPH_RVec3

    /* ContactSettings */
    [LibraryImport(LibName)]
    public static partial float JPH_ContactSettings_GetFriction(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_ContactSettings_SetFriction(nint settings, float friction);
    [LibraryImport(LibName)]
    public static partial float JPH_ContactSettings_GetRestitution(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_ContactSettings_SetRestitution(nint settings, float restitution);
    [LibraryImport(LibName)]
    public static partial float JPH_ContactSettings_GetInvMassScale1(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_ContactSettings_SetInvMassScale1(nint settings, float scale);
    [LibraryImport(LibName)]
    public static partial float JPH_ContactSettings_GetInvInertiaScale1(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_ContactSettings_SetInvInertiaScale1(nint settings, float scale);
    [LibraryImport(LibName)]
    public static partial float JPH_ContactSettings_GetInvMassScale2(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_ContactSettings_SetInvMassScale2(nint settings, float scale);
    [LibraryImport(LibName)]
    public static partial float JPH_ContactSettings_GetInvInertiaScale2(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_ContactSettings_SetInvInertiaScale2(nint settings, float scale);
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_ContactSettings_GetIsSensor(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_ContactSettings_SetIsSensor(nint settings, [MarshalAs(UnmanagedType.U1)] bool sensor);
    [LibraryImport(LibName)]
    public static partial void JPH_ContactSettings_GetRelativeLinearSurfaceVelocity(nint settings, Vector3* result);
    [LibraryImport(LibName)]
    public static partial void JPH_ContactSettings_SetRelativeLinearSurfaceVelocity(nint settings, Vector3* velocity);
    [LibraryImport(LibName)]
    public static partial void JPH_ContactSettings_GetRelativeAngularSurfaceVelocity(nint settings, Vector3* result);
    [LibraryImport(LibName)]
    public static partial void JPH_ContactSettings_SetRelativeAngularSurfaceVelocity(nint settings, Vector3* velocity);

    #region CharacterBase
    public struct JPH_CharacterBaseSettings
    {
        public Vector3 up;
        public Plane supportingVolume;
        public float maxSlopeAngle;
        public bool enhancedInternalEdgeRemoval;
        public /*const JPH_Shape**/nint shape;
    }

    /* CharacterBase */
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterBase_Destroy(nint handle);

    [LibraryImport(LibName)]
    public static partial float JPH_CharacterBase_GetCosMaxSlopeAngle(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterBase_SetMaxSlopeAngle(nint handle, float value);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterBase_GetUp(nint handle, out Vector3 result);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterBase_SetUp(nint handle, in Vector3 value);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_CharacterBase_IsSlopeTooSteep(nint handle, in Vector3 value);

    [LibraryImport(LibName)]
    public static partial nint JPH_CharacterBase_GetShape(nint handle);

    [LibraryImport(LibName)]
    public static partial GroundState JPH_CharacterBase_GetGroundState(nint handle);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_CharacterBase_IsSupported(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterBase_GetGroundPosition(nint handle, out Vector3 position); // RVec3

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterBase_GetGroundNormal(nint handle, out Vector3 normal);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterBase_GetGroundVelocity(nint handle, out Vector3 velocity);

    [LibraryImport(LibName)]
    public static partial nint JPH_CharacterBase_GetGroundMaterial(nint handle);

    [LibraryImport(LibName)]
    public static partial uint JPH_CharacterBase_GetGroundBodyId(nint handle);

    [LibraryImport(LibName)]
    public static partial uint JPH_CharacterBase_GetGroundSubShapeId(nint handle);

    [LibraryImport(LibName)]
    public static partial ulong JPH_CharacterBase_GetGroundUserData(nint handle);
    #endregion

    #region Characted
    public struct JPH_CharacterSettings     /* Inherics JPH_CharacterBaseSettings */
    {
        public JPH_CharacterBaseSettings baseSettings;
        public ObjectLayer layer;
        public float mass;
        public float friction;
        public float gravityFactor;
        public AllowedDOFs allowedDOFs;
    }

    /* CharacterSettings */
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterSettings_Init(JPH_CharacterSettings* settings);

    /* Character */
    [LibraryImport(LibName)]
    public static partial nint JPH_Character_Create(JPH_CharacterSettings* settings, in Vector3 position, in Quaternion rotation, ulong userData,/*JPH_PhysicsSystem**/ nint system);

    [LibraryImport(LibName)]
    public static partial nint JPH_Character_Create(JPH_CharacterSettings* settings, in Double3 position, in Quaternion rotation, ulong userData, nint physicsSystem);

    [LibraryImport(LibName)]
    public static partial void JPH_Character_AddToPhysicsSystem(nint character, Activation activationMode /*= JPH_ActivationActivate */, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);
    [LibraryImport(LibName)]
    public static partial void JPH_Character_RemoveFromPhysicsSystem(nint character, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);
    [LibraryImport(LibName)]
    public static partial void JPH_Character_Activate(nint character, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);
    [LibraryImport(LibName)]
    public static partial void JPH_Character_PostSimulation(nint character, float maxSeparationDistance, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);

    [LibraryImport(LibName)]
    public static partial void JPH_Character_SetLinearAndAngularVelocity(nint character, in Vector3 linearVelocity, in Vector3 angularVelocity, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);
    [LibraryImport(LibName)]
    public static partial void JPH_Character_GetLinearVelocity(nint character, out Vector3 result);
    [LibraryImport(LibName)]
    public static partial void JPH_Character_SetLinearVelocity(nint character, in Vector3 value, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);
    [LibraryImport(LibName)]
    public static partial void JPH_Character_AddLinearVelocity(nint character, in Vector3 value, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);
    [LibraryImport(LibName)]
    public static partial void JPH_Character_AddImpulse(nint character, in Vector3 value, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);
    [LibraryImport(LibName)]
    public static partial uint JPH_Character_GetBodyID(nint character);

    [LibraryImport(LibName)]
    public static partial void JPH_Character_GetPositionAndRotation(nint character, out Vector3 position, out Quaternion rotation, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);

    [LibraryImport(LibName)]
    public static partial void JPH_Character_GetPositionAndRotation(nint character, out Double3 position, out Quaternion rotation, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);

    [LibraryImport(LibName)]
    public static partial void JPH_Character_SetPositionAndRotation(nint character, in Vector3 position, in Quaternion rotation, Activation activationMode, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);

    [LibraryImport(LibName)]
    public static partial void JPH_Character_SetPositionAndRotation(nint character, in Double3 position, in Quaternion rotation, Activation activationMode, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);

    [LibraryImport(LibName)]
    public static partial void JPH_Character_GetPosition(nint character, out Vector3 position, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);

    [LibraryImport(LibName)]
    public static partial void JPH_Character_GetPosition(nint character, out Double3 position, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);

    [LibraryImport(LibName)]
    public static partial void JPH_Character_SetPosition(nint character, in Vector3 position, Activation activationMode, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);

    [LibraryImport(LibName)]
    public static partial void JPH_Character_SetPosition(nint character, in Double3 position, Activation activationMode, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);
    [LibraryImport(LibName)]
    public static partial void JPH_Character_GetRotation(nint character, out Quaternion rotation, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);
    [LibraryImport(LibName)]
    public static partial void JPH_Character_SetRotation(nint character, in Quaternion rotation, Activation activationMode, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);

    [LibraryImport(LibName)]
    public static partial void JPH_Character_GetCenterOfMassPosition(nint character, out Vector3 result, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);

    [LibraryImport(LibName)]
    public static partial void JPH_Character_GetCenterOfMassPosition(nint character, out Double3 result, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);

    [LibraryImport(LibName)]
    public static partial void JPH_Character_GetWorldTransform(nint character, out Matrix4x4 result, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);

    [LibraryImport(LibName)]
    public static partial void JPH_Character_GetWorldTransform(nint character, out RMatrix4x4 result, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);
    [LibraryImport(LibName)]
    public static partial JPH_ObjectLayer JPH_Character_GetLayer(nint character);
    [LibraryImport(LibName)]
    public static partial void JPH_Character_SetLayer(nint character, JPH_ObjectLayer value, [MarshalAs(UnmanagedType.U1)] bool lockBodies /*= true*/);
    [LibraryImport(LibName)]
    public static partial void JPH_Character_SetShape(nint character, /*const JPH_Shape**/nint shape, float maxPenetrationDepth, [MarshalAs(UnmanagedType.U1)] bool lockBodies /*= true*/);
    #endregion

    #region CharacterVirtual
    /* CharacterVirtualSettings */
    public struct JPH_CharacterVirtualSettings     /* Inherics JPH_CharacterBaseSettings */
    {
        public JPH_CharacterBaseSettings baseSettings;
        public CharacterID ID;
        public float mass;
        public float maxStrength;
        public Vector3 shapeOffset;
        public BackFaceMode backFaceMode;
        public float predictiveContactDistance;
        public uint maxCollisionIterations;
        public uint maxConstraintIterations;
        public float minTimeRemaining;
        public float collisionTolerance;
        public float characterPadding;
        public uint maxNumHits;
        public float hitReductionCosMaxAngle;
        public float penetrationRecoverySpeed;
        public /*const JPH_Shape**/nint innerBodyShape;
        public BodyID innerBodyIDOverride;
        public ObjectLayer innerBodyLayer;
    }


    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtualSettings_Init(JPH_CharacterVirtualSettings* settings);
    /* CharacterVirtual */
    [LibraryImport(LibName)]
    public static partial nint JPH_CharacterVirtual_Create(JPH_CharacterVirtualSettings* settings, in Vector3 position, in Quaternion rotation, ulong userData, nint physicsSystem);

    [LibraryImport(LibName)]
    public static partial nint JPH_CharacterVirtual_Create(JPH_CharacterVirtualSettings* settings, in Double3 position, in Quaternion rotation, ulong userData, nint physicsSystem);

    [LibraryImport(LibName)]
    public static partial CharacterID JPH_CharacterVirtual_GetID(nint handle);

    [LibraryImport(LibName)]
    public static partial nint JPH_CharacterVirtual_SetListener(nint handle, nint listener);

    [LibraryImport(LibName)]
    public static partial nint JPH_CharacterVirtual_SetCharacterVsCharacterCollision(nint handle, nint listener);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_GetLinearVelocity(nint handle, out Vector3 velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_SetLinearVelocity(nint handle, in Vector3 velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_GetPosition(nint handle, out Vector3 position); // RVec3

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_SetPosition(nint handle, in Vector3 position);// RVec3

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_GetRotation(nint handle, out Quaternion rotation);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_SetRotation(nint handle, in Quaternion rotation);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_GetWorldTransform(nint shape, out Matrix4x4 result); //RMatrix4x4

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_GetCenterOfMassTransform(nint shape, out Matrix4x4 result); //RMatrix4x4

    [LibraryImport(LibName)]
    public static partial float JPH_CharacterVirtual_GetMass(nint handle);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_SetMass(nint handle, float value);
    [LibraryImport(LibName)]
    public static partial float JPH_CharacterVirtual_GetMaxStrength(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_SetMaxStrength(nint settings, float value);

    [LibraryImport(LibName)]
    public static partial float JPH_CharacterVirtual_GetPenetrationRecoverySpeed(nint character);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_SetPenetrationRecoverySpeed(nint character, float value);
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_CharacterVirtual_GetEnhancedInternalEdgeRemoval(nint character);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_SetEnhancedInternalEdgeRemoval(nint character, [MarshalAs(UnmanagedType.U1)] bool value);
    [LibraryImport(LibName)]
    public static partial float JPH_CharacterVirtual_GetCharacterPadding(nint character);
    [LibraryImport(LibName)]
    public static partial uint JPH_CharacterVirtual_GetMaxNumHits(nint character);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_SetMaxNumHits(nint character, uint value);
    [LibraryImport(LibName)]
    public static partial float JPH_CharacterVirtual_GetHitReductionCosMaxAngle(nint character);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_SetHitReductionCosMaxAngle(nint character, float value);
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_CharacterVirtual_GetMaxHitsExceeded(nint character);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_GetShapeOffset(nint character, out Vector3 result);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_SetShapeOffset(nint character, in Vector3 value);

    [LibraryImport(LibName)]
    public static partial ulong JPH_CharacterVirtual_GetUserData(nint character);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_SetUserData(nint character, ulong value);

    [LibraryImport(LibName)]
    public static partial uint JPH_CharacterVirtual_GetInnerBodyID(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_Update(nint handle, float deltaTime, JPH_ObjectLayer layer, nint physicsSytem, nint bodyFilter, nint shapeFilter);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_ExtendedUpdate(nint handle, float deltaTime, ExtendedUpdateSettings* settings, JPH_ObjectLayer layer, nint physicsSytem, nint bodyFilter, nint shapeFilter);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_RefreshContacts(nint handle, JPH_ObjectLayer layer, nint physicsSytem, nint bodyFilter, nint shapeFilter);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_CancelVelocityTowardsSteepSlopes(nint handle, in Vector3 desiredVelocity, out Vector3 velocity);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_StartTrackingContactChanges(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_FinishTrackingContactChanges(nint handle);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_CharacterVirtual_CanWalkStairs(nint character, in Vector3 linearVelocity);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_CharacterVirtual_WalkStairs(nint character, float deltaTime,
        in Vector3 stepUp, in Vector3 stepForward, in Vector3 stepForwardTest, in Vector3 stepDownExtra,
        in ObjectLayer layer, nint system, nint bodyFilter, nint shapeFilter);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_CharacterVirtual_StickToFloor(nint character, in Vector3 stepDown, in ObjectLayer layer, nint system, nint bodyFilter, nint shapeFilter);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_UpdateGroundVelocity(nint character);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_CharacterVirtual_SetShape(
        nint character,
        nint shape, float maxPenetrationDepth,
        JPH_ObjectLayer layer,
        nint system, nint bodyFilter,
        nint shapeFilter);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_SetInnerBodyShape(nint character, nint shape);

    public struct JPH_CharacterVirtualContact
    {
        public ulong hash;
        public BodyID bodyB;
        public CharacterID characterIDB;
        public SubShapeID subShapeIDB;
        public Vector3 position; /*  JPH_RVec3 */
        public Vector3 linearVelocity;
        public Vector3 contactNormal;
        public Vector3 surfaceNormal;
        public float distance;
        public float fraction;
        public MotionType motionTypeB;
        public Bool8 isSensorB;
        public /*const JPH_CharacterVirtual**/nint characterB;
        public ulong userData;
        public /*const JPH_PhysicsMaterial**/nint material;
        public Bool8 hadCollision;
        public Bool8 wasDiscarded;
        public Bool8 canPushCharacter;
    }

    [LibraryImport(LibName)]
    public static partial int JPH_CharacterVirtual_GetNumActiveContacts(nint character);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVirtual_GetActiveContact(nint character, int index, JPH_CharacterVirtualContact* result);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_CharacterVirtual_HasCollidedWithBody(nint character, in BodyID bodyId);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_CharacterVirtual_HasCollidedWith(nint character, CharacterID other);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_CharacterVirtual_HasCollidedWithCharacter(nint character, nint other);

    public struct JPH_CharacterContactListener_Procs
    {
        public delegate* unmanaged<nint, nint, nint, Vector3*, Vector3*, void> OnAdjustBodyVelocity;
        public delegate* unmanaged<nint, nint, BodyID, SubShapeID, Bool8> OnContactValidate;
        public delegate* unmanaged<nint, nint, nint, SubShapeID, Bool8> OnCharacterContactValidate;
        public delegate* unmanaged<nint, nint, BodyID, SubShapeID, Vector3*, Vector3*, CharacterContactSettings*, void> OnContactAdded;
        public delegate* unmanaged<nint, nint, BodyID, SubShapeID, Vector3*, Vector3*, CharacterContactSettings*, void> OnContactPersisted;
        public delegate* unmanaged<nint, nint, BodyID, SubShapeID, void> OnContactRemoved;
        public delegate* unmanaged<nint, nint, nint, SubShapeID, Vector3*, Vector3*, CharacterContactSettings*, void> OnCharacterContactAdded;
        public delegate* unmanaged<nint, nint, nint, SubShapeID, Vector3*, Vector3*, CharacterContactSettings*, void> OnCharacterContactPersisted;
        public delegate* unmanaged<nint, nint, CharacterID, SubShapeID, void> OnCharacterContactRemoved;
        public delegate* unmanaged<nint, nint, BodyID, SubShapeID, Vector3*, Vector3*, Vector3*, nint, Vector3*, Vector3*, void> OnContactSolve;
        public delegate* unmanaged<nint, nint, nint, SubShapeID, Vector3*, Vector3*, Vector3*, nint, Vector3*, Vector3*, void> OnCharacterContactSolve;
    }

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterContactListener_SetProcs(in JPH_CharacterContactListener_Procs procs);

    [LibraryImport(LibName)]
    public static partial nint JPH_CharacterContactListener_Create(nint userData);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterContactListener_Destroy(nint listener);
    #endregion

    #region CharacterVsCharacterCollision
    public struct JPH_CharacterVsCharacterCollision_Procs
    {
        public delegate* unmanaged<nint, nint, Matrix4x4*, JPH_CollideShapeSettings*, Vector3*, void> CollideCharacter;
        public delegate* unmanaged<nint, nint, Matrix4x4*, Vector3*, JPH_ShapeCastSettings*, Vector3*, void> CastCharacter;
    }

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVsCharacterCollision_SetProcs(in JPH_CharacterVsCharacterCollision_Procs procs);


    [LibraryImport(LibName)]
    public static partial nint JPH_CharacterVsCharacterCollision_Create(nint userData);


    [LibraryImport(LibName)]
    public static partial nint JPH_CharacterVsCharacterCollision_CreateSimple();

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVsCharacterCollisionSimple_AddCharacter(nint characterVsCharacter, nint character);
    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVsCharacterCollisionSimple_RemoveCharacter(nint characterVsCharacter, nint character);

    [LibraryImport(LibName)]
    public static partial void JPH_CharacterVsCharacterCollision_Destroy(nint handle);
    #endregion

    #region DebugRenderer
    public struct JPH_DebugRenderer_Procs
    {
        public delegate* unmanaged<nint, Vector3*, Vector3*, uint, void> DrawLine;
        public delegate* unmanaged<nint, Vector3*, Vector3*, Vector3*, uint, DebugRenderer.CastShadow, void> DrawTriangle;
        public delegate* unmanaged<nint, Vector3*, byte*, uint, float, void> DrawText3D;
    }

    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_SetProcs(in JPH_DebugRenderer_Procs procs);

    [LibraryImport(LibName)]
    public static partial nint JPH_DebugRenderer_Create(nint userData);

    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_Destroy(nint renderer);

    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_NextFrame(nint renderer);

    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_SetCameraPos(nint renderer, in Vector3 position);

    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_SetCameraPos(nint renderer, in Double3 position);

    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_DrawLine(nint renderer, in Vector3 from, in Vector3 to, uint color);

    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_DrawLine(nint renderer, in Double3 from, in Double3 to, uint color);

    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_DrawWireBox(nint renderer, in BoundingBox box, uint color);

    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_DrawWireBox2(nint renderer, in Matrix4x4 matrix, in BoundingBox box, uint color);

    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_DrawWireBox2(nint renderer, in RMatrix4x4 matrix, in BoundingBox box, uint color);

    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_DrawMarker(nint renderer, /*RVec3*/ in Vector3 position, uint color, float size);
    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_DrawArrow(nint renderer, /*RVec3*/ in Vector3 from, /*RVec3*/ in Vector3 to, uint color, float size);
    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_DrawCoordinateSystem(nint renderer, /*RMatrix4x4*/ in Matrix4x4 matrix, float size);
    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_DrawPlane(nint renderer, /*RVec3*/ in Vector3 point, in Vector3 normal, uint color, float size);
    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_DrawWireTriangle(nint renderer, /*RVec3*/ in Vector3 v1, /*RVec3*/ in Vector3 v2, /*RVec3*/ in Vector3 v3, uint color);
    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_DrawWireSphere(nint renderer, /*RVec3*/ in Vector3 center, float radius, uint color, int level);
    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_DrawWireUnitSphere(nint renderer, /*RMatrix4x4*/ in Matrix4x4 matrix, uint color, int level);
    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_DrawTriangle(nint renderer, /*RVec3*/ in Vector3 v1, /*RVec3*/ in Vector3 v2, /*RVec3*/ in Vector3 v3, uint color, DebugRenderer.CastShadow castShadow);
    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_DrawBox(nint renderer, in BoundingBox box, uint color, DebugRenderer.CastShadow castShadow, DebugRenderer.DrawMode drawMode);
    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_DrawBox2(nint renderer, /*RMatrix4x4*/ in Matrix4x4 matrix, in BoundingBox box, uint color, DebugRenderer.CastShadow castShadow, DebugRenderer.DrawMode drawMode);
    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_DrawSphere(nint renderer, /*RVec3*/ in Vector3 center, float radius, uint color, DebugRenderer.CastShadow castShadow, DebugRenderer.DrawMode drawMode);
    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_DrawUnitSphere(nint renderer, /*RMatrix4x4*/ in Matrix4x4 matrix, uint color, DebugRenderer.CastShadow castShadow, DebugRenderer.DrawMode drawMode);
    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_DrawCapsule(nint renderer, /*RMatrix4x4*/ in Matrix4x4 matrix, float halfHeightOfCylinder, float radius, uint color, DebugRenderer.CastShadow castShadow, DebugRenderer.DrawMode drawMode);
    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_DrawCylinder(nint renderer, /*RMatrix4x4*/ in Matrix4x4 matrix, float halfHeight, float radius, uint color, DebugRenderer.CastShadow castShadow, DebugRenderer.DrawMode drawMode);
    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_DrawOpenCone(nint renderer, /*RVec3*/ in Vector3 top, in Vector3 axis, in Vector3 perpendicular, float halfAngle, float length, uint color, DebugRenderer.CastShadow castShadow, DebugRenderer.DrawMode drawMode);
    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_DrawSwingConeLimits(nint renderer, /*RMatrix4x4*/ in Matrix4x4 matrix, float swingYHalfAngle, float swingZHalfAngle, float edgeLength, uint color, DebugRenderer.CastShadow castShadow, DebugRenderer.DrawMode drawMode);
    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_DrawSwingPyramidLimits(nint renderer, /*RMatrix4x4*/ in Matrix4x4 matrix, float minSwingYAngle, float maxSwingYAngle, float minSwingZAngle, float maxSwingZAngle, float edgeLength, uint color, DebugRenderer.CastShadow castShadow, DebugRenderer.DrawMode drawMode);
    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_DrawPie(nint renderer, /*RVec3*/ in Vector3 center, float radius, in Vector3 normal, in Vector3 axis, float minAngle, float maxAngle, uint color, DebugRenderer.CastShadow castShadow, DebugRenderer.DrawMode drawMode);
    [LibraryImport(LibName)]
    public static partial void JPH_DebugRenderer_DrawTaperedCylinder(nint renderer, /*RMatrix4x4*/ in Matrix4x4 matrix, float top, float bottom, float topRadius, float bottomRadius, uint color, DebugRenderer.CastShadow castShadow, DebugRenderer.DrawMode drawMode);
    #endregion

    #region Skeleton
    public readonly struct SkeletonJoint
    {
        public readonly byte* name;
        public readonly byte* parentName;
        public readonly int parentJointIndex;
    }

    [LibraryImport(LibName)]
    public static partial nint JPH_Skeleton_Create();
    [LibraryImport(LibName)]
    public static partial void JPH_Skeleton_Destroy(nint skeleton);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial uint JPH_Skeleton_AddJoint(nint skeleton, string name);
    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial uint JPH_Skeleton_AddJoint2(nint skeleton, string name, int parentIndex);
    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial uint JPH_Skeleton_AddJoint3(nint skeleton, string name, string parentName);
    [LibraryImport(LibName)]
    public static partial int JPH_Skeleton_GetJointCount(nint skeleton);
    [LibraryImport(LibName)]
    public static partial void JPH_Skeleton_GetJoint(nint skeleton, int index, out SkeletonJoint joint);
    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial int JPH_Skeleton_GetJointIndex(nint skeleton, string name);
    [LibraryImport(LibName)]
    public static partial void JPH_Skeleton_CalculateParentJointIndices(nint skeleton);
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Skeleton_AreJointsCorrectlyOrdered(nint skeleton);
    #endregion

    #region Ragdoll
    /* Ragdoll */
    [LibraryImport(LibName)]
    public static partial nint JPH_RagdollSettings_Create();
    [LibraryImport(LibName)]
    public static partial void JPH_RagdollSettings_Destroy(nint settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_RagdollSettings_GetSkeleton(nint character);
    [LibraryImport(LibName)]
    public static partial void JPH_RagdollSettings_SetSkeleton(nint character, nint skeleton);
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_RagdollSettings_Stabilize(nint settings);
    [LibraryImport(LibName)]
    public static partial void JPH_RagdollSettings_DisableParentChildCollisions(nint settings, Matrix4x4* jointMatrices /*=nullptr*/, float minSeparationDistance/* = 0.0f*/);
    [LibraryImport(LibName)]
    public static partial void JPH_RagdollSettings_CalculateBodyIndexToConstraintIndex(nint settings);
    [LibraryImport(LibName)]
    public static partial int JPH_RagdollSettings_GetConstraintIndexForBodyIndex(nint settings, int bodyIndex);
    [LibraryImport(LibName)]
    public static partial void JPH_RagdollSettings_CalculateConstraintIndexToBodyIdxPair(nint settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_RagdollSettings_CreateRagdoll(nint settings, nint system, uint collisionGroup, ulong userData);
    [LibraryImport(LibName)]
    public static partial void JPH_Ragdoll_Destroy(nint ragdoll);
    [LibraryImport(LibName)]
    public static partial void JPH_Ragdoll_AddToPhysicsSystem(nint ragdoll, Activation activationMode /*= JPH_ActivationActivate */, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);
    [LibraryImport(LibName)]
    public static partial void JPH_Ragdoll_RemoveFromPhysicsSystem(nint ragdoll, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);
    [LibraryImport(LibName)]
    public static partial void JPH_Ragdoll_Activate(nint ragdoll, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);
    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_Ragdoll_IsActive(nint ragdoll, [MarshalAs(UnmanagedType.U1)] bool lockBodies /* = true */);
    [LibraryImport(LibName)]
    public static partial void JPH_Ragdoll_ResetWarmStart(nint ragdoll);
    #endregion

    [LibraryImport(LibName)]
    public static partial void JPH_EstimateCollisionResponse(nint body1, nint body2, nint manifold, float combinedFriction, float combinedRestitution, float minVelocityForRestitution, int numIterations, CollisionEstimationResult* result);

    sealed class UTF8EncodingRelaxed : UTF8Encoding
    {
        public static new readonly UTF8EncodingRelaxed Default = new UTF8EncodingRelaxed();

        private UTF8EncodingRelaxed() : base(false, false)
        {
        }
    }

    [LibraryImport(LibName)]
    public static partial nint JPH_WheelSettingsWV_Create(
        in Vector3 position,
        in Vector3 suspensionForcePoint,
        in Vector3 suspensionDirection,
        in Vector3 steeringAxis,
        in Vector3 wheelUp,
        in Vector3 wheelForward,
        float suspensionMinLength,
        float suspensionMaxLength,
        float suspensionPreloadLength,
        in SpringSettings suspensionSpring,
        float radius,
        float width,
        [MarshalAs(UnmanagedType.U1)] bool enableSuspensionForcePoint,
        float inertia,
        float angularDamping,
        float maxSteerAngle,
        //LinearCurve longitudinalFriction, // NOTE: BGE: just using default values for now.
        //LinearCurve lateralFriction,      // NOTE: BGE: just using default values for now.
        float maxBrakeTorque,
        float maxHandBrakeTorque);

    [LibraryImport(LibName)]
    public static partial void JPH_WheelSettingsWV_Destroy(nint settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_WheelWV_Create(nint wheelSettingsWV);

    [LibraryImport(LibName)]
    public static partial void JPH_WheelWV_Destroy(nint wheel);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_WheelWV_HasContact(nint wheel);

    [LibraryImport(LibName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static partial bool JPH_WheelWV_HasHitHardPoint(nint wheel);

    [LibraryImport(LibName)]
    public static partial nint JPH_VehicleEngineSettings_Create(
        float maxTorque,
        float minRPM,
        float maxRPM,
        //LinearCurve normalizedTorque, // NOTE: BGE: just using default values for now.
        float inertia,
        float angularDamping);

    [LibraryImport(LibName)]
    public static partial void JPH_VehicleEngineSettings_Destroy(nint settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_VehicleTransmissionSettings_Create(
        TransmissionMode mode,
        //Array<float> gearRatios,		    // NOTE: BGE: just using default values for now.
        //Array<float> reverseGearRatios,   // NOTE: BGE: just using default values for now.
        float switchTime,
        float clutchReleaseTime,
        float switchLatency,
        float shiftUpRPM,
        float shiftDownRPM,
        float clutchStrength);

    [LibraryImport(LibName)]
    public static partial void JPH_VehicleTransmissionSettings_Destroy(nint settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_VehicleCollisionTesterRay_Create(ObjectLayer layer, in Vector3 up, float maxSlopeAngle);

    [LibraryImport(LibName)]
    public static partial void JPH_VehicleCollisionTesterRay_Destroy(nint tester);

    [LibraryImport(LibName)]
    public static partial nint JPH_VehicleCollisionTesterCastSphere_Create(ObjectLayer layer, float radius, in Vector3 up, float maxSlopeAngle);

    [LibraryImport(LibName)]
    public static partial void JPH_VehicleCollisionTesterCastSphere_Destroy(nint tester);

    [LibraryImport(LibName)]
    public static partial nint JPH_VehicleCollisionTesterCastCylinder_Create(ObjectLayer layer, float convexRadiusFraction);

    [LibraryImport(LibName)]
    public static partial void JPH_VehicleCollisionTesterCastCylinder_Destroy(nint tester);

    [LibraryImport(LibName)]
    public static partial nint JPH_VehicleConstraintSettings_Create(
        in Vector3 up,
        in Vector3 forward,
        float maxPitchRollAngle,
        nint* wheels,
        int wheelsCount,
        //VehicleAntiRollBars antiRollBars, // NOTE: BGE: just using default values for now.
        nint vehicleControllerSettings);

    [LibraryImport(LibName)]
    public static partial void JPH_VehicleConstraintSettings_Destroy(nint settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_VehicleConstraint_Create(nint body, nint constraintSettings);

    [LibraryImport(LibName)]
    public static partial void JPH_VehicleConstraint_Destroy(nint controller);

    [LibraryImport(LibName)]
    public static partial nint JPH_VehicleConstraint_AsPhysicsStepListener(nint constraint);

    [LibraryImport(LibName)]
    public static partial nint JPH_VehicleConstraint_GetWheeledVehicleController(nint constraint);

    [LibraryImport(LibName)]
    public static partial void JPH_VehicleConstraint_SetVehicleCollisionTester(nint constraint, nint tester);

    [LibraryImport(LibName)]
    public static partial nint JPH_WheeledVehicleControllerSettings_Create(
        nint engine,
        nint transmission,
        //Array<VehicleDifferentialSettings> differentials,	// NOTE: BGE: just using default values for now.
        float differentialLimitedSlipRatio);

    [LibraryImport(LibName)]
    public static partial void JPH_WheeledVehicleControllerSettings_Destroy(nint settings);

    [LibraryImport(LibName)]
    public static partial nint JPH_WheeledVehicleController_GetConstraint(nint controller);

    [LibraryImport(LibName)]
    public static partial void JPH_WheeledVehicleController_SetForwardInput(nint controller, float forward);

    [LibraryImport(LibName)]
    public static partial void JPH_WheeledVehicleController_SetRightInput(nint controller, float right);

    [LibraryImport(LibName)]
    public static partial void JPH_WheeledVehicleController_SetBrakeInput(nint controller, float brake);

    [LibraryImport(LibName)]
    public static partial void JPH_WheeledVehicleController_SetHandBrakeInput(nint controller, float handBrake);
}
