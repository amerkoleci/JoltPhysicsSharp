// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace JoltPhysicsSharp;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate Bool32 AssertFailedDelegate(string inExpression, string inMessage, string inFile, uint inLine);

internal static unsafe partial class JoltApi
{
    private const string LibName = "joltc";
    private const string LibDoubleName = "joltc_double";

    public static bool DoublePrecision { get; set; }

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

    [LibraryImport(LibName)]
    public static partial Bool32 JPH_Init(uint tempAllocatorSize);

    [LibraryImport(LibName)]
    public static partial void JPH_Shutdown();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_SetAssertFailureHandler(AssertFailedDelegate handler);

    //  BroadPhaseLayerInterface
    [LibraryImport(LibName)]
    public static partial nint JPH_BroadPhaseLayerInterfaceMask_Create( uint numBroadPhaseLayers);

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

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_ObjectVsBroadPhaseLayerFilter_Destroy(nint handle);

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
        public delegate* unmanaged<nint, BroadPhaseLayer, Bool32> ShouldCollide;
    }

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BroadPhaseLayerFilter_SetProcs(JPH_BroadPhaseLayerFilter_Procs procs);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_BroadPhaseLayerFilter_Create();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BroadPhaseLayerFilter_Destroy(nint handle);

    //  ObjectLayerFilter
    public struct JPH_ObjectLayerFilter_Procs
    {
        public delegate* unmanaged<IntPtr, ObjectLayer, Bool32> ShouldCollide;
    }

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_ObjectLayerFilter_SetProcs(JPH_ObjectLayerFilter_Procs procs);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_ObjectLayerFilter_Create();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_ObjectLayerFilter_Destroy(nint handle);

    //  BodyFilter
    public struct JPH_BodyFilter_Procs
    {
        public delegate* unmanaged<IntPtr, BodyID, Bool32> ShouldCollide;
        public delegate* unmanaged<IntPtr, IntPtr, Bool32> ShouldCollideLocked;
    }

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyFilter_SetProcs(JPH_BodyFilter_Procs procs);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_BodyFilter_Create();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyFilter_Destroy(nint handle);

    /* ShapeSettings */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_ShapeSettings_Destroy(nint shape);

    /* ConvexShape */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_ConvexShapeSettings_GetDensity(nint shape);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_ConvexShapeSettings_SetDensity(nint shape, float value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_ConvexShape_GetDensity(nint shape);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_ConvexShape_SetDensity(nint shape, float value);

    /* BoxShapeSettings */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_BoxShapeSettings_Create(in Vector3 halfExtent, float convexRadius);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_BoxShapeSettings_CreateShape(nint settings);

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
    public static extern nint JPH_SphereShapeSettings_CreateShape(nint settings);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_SphereShapeSettings_GetRadius(IntPtr shape);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_SphereShapeSettings_SetRadius(IntPtr shape, float radius);

    /* TriangleShapeSettings */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_TriangleShapeSettings_Create(in Vector3 v1, in Vector3 v2, in Vector3 v3, float convexRadius);

    /* CapsuleShape */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_CapsuleShapeSettings_Create(float halfHeightOfCylinder, float radius);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_CapsuleShape_Create(float halfHeightOfCylinder, float radius);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_CapsuleShape_GetRadius(nint handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_CapsuleShape_GetHalfHeightOfCylinder(nint handle);

    /* CylinderShape */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_CylinderShapeSettings_Create(float halfHeight, float radius, float convexRadius);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_CylinderShape_Create(float halfHeight, float radius);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_CylinderShape_GetRadius(nint handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_CylinderShape_GetHalfHeight(nint handle);

    /* ConvexHullShape */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_ConvexHullShapeSettings_CreateShape(nint settings);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_ConvexHullShapeSettings_Create(Vector3* points, int pointsCount, float maxConvexRadius);

    /* MeshShape  */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_MeshShapeSettings_Create(Triangle* triangle, int triangleCount);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_MeshShapeSettings_Create2(Vector3* vertices, int verticesCount, IndexedTriangle* triangles, int triangleCount);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_MeshShapeSettings_Sanitize(IntPtr shape);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_MeshShapeSettings_CreateShape(nint settings);

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

    /* MutableCompoundShape */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_MutableCompoundShapeSettings_Create();

    /* RotatedTranslatedShape */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_RotatedTranslatedShapeSettings_Create(in Vector3 position, in Quaternion rotation, nint shapeSettings);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_RotatedTranslatedShapeSettings_Create2(in Vector3 position, in Quaternion rotation, nint shape);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_RotatedTranslatedShapeSettings_CreateShape(nint shapeSettings);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_RotatedTranslatedShape_Create(in Vector3 position, in Quaternion rotation, nint shape);

    /* Shape */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Shape_Destroy(IntPtr shape);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Shape_GetLocalBounds(IntPtr shape, BoundingBox* box);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Shape_GetMassProperties(nint shape, out MassProperties properties);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Shape_GetCenterOfMass(nint handle, out Vector3 result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_Shape_GetInnerRadius(nint handle);

    /* SphereShape */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_SphereShape_Create(float radius);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_SphereShape_GetRadius(IntPtr shape);

    /* BodyCreationSettings */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BodyCreationSettings_Create();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BodyCreationSettings_Create2(IntPtr shapeSettings, in Double3 position, in Quaternion rotation, MotionType motionType, ushort objectLayer);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BodyCreationSettings_Create3(IntPtr shape, in Double3 position, in Quaternion rotation, MotionType motionType, ushort objectLayer);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyCreationSettings_Destroy(IntPtr settings);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyCreationSettings_GetLinearVelocity(IntPtr settings, out Vector3 velocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyCreationSettings_SetLinearVelocity(IntPtr settings, in Vector3 velocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyCreationSettings_GetAngularVelocity(IntPtr settings, out Vector3 velocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyCreationSettings_SetAngularVelocity(IntPtr settings, in Vector3 velocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern MotionType JPH_BodyCreationSettings_GetMotionType(IntPtr settings);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyCreationSettings_SetMotionType(IntPtr settings, MotionType value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern AllowedDOFs JPH_BodyCreationSettings_GetAllowedDOFs(IntPtr settings);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyCreationSettings_SetAllowedDOFs(IntPtr settings, AllowedDOFs value);

    /* SoftBodyCreationSettings */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_SoftBodyCreationSettings_Create();

    //[DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    //public static extern void JPH_SoftBodyCreationSettings_Destroy(IntPtr settings);

    #region JPH_Constraint
    /* JPH_Constraint */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_ConstraintSettings_Destroy(IntPtr handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Constraint_Destroy(IntPtr constraint);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_Constraint_GetConstraintSettings(nint constraint);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern Bool32 JPH_Constraint_GetEnabled(IntPtr constraint);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Constraint_SetEnabled(IntPtr constraint, Bool32 value);
    #endregion

    #region JPH_TwoBodyConstraint
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_TwoBodyConstraint_GetBody1(nint constraint);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_TwoBodyConstraint_GetBody2(nint constraint);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_TwoBodyConstraint_GetConstraintToBody1Matrix(nint constraint, out Matrix4x4 result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_TwoBodyConstraint_GetConstraintToBody2Matrix(nint constraint, out Matrix4x4 result);
    #endregion

    #region JPH_PointConstraint
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_PointConstraintSettings_Create();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ConstraintSpace JPH_PointConstraintSettings_GetSpace(IntPtr handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PointConstraintSettings_SetSpace(IntPtr handle, ConstraintSpace value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PointConstraintSettings_GetPoint1(IntPtr handle, out Double3 result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PointConstraintSettings_SetPoint1(IntPtr handle, in Double3 value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PointConstraintSettings_GetPoint2(IntPtr handle, out Double3 result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PointConstraintSettings_SetPoint2(IntPtr handle, in Double3 value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_PointConstraintSettings_CreateConstraint(IntPtr handle, IntPtr body1, IntPtr body2);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PointConstraint_SetPoint1(IntPtr handle, ConstraintSpace space, in Double3 value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PointConstraint_SetPoint2(IntPtr handle, ConstraintSpace space, in Double3 value); 
    #endregion

    #region JPH_DistanceConstraint
    /* JPH_DistanceConstraint */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_DistanceConstraintSettings_Create();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_DistanceConstraintSettings_GetPoint1(nint handle, out Vector3 result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_DistanceConstraintSettings_SetPoint1(nint handle, in Vector3 value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_DistanceConstraintSettings_GetPoint2(nint handle, out Vector3 result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_DistanceConstraintSettings_SetPoint2(nint handle, in Vector3 value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_DistanceConstraintSettings_CreateConstraint(nint handle, nint body1, nint body2);
    #endregion

    #region JPH_HingeConstraint
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_HingeConstraintSettings_Create();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_HingeConstraintSettings_GetPoint1(IntPtr handle, out Double3 result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_HingeConstraintSettings_SetPoint1(IntPtr handle, in Double3 value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_HingeConstraintSettings_GetPoint2(IntPtr handle, out Double3 result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_HingeConstraintSettings_SetPoint2(IntPtr handle, in Double3 value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_HingeConstraintSettings_CreateConstraint(nint handle, nint body1, nint body2);
    #endregion

    #region JPH_SliderConstraint
    [LibraryImport(LibName)]
    public static partial nint JPH_SliderConstraintSettings_Create();

    [LibraryImport(LibName)]
    public static partial void JPH_SliderConstraintSettings_SetSliderAxis(IntPtr handle, Vector3* value);

    [LibraryImport(LibName)]
    public static partial nint JPH_SliderConstraintSettings_CreateConstraint(nint handle, nint body1, nint body2);
    #endregion

    #region JPH_SwingTwistConstraint
    [LibraryImport(LibName)]
    public static partial nint JPH_SwingTwistConstraintSettings_Create();

    [LibraryImport(LibName)]
    public static partial nint JPH_SwingTwistConstraintSettings_CreateConstraint(nint handle, nint body1, nint body2);

    [LibraryImport(LibName)]
    public static partial float JPH_SwingTwistConstraint_GetNormalHalfConeAngle(nint handle);
    #endregion

    #region JPH_SixDOFConstraint
    [LibraryImport(LibName)]
    public static partial nint JPH_SixDOFConstraintSettings_Create();

    [LibraryImport(LibName)]
    public static partial nint JPH_SixDOFConstraintSettings_CreateConstraint(nint handle, nint body1, nint body2);

    [LibraryImport(LibName)]
    public static partial float JPH_SixDOFConstraint_GetLimitsMin(nint handle, uint axis);

    [LibraryImport(LibName)]
    public static partial float JPH_SixDOFConstraint_GetLimitsMax(nint handle, uint axis);
    #endregion

    /* PhysicsSystem */
    [StructLayout(LayoutKind.Sequential)]
    public struct NativePhysicsSystemSettings
    {
        public int maxBodies; /* 10240 */
        public int maxBodyPairs; /* 65536 */
        public int maxContactConstraints; /* 10240 */
        private int _padding;
        public /*BroadPhaseLayerInterfaceTable*/ nint broadPhaseLayerInterface;
        public /*ObjectLayerPairFilterTable**/ nint objectLayerPairFilter;
        public /*ObjectVsBroadPhaseLayerFilterTable* */nint objectVsBroadPhaseLayerFilter;
    }

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPH_PhysicsSystem_Create(NativePhysicsSystemSettings* settings);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PhysicsSystem_Destroy(nint handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PhysicsSystem_OptimizeBroadPhase(nint handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern PhysicsUpdateError JPH_PhysicsSystem_Step(nint handle, float deltaTime, int collisionSteps);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PhysicsSystem_SetContactListener(IntPtr system, IntPtr listener);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_PhysicsSystem_SetBodyActivationListener(IntPtr system, IntPtr listener);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_PhysicsSystem_GetNumBodies(IntPtr system);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_PhysicsSystem_GetNumActiveBodies(IntPtr system, BodyType type);

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
    public static extern IntPtr JPH_PhysicsSystem_GetBodyInterfaceNoLock(IntPtr system);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BodyInterface_CreateBody(IntPtr handle, IntPtr settings);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BodyInterface_CreateSoftBody(IntPtr handle, IntPtr settings);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_BodyInterface_CreateAndAddBody(IntPtr handle, IntPtr bodyID, Activation activation);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BodyInterface_CreateBodyWithID(IntPtr handle, uint bodyID, IntPtr settings);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BodyInterface_CreateBodyWithoutID(IntPtr handle, IntPtr settings);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_DestroyBody(IntPtr handle, uint bodyID);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_DestroyBodyWithoutID(IntPtr handle, IntPtr body);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern Bool32 JPH_BodyInterface_AssignBodyID(IntPtr handle, IntPtr body);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern Bool32 JPH_BodyInterface_AssignBodyID2(IntPtr handle, IntPtr body, uint bodyID);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BodyInterface_UnassignBodyID(IntPtr handle, uint bodyID);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_AddBody(IntPtr handle, uint bodyID, Activation activationMode);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_RemoveBody(IntPtr handle, uint bodyID);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_GetLinearVelocity(IntPtr handle, uint bodyID, out Vector3 velocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetLinearVelocity(IntPtr handle, uint bodyID, in Vector3 velocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_GetCenterOfMassPosition(IntPtr handle, uint bodyID, out Double3 position);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern Bool32 JPH_BodyInterface_IsActive(IntPtr handle, uint bodyID);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern Bool32 JPH_BodyInterface_IsAdded(IntPtr handle, uint bodyID);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern BodyType JPH_BodyInterface_GetBodyType(IntPtr handle, uint bodyID);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern MotionType JPH_BodyInterface_GetMotionType(IntPtr handle, uint bodyID);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetMotionType(IntPtr handle, uint bodyID, MotionType motionType, Activation activationMode);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern MotionQuality JPH_BodyInterface_GetMotionQuality(IntPtr handle, uint bodyID);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetMotionQuality(IntPtr handle, uint bodyID, MotionQuality quality);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_BodyInterface_GetRestitution(IntPtr handle, uint bodyID);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetRestitution(IntPtr handle, uint bodyID, float value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_BodyInterface_GetFriction(IntPtr handle, uint bodyID);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetFriction(IntPtr handle, uint bodyID, float value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetPosition(IntPtr handle, uint bodyId, in Double3 position, Activation activationMode);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_GetPosition(IntPtr handle, uint bodyId, out Double3 position);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetRotation(IntPtr handle, uint bodyId, in Quaternion rotation, Activation activationMode);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_GetRotation(IntPtr handle, uint bodyId, out Quaternion rotation);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetPositionAndRotation(IntPtr handle, uint bodyID, in Double3 position, in Quaternion rotation, Activation activationMode);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetPositionAndRotationWhenChanged(IntPtr handle, uint bodyID, in Double3 position, in Quaternion rotation, Activation activationMode);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetPositionRotationAndVelocity(IntPtr handle, uint bodyID, in Double3 position, in Quaternion rotation, in Vector3 linearVelocity, in Vector3 angularVelocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetShape(IntPtr handle, uint bodyId, IntPtr shape, Bool32 updateMassProperties, Activation activationMode);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_NotifyShapeChanged(IntPtr handle, uint bodyId, in Vector3 previousCenterOfMass, Bool32 updateMassProperties, Activation activationMode);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_ActivateBody(IntPtr handle, uint bodyId);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_DeactivateBody(IntPtr handle, uint bodyId);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetObjectLayer(IntPtr handle, uint bodyId, ushort layer);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ushort JPH_BodyInterface_GetObjectLayer(IntPtr handle, uint bodyId);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_GetWorldTransform(IntPtr handle, uint bodyId, out Matrix4x4 result); // RMatrix4x4

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_GetCenterOfMassTransform(IntPtr handle, uint bodyId, out Matrix4x4 result); // RMatrix4x4

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_MoveKinematic(IntPtr handle, uint bodyId, in Double3 targetPosition, in Quaternion targetRotation, float deltaTime);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetLinearAndAngularVelocity(IntPtr handle, uint bodyId, in Vector3 linearVelocity, in Vector3 angularVelocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_GetLinearAndAngularVelocity(IntPtr handle, uint bodyId, out Vector3 linearVelocity, out Vector3 angularVelocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_AddLinearVelocity(IntPtr handle, uint bodyId, in Vector3 linearVelocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_AddLinearAndAngularVelocity(IntPtr handle, uint bodyId, in Vector3 linearVelocity, in Vector3 angularVelocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetAngularVelocity(IntPtr handle, uint bodyId, in Vector3 angularVelocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_GetAngularVelocity(IntPtr handle, uint bodyId, out Vector3 angularVelocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_GetPointVelocity(IntPtr handle, uint bodyId, in Double3 point, out Vector3 velocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_AddForce(IntPtr handle, uint bodyId, in Vector3 force);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_AddForce2(IntPtr handle, uint bodyId, in Vector3 force, in Double3 point);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_AddTorque(IntPtr handle, uint bodyId, in Vector3 torque);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_AddForceAndTorque(IntPtr handle, uint bodyId, in Vector3 force, in Vector3 torque);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_AddImpulse(IntPtr handle, uint bodyId, in Vector3 impulse);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_AddImpulse2(IntPtr handle, uint bodyId, in Vector3 impulse, in Double3 point);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_AddAngularImpulse(IntPtr handle, uint bodyId, in Vector3 angularImpulse);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_GetInverseInertia(IntPtr handle, uint bodyId, out Matrix4x4 result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetGravityFactor(IntPtr handle, uint bodyId, float gravityFactor);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_BodyInterface_GetGravityFactor(IntPtr handle, uint bodyId);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_InvalidateContactCache(IntPtr handle, uint bodyId);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyInterface_SetUserData(IntPtr handle, uint bodyId, ulong userData);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong JPH_BodyInterface_GetUserData(IntPtr handle, uint bodyId);

    /* BodyLockInterface */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPC_PhysicsSystem_GetBodyLockInterface(nint system);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPC_PhysicsSystem_GetBodyLockInterfaceNoLock(nint system);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyLockInterface_LockRead(nint lockInterface, uint bodyID, out BodyLockRead @lock);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyLockInterface_UnlockRead(nint lockInterface, in BodyLockRead @lock);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyLockInterface_LockWrite(nint lockInterface, uint bodyID, out BodyLockWrite @lock);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyLockInterface_UnlockWrite(nint lockInterface, in BodyLockWrite @lock);

    /* JPH_MotionProperties */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_MotionProperties_SetLinearDamping(nint properties, float damping);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_MotionProperties_GetLinearDamping(nint properties);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_MotionProperties_SetAngularDamping(nint properties, float damping);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_MotionProperties_GetAngularDamping(nint properties);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_MotionProperties_GetInverseMassUnchecked(nint properties);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_MotionProperties_SetMassProperties(nint properties, AllowedDOFs allowedDOFs, in MassProperties massProperties);

    /* BodyLockInterface */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPC_PhysicsSystem_GetNarrowPhaseQuery(nint system);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern nint JPC_PhysicsSystem_GetNarrowPhaseQueryNoLock(nint system);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern Bool32 JPH_NarrowPhaseQuery_CastRay(nint system,
        in Double3 origin, in Vector3 direction,
        ref RayCastResult hit,
        IntPtr broadPhaseLayerFilter,
        IntPtr objectLayerFilter,
        IntPtr bodyFilter);

    /* Body */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_Body_GetID(IntPtr body);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern BodyType JPH_Body_GetBodyType(IntPtr body);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_GetWorldSpaceBounds(IntPtr body, BoundingBox* box);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern Bool32 JPH_Body_IsActive(IntPtr handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern Bool32 JPH_Body_IsStatic(IntPtr handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern Bool32 JPH_Body_IsKinematic(IntPtr handle);

    [LibraryImport(LibName)]
    public static partial Bool32 JPH_Body_IsDynamic(IntPtr handle);

    [LibraryImport(LibName)]
    public static partial Bool32 JPH_Body_IsSensor(IntPtr handle);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetIsSensor(nint handle, Bool32 value);

    [LibraryImport(LibName)]
    public static partial Bool32 JPH_Body_SensorDetectsStatic(IntPtr handle);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetSensorDetectsStatic(nint handle, Bool32 value);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetUseManifoldReduction(nint handle, Bool32 value);

    [LibraryImport(LibName)]
    public static partial Bool32 JPH_Body_GetUseManifoldReduction(nint handle);

    [LibraryImport(LibName)]
    public static partial Bool32 JPH_Body_GetUseManifoldReductionWithBody(nint handle, nint other);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetApplyGyroscopicForce(nint handle, Bool32 value);

    [LibraryImport(LibName)]
    public static partial Bool32 JPH_Body_GetApplyGyroscopicForce(nint handle);

    [LibraryImport(LibName)]
    public static partial nint JPH_Body_GetMotionProperties(nint handle);

    [LibraryImport(LibName)]
    public static partial MotionType JPH_Body_GetMotionType(nint handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_SetMotionType(nint handle, MotionType motionType);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern Bool32 JPH_Body_GetAllowSleeping(nint handle);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_SetAllowSleeping(nint handle, Bool32 motionType);

    [LibraryImport(LibName)]
    public static partial void JPH_Body_ResetSleepTimer(nint handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_Body_GetFriction(IntPtr handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_SetFriction(IntPtr handle, float value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float JPH_Body_GetRestitution(IntPtr handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_SetRestitution(IntPtr handle, float value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_GetPosition(IntPtr handle, out Double3 result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_GetRotation(IntPtr handle, out Quaternion result);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_GetCenterOfMassPosition(IntPtr handle, out Double3 result);

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
    public static extern void JPH_Body_AddForceAtPosition(IntPtr handle, in Vector3 velocity, in Double3 position);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_AddTorque(IntPtr handle, in Vector3 value);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_GetAccumulatedForce(IntPtr handle, out Vector3 force);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_GetAccumulatedTorque(IntPtr handle, out Vector3 force);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_AddImpulse(IntPtr handle, in Vector3 impulse);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_AddImpulseAtPosition(IntPtr handle, in Vector3 impulse, in Double3 position);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_AddAngularImpulse(IntPtr handle, in Vector3 angularImpulse);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_Body_SetUserData(IntPtr handle, ulong userData);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong JPH_Body_GetUserData(IntPtr handle);

    // ContactListener
    public struct JPH_ContactListener_Procs
    {
        public delegate* unmanaged<IntPtr, IntPtr, IntPtr, Double3*, IntPtr, uint> OnContactValidate;
        public delegate* unmanaged<IntPtr, IntPtr, IntPtr, void> OnContactAdded;
        public delegate* unmanaged<IntPtr, IntPtr, IntPtr, void> OnContactPersisted;
        public delegate* unmanaged<IntPtr, SubShapeIDPair*, void> OnContactRemoved;
    }
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_ContactListener_SetProcs(JPH_ContactListener_Procs procs);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_ContactListener_Create();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_ContactListener_Destroy(IntPtr handle);

    // BodyActivationListener
    public struct JPH_BodyActivationListener_Procs
    {
        public delegate* unmanaged<IntPtr, uint, ulong, void> OnBodyActivated;
        public delegate* unmanaged<IntPtr, uint, ulong, void> OnBodyDeactivated;
    }

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyActivationListener_SetProcs(JPH_BodyActivationListener_Procs procs);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_BodyActivationListener_Create();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_BodyActivationListener_Destroy(IntPtr handle);

    /* CharacterBaseSettings */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_CharacterBaseSettings_Destroy(IntPtr handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_CharacterBaseSettings_SetSupportingVolume(IntPtr handle, in Vector3 normal, float costant);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_CharacterBaseSettings_SetMaxSlopeAngle(IntPtr handle, float maxSlopeAngle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_CharacterBaseSettings_SetShape(IntPtr handle, IntPtr shape);

    /* CharacterBase */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_CharacterBase_Destroy(IntPtr handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern GroundState JPH_CharacterBase_GetGroundState(IntPtr handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool JPH_CharacterBase_IsSupported(IntPtr handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_CharacterBase_GetGroundPosition(IntPtr handle, out Double3 position);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_CharacterBase_GetGroundNormal(IntPtr handle, out Vector3 normal);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_CharacterBase_GetGroundVelocity(IntPtr handle, out Vector3 velocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_CharacterBase_GetGroundBodyId(IntPtr handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint JPH_CharacterBase_GetGroundSubShapeId(IntPtr handle);

    /* CharacterVirtualSettings */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_CharacterVirtualSettings_Create();

    /* CharacterVirtual */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_CharacterVirtual_Create(IntPtr settings, in Double3 position, in Quaternion rotation, IntPtr physicsSystem);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_CharacterVirtual_GetLinearVelocity(IntPtr handle, out Vector3 velocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_CharacterVirtual_SetLinearVelocity(IntPtr handle, in Vector3 velocity);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_CharacterVirtual_GetPosition(IntPtr handle, out Double3 position);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_CharacterVirtual_SetPosition(IntPtr handle, in Double3 position);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_CharacterVirtual_GetRotation(IntPtr handle, out Quaternion rotation);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_CharacterVirtual_SetRotation(IntPtr handle, in Quaternion rotation);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_CharacterVirtual_ExtenedUpdate(IntPtr handle, float deltaTime,
        IntPtr settings, ushort layer, IntPtr physicsSytem);

    /* ExtendedUpdateSettings */
    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr JPH_ExtendedUpdateSettings_Create();

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_ExtendedUpdateSettings_Destroy(IntPtr handle);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_ExtendedUpdateSettings_SetStickToFloorStepDown(IntPtr handle, in Vector3 stickToFloorStepDown);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void JPH_ExtendedUpdateSettings_SetWalkStairsStepUp(IntPtr handle, in Vector3 walkStairsStepUp);
}
