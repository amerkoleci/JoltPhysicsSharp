// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class ShapeSettings : NativeObject
{
    protected ShapeSettings()
    {
    }

    protected ShapeSettings(nint handle)
        : base(handle)
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="ShapeSettings" /> class.
    /// </summary>
    ~ShapeSettings() => Dispose(disposing: false);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            JPH_ShapeSettings_Destroy(Handle);
        }
    }

    public ulong UserData
    {
        get => JPH_ShapeSettings_GetUserData(Handle);
        set => JPH_ShapeSettings_SetUserData(Handle, value);
    }

    public abstract Shape Create();
}


public abstract unsafe class Shape : NativeObject
{
    protected Shape()
    {
    }

    protected Shape(nint handle)
        : base(handle)
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="Shape" /> class.
    /// </summary>
    ~Shape() => Dispose(disposing: false);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            JPH_Shape_Destroy(Handle);
        }
    }

    public ShapeType Type => JPH_Shape_GetType(Handle);
    public ShapeSubType SubType => JPH_Shape_GetSubType(Handle);
    public ulong UserData
    {
        get => JPH_Shape_GetUserData(Handle);
        set => JPH_Shape_SetUserData(Handle, value);
    }


    public BoundingBox LocalBounds
    {
        get
        {
            BoundingBox result = default;
            JPH_Shape_GetLocalBounds(Handle, &result);
            return result;
        }
    }

    public uint SubShapeIDBitsRecursive => JPH_Shape_GetSubShapeIDBitsRecursive(Handle);

    public MassProperties MassProperties
    {
        get
        {
            MassProperties properties;
            JPH_Shape_GetMassProperties(Handle, &properties);
            return properties;
        }
    }

    public float InnerRadius => JPH_Shape_GetInnerRadius(Handle);
    public float Volume => JPH_Shape_GetVolume(Handle);

    public Vector3 CenterOfMass
    {
        get
        {
            Vector3 result;
            JPH_Shape_GetCenterOfMass(Handle, &result);
            return result;
        }
    }

    public void GetCenterOfMass(out Vector3 result)
    {
        Unsafe.SkipInit(out result);

        fixed (Vector3* resultPtr = &result)
        {
            JPH_Shape_GetCenterOfMass(Handle, resultPtr);
        }
    }

    public void GetMassProperties(out MassProperties properties)
    {
        Unsafe.SkipInit(out properties);

        fixed (MassProperties* resultPtr = &properties)
        {
            JPH_Shape_GetMassProperties(Handle, resultPtr);
        }
    }


    public void GetWorldSpaceBounds(in Matrix4x4 centerOfMassTransform, in Vector3 scale, out BoundingBox result)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRWorldSpaceBounds)}");

        Unsafe.SkipInit(out result);

        fixed (Matrix4x4* centerOfMassTransformPtr = &centerOfMassTransform)
        fixed (Vector3* scalePtr = &scale)
        fixed (BoundingBox* resultPtr = &result)
        {
            JPH_Shape_GetWorldSpaceBounds(Handle, centerOfMassTransformPtr, scalePtr, resultPtr);
        }
    }

    public void GetRWorldSpaceBounds(in RMatrix4x4 centerOfMassTransform, in Vector3 scale, out BoundingBox result)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetWorldSpaceBounds)}");

        Unsafe.SkipInit(out result);

        fixed (RMatrix4x4* centerOfMassTransformPtr = &centerOfMassTransform)
        fixed (Vector3* scalePtr = &scale)
        fixed (BoundingBox* resultPtr = &result)
        {
            JPH_Shape_GetWorldSpaceBoundsDouble(Handle, centerOfMassTransformPtr, scalePtr, resultPtr);
        }
    }

    public nint GetMaterial(SubShapeID subShapeID) => JPH_Shape_GetMaterial(Handle, subShapeID);

    public Vector3 GetSurfaceNormal(SubShapeID subShapeID, in Vector3 localPosition)
    {
        fixed (Vector3* localPositionPtr = &localPosition)
        {
            Vector3 normal;
            JPH_Shape_GetSurfaceNormal(Handle, subShapeID, localPositionPtr, &normal);
            return normal;
        }
    }

    public void GetSurfaceNormal(SubShapeID subShapeID, in Vector3 localPosition, out Vector3 normal)
    {
        Unsafe.SkipInit(out normal);

        fixed (Vector3* localPositionPtr = &localPosition)
        fixed (Vector3* normalPtr = &normal)
        {
            JPH_Shape_GetSurfaceNormal(Handle, subShapeID, localPositionPtr, normalPtr);
        }
    }

    public bool CastRay(in Vector3 origin, in Vector3 direction, out RayCastResult hit)
    {
        return JPH_Shape_CastRay(Handle, in origin, in direction, out hit);
    }

    public bool CastRay(in Vector3 origin, in Vector3 direction, CollisionCollectorType collectorType, ICollection<RayCastResult> result)
    {
        return CastRay(in origin, in direction, new RayCastSettings(), collectorType, result);
    }

    public bool CastRay(in Vector3 origin, in Vector3 direction, in RayCastSettings rayCastSettings, CollisionCollectorType collectorType, ICollection<RayCastResult> result)
    {
        GCHandle callbackHandle = GCHandle.Alloc(result);
        bool callbackResult = JPH_Shape_CastRay2(Handle, in origin, in direction, in rayCastSettings, collectorType, &OnCastRayResultCallback, GCHandle.ToIntPtr(callbackHandle));
        callbackHandle.Free();
        return callbackResult;
    }

    public bool CollidePoint(in Vector3 point)
    {
        return JPH_Shape_CollidePoint(Handle, in point);
    }

    public bool CollidePoint(in Vector3 point, CollisionCollectorType collectorType, ICollection<CollidePointResult> result)
    {
        GCHandle callbackHandle = GCHandle.Alloc(result);
        bool callbackResult = JPH_Shape_CollidePoint2(Handle, in point, collectorType, &OnCollidePointCallback, GCHandle.ToIntPtr(callbackHandle));
        callbackHandle.Free();
        return callbackResult;
    }

    [UnmanagedCallersOnly]
    private static unsafe void OnCastRayResultCallback(nint userData, RayCastResult* result)
    {
        ICollection<RayCastResult> collection = (ICollection<RayCastResult>)GCHandle.FromIntPtr(userData).Target!;
        collection.Add(*result);
    }

    [UnmanagedCallersOnly]
    private static unsafe void OnCollidePointCallback(nint userData, CollidePointResult* result)
    {
        ICollection<CollidePointResult> collection = (ICollection<CollidePointResult>)GCHandle.FromIntPtr(userData).Target!;
        collection.Add(*result);
    }
}
