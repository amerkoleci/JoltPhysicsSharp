// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
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

    protected override void DisposeNative()
    {
        JPH_ShapeSettings_Destroy(Handle);
    }

    public ulong UserData
    {
        get => JPH_ShapeSettings_GetUserData(Handle);
        set => JPH_ShapeSettings_SetUserData(Handle, value);
    }

    public abstract Shape Create();
}


public class Shape : NativeObject
{
    protected Shape()
    {
    }

    internal Shape(nint handle, bool owns = true)
        : base(handle, owns)
    {
    }

    protected override void DisposeNative()
    {
        JPH_Shape_Destroy(Handle);
    }

    public ShapeType Type => JPH_Shape_GetType(Handle);
    public ShapeSubType SubType => JPH_Shape_GetSubType(Handle);
    public ulong UserData
    {
        get => JPH_Shape_GetUserData(Handle);
        set => JPH_Shape_SetUserData(Handle, value);
    }

    public uint SubShapeIDBitsRecursive => JPH_Shape_GetSubShapeIDBitsRecursive(Handle);

    public MassProperties MassProperties
    {
        get
        {
            JPH_Shape_GetMassProperties(Handle, out MassProperties properties);
            return properties;
        }
    }

    public float InnerRadius => JPH_Shape_GetInnerRadius(Handle);
    public float Volume => JPH_Shape_GetVolume(Handle);

    public Vector3 CenterOfMass
    {
        get
        {
            JPH_Shape_GetCenterOfMass(Handle, out Vector3 result);
            return result;
        }
    }

    public BoundingBox LocalBounds
    {
        get
        {
            JPH_Shape_GetLocalBounds(Handle, out BoundingBox result);
            return result;
        }
    }

    public void GetCenterOfMass(out Vector3 result)
    {
        JPH_Shape_GetCenterOfMass(Handle, out result);
    }

    public void GetLocalBounds(out BoundingBox result)
    {
        JPH_Shape_GetLocalBounds(Handle, out result);
    }

    public void GetMassProperties(out MassProperties properties)
    {
        JPH_Shape_GetMassProperties(Handle, out properties);
    }

    public BoundingBox GetWorldSpaceBounds(in Matrix4x4 centerOfMassTransform, in Vector3 scale)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRWorldSpaceBounds)}");

        JPH_Shape_GetWorldSpaceBounds(Handle, centerOfMassTransform.ToJolt(), in scale, out BoundingBox result);
        return result;
    }

    public void GetWorldSpaceBounds(in Matrix4x4 centerOfMassTransform, in Vector3 scale, out BoundingBox result)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(GetRWorldSpaceBounds)}");

        JPH_Shape_GetWorldSpaceBounds(Handle, centerOfMassTransform.ToJolt(), in scale, out result);
    }

    public BoundingBox GetRWorldSpaceBounds(in RMatrix4x4 centerOfMassTransform, in Vector3 scale)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetWorldSpaceBounds)}");

        JPH_Shape_GetWorldSpaceBounds(Handle, in centerOfMassTransform, in scale, out BoundingBox result);
        return result;
    }

    public void GetRWorldSpaceBounds(in RMatrix4x4 centerOfMassTransform, in Vector3 scale, out BoundingBox result)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(GetWorldSpaceBounds)}");

        JPH_Shape_GetWorldSpaceBounds(Handle, in centerOfMassTransform, in scale, out result);
    }

    public PhysicsMaterial? GetMaterial(SubShapeID subShapeID) => PhysicsMaterial.GetObject(JPH_Shape_GetMaterial(Handle, subShapeID));

    public Vector3 GetSurfaceNormal(SubShapeID subShapeID, in Vector3 localPosition)
    {
        JPH_Shape_GetSurfaceNormal(Handle, subShapeID, in localPosition, out Vector3 normal);
        return normal;
    }

    public void GetSurfaceNormal(SubShapeID subShapeID, in Vector3 localPosition, out Vector3 normal)
    {
        JPH_Shape_GetSurfaceNormal(Handle, subShapeID, in localPosition, out normal);
    }

    public bool IsValidScale(in Vector3 scale) => JPH_Shape_IsValidScale(Handle, in scale);

    public Vector3 MakeScaleValid(in Vector3 scale)
    {
        JPH_Shape_MakeScaleValid(Handle, in scale, out Vector3 result);
        return result;
    }

    public void MakeScaleValid(in Vector3 scale, out Vector3 result) => JPH_Shape_MakeScaleValid(Handle, in scale, out result);

    public Shape? ScaleShape(in Vector3 scale)
    {
        nint shape = JPH_Shape_ScaleShape(Handle, in scale);
        if (shape == 0)
            return default;

        return new(shape);
    }

    public bool CastRay(in Ray ray, out RayCastResult hit)
    {
        return JPH_Shape_CastRay(Handle, in ray.Position, in ray.Direction, out hit);
    }

    public bool CastRay(in Ray ray, CollisionCollectorType collectorType, ICollection<RayCastResult> result)
    {
        return CastRay(in ray, new RayCastSettings(), collectorType, result);
    }

    public unsafe bool CastRay(in Ray ray, in RayCastSettings rayCastSettings, CollisionCollectorType collectorType, ICollection<RayCastResult> result, ShapeFilter? shapeFilter = default)
    {
        GCHandle callbackHandle = GCHandle.Alloc(result);
        bool callbackResult = JPH_Shape_CastRay2(Handle, in ray.Position, in ray.Direction,
            in rayCastSettings,
            collectorType,
            &OnCastRayResultCallback,
            GCHandle.ToIntPtr(callbackHandle),
            shapeFilter?.Handle ?? 0);
        callbackHandle.Free();
        return callbackResult;
    }

    public bool CollidePoint(in Vector3 point, ShapeFilter? shapeFilter = default)
    {
        return JPH_Shape_CollidePoint(Handle, in point, shapeFilter?.Handle ?? 0);
    }

    public unsafe bool CollidePoint(in Vector3 point, CollisionCollectorType collectorType, ICollection<CollidePointResult> result, ShapeFilter? shapeFilter = default)
    {
        GCHandle callbackHandle = GCHandle.Alloc(result);
        bool callbackResult = JPH_Shape_CollidePoint2(Handle, in point,
            collectorType,
            &OnCollidePointCallback,
            GCHandle.ToIntPtr(callbackHandle),
            shapeFilter?.Handle ?? 0);
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

    internal static Shape? GetObject(nint handle)
    {
        return GetOrAddObject(handle, (nint h) => new Shape(h, false));
    }
}
