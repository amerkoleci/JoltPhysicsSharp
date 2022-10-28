// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace JoltPhysicsSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.Bool)]
public delegate bool ObjectVsBroadPhaseLayerFilter(ObjectLayer layer1, BroadPhaseLayer layer2);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.Bool)]
public delegate bool ObjectLayerPairFilter(ObjectLayer layer1, ObjectLayer layer2);

internal static unsafe class JoltApi
{
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
        if (libraryName is not "joltc")
            return false;

        string rid = RuntimeInformation.RuntimeIdentifier;

        string nugetNativeLibsPath = Path.Combine(AppContext.BaseDirectory, $@"runtimes\{rid}\native");
        bool isNuGetRuntimeLibrariesDirectoryPresent = Directory.Exists(nugetNativeLibsPath);
        string dllName = "joltc";

        if (OperatingSystem.IsWindows())
        {
            dllName = "joltc.dll";

            if (!isNuGetRuntimeLibrariesDirectoryPresent)
            {
                rid = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X64 => "win-x64",
                    Architecture.Arm64 => "win-arm64",
                    _ => "win-x64"
                };

                nugetNativeLibsPath = Path.Combine(AppContext.BaseDirectory, $@"runtimes\{rid}\native");
                isNuGetRuntimeLibrariesDirectoryPresent = Directory.Exists(nugetNativeLibsPath);
            }
        }
        else if (OperatingSystem.IsLinux())
        {
            dllName = "libjoltc.so";
        }
        else if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst())
        {
            dllName = "libjoltc.dylib";
        }

        if (isNuGetRuntimeLibrariesDirectoryPresent)
        {
            string joltcPath = Path.Combine(AppContext.BaseDirectory, $@"runtimes\{rid}\native\{dllName}");

            if (NativeLibrary.TryLoad(joltcPath, out nativeLibrary))
            {
                return true;
            }
        }
        else
        {
            if (NativeLibrary.TryLoad("dxcompiler", assembly, searchPath, out nativeLibrary))
            {
                return true;
            }
        }

        nativeLibrary = IntPtr.Zero;
        return false;
    }
#endif


    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool JPH_Init();

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Shutdown();

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_TempAllocatorMalloc_Create();

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_TempAllocator_Create(uint size);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_TempAllocator_Destroy(IntPtr handle);


    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_JobSystemThreadPool_Create(uint maxJobs, uint maxBarriers, int inNumThreads);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_JobSystemThreadPool_Destroy(IntPtr handle);

    //  BroadPhaseLayerInterface
    public struct JPH_BroadPhaseLayerInterface_Procs
    {
        public delegate* unmanaged[Cdecl]<IntPtr, uint> GetNumBroadPhaseLayers;
        public delegate* unmanaged[Cdecl]<IntPtr, ObjectLayer, BroadPhaseLayer> GetBroadPhaseLayer;
        public delegate* unmanaged[Cdecl]<IntPtr, BroadPhaseLayer, IntPtr> GetBroadPhaseLayerName;
    }

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BroadPhaseLayerInterface_SetProcs(JPH_BroadPhaseLayerInterface_Procs procs);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BroadPhaseLayerInterface_Create();

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BroadPhaseLayerInterface_Destroy(IntPtr handle);

    /* ShapeSettings */
    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_ShapeSettings_Destroy(IntPtr shape);

    /* BoxShapeSettings */
    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BoxShapeSettings_Create(in Vector3 halfExtent, float convexRadius);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BoxShape_Create(in Vector3 halfExtent, float convexRadius);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BoxShape_GetHalfExtent(IntPtr handle, out Vector3 halfExtent);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_BoxShape_GetVolume(IntPtr handle);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_BoxShape_GetConvexRadius(IntPtr handle);

    /* SphereShapeSettings */
    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_SphereShapeSettings_Create(float radius);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_SphereShapeSettings_GetRadius(IntPtr shape);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_SphereShapeSettings_SetRadius(IntPtr shape, float radius);

    /* TriangleShapeSettings */
    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_TriangleShapeSettings_Create(in Vector3 v1, in Vector3 v2, in Vector3 v3, float convexRadius);

    /* CapsuleShapeSettings */
    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_CapsuleShapeSettings_Create(float halfHeightOfCylinder, float radius);

    /* CylinderShapeSettings */
    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_CylinderShapeSettings_Create(float halfHeight, float radius, float convexRadius);

    /* ConvexHullShape */
    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_ConvexHullShapeSettings_Create(Vector3* points, int pointsCount, float maxConvexRadius);

    /* MeshShape  */
    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_MeshShapeSettings_Create(Triangle* triangle, int triangleCount);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_MeshShapeSettings_Create2(Vector3* vertices, int verticesCount, IndexedTriangle* triangles, int triangleCount);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_MeshShapeSettings_Sanitize(IntPtr shape);

    /* HeightFieldShape  */
    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_HeightFieldShapeSettings_Create(float* samples, in Vector3 offset, in Vector3 scale, int sampleCount);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_MeshShapeSettings_DetermineMinAndMaxSample(IntPtr settings, out float outMinValue, out float outMaxValue, out float outQuantizationScale);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_MeshShapeSettings_CalculateBitsPerSampleForError(IntPtr settings, float maxError);

    /* TaperedCapsuleShapeSettings */
    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_TaperedCapsuleShapeSettings_Create(float halfHeightOfTaperedCylinder, float topRadius, float bottomRadius);

    /* Shape */
    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Shape_Destroy(IntPtr shape);

    /* SphereShape */
    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_SphereShape_Create(float radius);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_SphereShape_GetRadius(IntPtr shape);

    /* BodyCreationSettings */
    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BodyCreationSettings_Create();

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BodyCreationSettings_Create2(IntPtr shapeSettings, in Vector3 position, in Quaternion rotation, MotionType motionType, ushort objectLayer);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BodyCreationSettings_Create3(IntPtr shape, in Vector3 position, in Quaternion rotation, MotionType motionType, ushort objectLayer);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyCreationSettings_Destroy(IntPtr settings);


    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_PhysicsSystem_Create();

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PhysicsSystem_Destroy(IntPtr handle);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PhysicsSystem_Init(IntPtr handle,
        uint inMaxBodies, uint numBodyMutexes, uint maxBodyPairs, uint maxContactConstraints,
        IntPtr layer,
        ObjectVsBroadPhaseLayerFilter inObjectVsBroadPhaseLayerFilter,
        ObjectLayerPairFilter inObjectLayerPairFilter);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PhysicsSystem_OptimizeBroadPhase(IntPtr handle);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PhysicsSystem_Update(IntPtr handle,
        float deltaTime, int collisionSteps, int integrationSubSteps,
        IntPtr tempAlocator, IntPtr jobSystem);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PhysicsSystem_SetContactListener(IntPtr system, IntPtr listener);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PhysicsSystem_SetBodyActivationListener(IntPtr system, IntPtr listener);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_PhysicsSystem_GetNumBodies(IntPtr system);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_PhysicsSystem_GetNumActiveBodies(IntPtr system);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_PhysicsSystem_GetMaxBodies(IntPtr system);

    /* BodyInterface */
    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_PhysicsSystem_GetBodyInterface(IntPtr system);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BodyInterface_CreateBody(IntPtr handle, IntPtr settings);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_BodyInterface_CreateAndAddBody(IntPtr handle, IntPtr bodyID, ActivationMode activation);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BodyInterface_CreateBodyWithID(IntPtr handle, uint bodyID, IntPtr settings);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BodyInterface_CreateBodyWithoutID(IntPtr handle, IntPtr settings);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_DestroyBody(IntPtr handle, uint bodyID);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_DestroyBodyWithoutID(IntPtr handle, IntPtr body);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool JPH_BodyInterface_AssignBodyID(IntPtr handle, IntPtr body);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool JPH_BodyInterface_AssignBodyID2(IntPtr handle, IntPtr body, uint bodyID);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern IntPtr JPH_BodyInterface_UnassignBodyID(IntPtr handle, uint bodyID);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_AddBody(IntPtr handle, uint bodyID, ActivationMode activation);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_RemoveBody(IntPtr handle, uint bodyID);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetLinearVelocity(IntPtr handle, uint bodyID, in Vector3 velocity);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_GetLinearVelocity(IntPtr handle, uint bodyID, out Vector3 velocity);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_GetCenterOfMassPosition(IntPtr handle, uint bodyID, out Vector3 velocity);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool JPH_BodyInterface_IsActive(IntPtr handle, uint bodyID);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool JPH_BodyInterface_IsAdded(IntPtr handle, uint bodyID);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern MotionType JPH_BodyInterface_GetMotionType(IntPtr handle, uint bodyID);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetMotionType(IntPtr handle, uint bodyID, MotionType motionType, ActivationMode activationMode);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_BodyInterface_GetRestitution(IntPtr handle, uint bodyID);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetRestitution(IntPtr handle, uint bodyID, float value);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_BodyInterface_GetFriction(IntPtr handle, uint bodyID);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetFriction(IntPtr handle, uint bodyID, float value);

    /* Body */
    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_Body_GetID(IntPtr body);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool JPH_Body_IsActive(IntPtr handle);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool JPH_Body_IsStatic(IntPtr handle);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool JPH_Body_IsKinematic(IntPtr handle);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool JPH_Body_IsDynamic(IntPtr handle);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool JPH_Body_IsSensor(IntPtr handle);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern MotionType JPH_Body_GetMotionType(IntPtr handle);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_SetMotionType(IntPtr handle, MotionType motionType);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_Body_GetFriction(IntPtr handle);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_SetFriction(IntPtr handle, float value);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_Body_GetRestitution(IntPtr handle);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_SetRestitution(IntPtr handle, float value);

    // ContactListener
    public struct JPH_ContactListener_Procs
    {
        public delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, IntPtr, ValidateResult> OnContactValidate;
        public delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, void> OnContactAdded;
        public delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, void> OnContactPersisted;
        public delegate* unmanaged[Cdecl]<IntPtr, SubShapeIDPair*, void> OnContactRemoved;
    }

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_ContactListener_SetProcs(JPH_ContactListener_Procs procs);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_ContactListener_Create();

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_ContactListener_Destroy(IntPtr handle);

    // BodyActivationListener
    public struct JPH_BodyActivationListener_Procs
    {
        public delegate* unmanaged[Cdecl]<IntPtr, uint, ulong, void> OnBodyActivated;
        public delegate* unmanaged[Cdecl]<IntPtr, uint, ulong, void> OnBodyDeactivated;
    }

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyActivationListener_SetProcs(JPH_BodyActivationListener_Procs procs);

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BodyActivationListener_Create();

    [DllImport("joltc", CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyActivationListener_Destroy(IntPtr handle);
}
