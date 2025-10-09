// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

using unsafe JPH_CastRayCollector = delegate* unmanaged<nint, RayCastResult*, float>;
using unsafe JPH_CollidePointCollector = delegate* unmanaged<nint, CollidePointResult*, float>;
using unsafe JPH_CollideShapeCollector = delegate* unmanaged<nint, CollideShapeResult*, float>;
using unsafe JPH_CastShapeCollector = delegate* unmanaged<nint, ShapeCastResult*, float>;

public readonly unsafe struct NarrowPhaseQuery : IEquatable<NarrowPhaseQuery>
{
    public NarrowPhaseQuery(nint handle) { Handle = handle; }
    public nint Handle { get; }
    public bool IsNull => Handle == 0;
    public static NarrowPhaseQuery Null => new(0);
    public static implicit operator NarrowPhaseQuery(nint handle) => new(handle);
    public static bool operator ==(NarrowPhaseQuery left, NarrowPhaseQuery right) => left.Handle == right.Handle;
    public static bool operator !=(NarrowPhaseQuery left, NarrowPhaseQuery right) => left.Handle != right.Handle;
    public static bool operator ==(NarrowPhaseQuery left, nint right) => left.Handle == right;
    public static bool operator !=(NarrowPhaseQuery left, nint right) => left.Handle != right;
    public bool Equals(NarrowPhaseQuery other) => Handle == other.Handle;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is NarrowPhaseQuery handle && Equals(handle);

    /// <inheritdoc/>
    public override int GetHashCode() => Handle.GetHashCode();

    #region CastRay
    public bool CastRay(
        in Ray ray,
        out RayCastResult hit,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(CastRay)}");

        return JPH_NarrowPhaseQuery_CastRay(Handle, in ray.Position, in ray.Direction, out hit,
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0);
    }

    public bool CastRay(
        in Ray ray,
        JPH_CastRayCollector callback,
        nint userData = 0,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(CastRay)}");

        return JPH_NarrowPhaseQuery_CastRay2(Handle, in ray.Position, in ray.Direction,
            null,
            callback, userData,
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0);
    }

    public bool CastRay(
        in Ray ray,
        RayCastSettings settings,
        JPH_CastRayCollector callback,
        nint userData = 0,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(CastRay)}");

        return JPH_NarrowPhaseQuery_CastRay2(Handle,
            in ray.Position, in ray.Direction,
            &settings,
            callback, userData,
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0);
    }

    public bool CastRay(
        in Ray ray,
        RayCastSettings settings,
        CollisionCollectorType collectorType,
        ICollection<RayCastResult> results,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(CastRay)}");

        GCHandle callbackHandle = GCHandle.Alloc(results);
        bool callbackResult = JPH_NarrowPhaseQuery_CastRay3(Handle, in ray.Position, in ray.Direction,
            &settings,
            collectorType, &OnCastRayResultCallback, GCHandle.ToIntPtr(callbackHandle),
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0);
        callbackHandle.Free();
        return callbackResult;
    }

    public bool CastRay(
        in RVector3 origin,
        in Vector3 direction,
        out RayCastResult hit,
        BroadPhaseLayerFilter broadPhaseFilter,
        ObjectLayerFilter objectLayerFilter,
        BodyFilter bodyFilter)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(CastRay)}");

        return JPH_NarrowPhaseQuery_CastRay(Handle, in origin, in direction, out hit, broadPhaseFilter.Handle, objectLayerFilter.Handle, bodyFilter.Handle);
    }

    public bool CastRay(
        in RVector3 origin,
        in Vector3 direction,
        RayCastSettings settings,
        JPH_CastRayCollector callback,
        nint userData = 0,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(CastRay)}");

        return JPH_NarrowPhaseQuery_CastRay2(Handle, in origin, in direction,
            &settings,
            callback, userData,
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0);
    }

    public bool CastRay(
        in RVector3 origin,
        in Vector3 direction,
        JPH_CastRayCollector callback,
        nint userData = 0,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(CastRay)}");

        return JPH_NarrowPhaseQuery_CastRay2(Handle, in origin, in direction,
            null,
            callback, userData,
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0);
    }

    public bool CastRay(
        in RVector3 origin,
        in Vector3 direction,
        RayCastSettings settings,
        CollisionCollectorType collectorType,
        ICollection<RayCastResult> results,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(CastRay)}");

        GCHandle callbackHandle = GCHandle.Alloc(results);
        bool callbackResult = JPH_NarrowPhaseQuery_CastRay3(Handle, in origin, in direction,
            &settings,
            collectorType, &OnCastRayResultCallback,
            GCHandle.ToIntPtr(callbackHandle),
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0,
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
    #endregion

    #region CollidePoint
    public bool CollidePoint(
        in Vector3 point,
        JPH_CollidePointCollector callback,
        nint userData = 0,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(CollidePoint)}");

        return JPH_NarrowPhaseQuery_CollidePoint(Handle,
            in point,
            callback, userData,
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0);
    }

    public bool CollidePoint(
        in Vector3 point,
        CollisionCollectorType collectorType,
        ICollection<CollidePointResult> results,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(CollidePoint)}");

        GCHandle callbackHandle = GCHandle.Alloc(results);
        bool callbackResult = JPH_NarrowPhaseQuery_CollidePoint2(Handle,
            in point,
            collectorType,
            &OnCollidePointCallback,
            GCHandle.ToIntPtr(callbackHandle),
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0);
        callbackHandle.Free();
        return callbackResult;
    }

    public bool CollidePoint(
        in RVector3 point,
        JPH_CollidePointCollector callback,
        nint userData = 0,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(CollidePoint)}");

        return JPH_NarrowPhaseQuery_CollidePoint(Handle,
            in point,
            callback, userData,
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0);
    }

    public bool CollidePoint(
        in RVector3 point,
        CollisionCollectorType collectorType,
        ICollection<CollidePointResult> results,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(CollidePoint)}");

        GCHandle callbackHandle = GCHandle.Alloc(results);
        bool callbackResult = JPH_NarrowPhaseQuery_CollidePoint2(Handle,
            in point,
            collectorType,
            &OnCollidePointCallback,
            GCHandle.ToIntPtr(callbackHandle),
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0);
        callbackHandle.Free();
        return callbackResult;
    }

    [UnmanagedCallersOnly]
    private static unsafe void OnCollidePointCallback(nint userData, CollidePointResult* result)
    {
        ICollection<CollidePointResult> collection = (ICollection<CollidePointResult>)GCHandle.FromIntPtr(userData).Target!;
        collection.Add(*result);
    }
    #endregion

    #region CollideShape
    public bool CollideShape(Shape shape,
        in Vector3 scale, in Matrix4x4 centerOfMassTransform,
        in Vector3 baseOffset,
        JPH_CollideShapeCollector callback,
        nint userData = 0,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(CollideShape)}");

        bool callbackResult = JPH_NarrowPhaseQuery_CollideShape(Handle, shape.Handle,
            in scale, centerOfMassTransform.ToJolt(),
            null,
            in baseOffset,
            callback, userData,
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0);
        return callbackResult;
    }

    public bool CollideShape(Shape shape,
        in Vector3 scale, in Matrix4x4 centerOfMassTransform,
        in CollideShapeSettings settings,
        in Vector3 baseOffset,
        JPH_CollideShapeCollector callback,
        nint userData = 0,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(CollideShape)}");

        JPH_CollideShapeSettings nativeSettings;
        settings.ToNative(&nativeSettings);
        return JPH_NarrowPhaseQuery_CollideShape(Handle, shape.Handle,
            in scale, centerOfMassTransform.ToJolt(),
            &nativeSettings,
            in baseOffset,
            callback, userData,
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0);
    }

    public bool CollideShape(Shape shape,
        in Vector3 scale, in Matrix4x4 centerOfMassTransform,
        in Vector3 baseOffset,
        CollisionCollectorType collectorType,
        ICollection<CollideShapeResult> results,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(CollideShape)}");

        GCHandle callbackHandle = GCHandle.Alloc(results);
        bool callbackResult = JPH_NarrowPhaseQuery_CollideShape2(Handle, shape.Handle,
            in scale, centerOfMassTransform.ToJolt(),
            null,
            in baseOffset,
            collectorType,
            &OnCollideShapeCallback,
            GCHandle.ToIntPtr(callbackHandle),
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0);
        callbackHandle.Free();
        return callbackResult;
    }

    public bool CollideShape(Shape shape,
        in Vector3 scale, in Matrix4x4 centerOfMassTransform,
        in CollideShapeSettings settings,
        in Vector3 baseOffset,
        CollisionCollectorType collectorType,
        ICollection<CollideShapeResult> results,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(CollideShape)}");

        JPH_CollideShapeSettings nativeSettings;
        settings.ToNative(&nativeSettings);

        GCHandle callbackHandle = GCHandle.Alloc(results);
        bool callbackResult = JPH_NarrowPhaseQuery_CollideShape2(Handle, shape.Handle,
            in scale, centerOfMassTransform.ToJolt(),
            &nativeSettings,
            in baseOffset,
            collectorType,
            &OnCollideShapeCallback,
            GCHandle.ToIntPtr(callbackHandle),
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0);
        callbackHandle.Free();
        return callbackResult;
    }

    public bool CollideShape(Shape shape,
        in Vector3 scale, in RMatrix4x4 centerOfMassTransform,
        in RVector3 baseOffset,
        JPH_CollideShapeCollector callback,
        nint userData = 0,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(CollideShape)}");

        return JPH_NarrowPhaseQuery_CollideShape(Handle, shape.Handle,
            in scale, in centerOfMassTransform,
            null,
            in baseOffset,
            callback, userData,
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0
            );
    }

    public bool CollideShape(Shape shape,
        in Vector3 scale, in RMatrix4x4 centerOfMassTransform,
        in CollideShapeSettings settings,
        in RVector3 baseOffset,
        JPH_CollideShapeCollector callback,
        nint userData = 0,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(CollideShape)}");

        JPH_CollideShapeSettings nativeSettings;
        settings.ToNative(&nativeSettings);
        return JPH_NarrowPhaseQuery_CollideShape(Handle, shape.Handle,
            in scale, in centerOfMassTransform,
            &nativeSettings,
            in baseOffset,
            callback, userData,
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0);
    }

    public bool CollideShape(Shape shape,
        in Vector3 scale, in RMatrix4x4 centerOfMassTransform,
        in RVector3 baseOffset,
        CollisionCollectorType collectorType,
        ICollection<CollideShapeResult> results,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(CollideShape)}");

        GCHandle callbackHandle = GCHandle.Alloc(results);
        bool callbackResult = JPH_NarrowPhaseQuery_CollideShape2(Handle, shape.Handle,
            in scale, in centerOfMassTransform,
            null,
            in baseOffset,
            collectorType,
            &OnCollideShapeCallback,
            GCHandle.ToIntPtr(callbackHandle),
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0);
        callbackHandle.Free();
        return callbackResult;
    }

    public bool CollideShape(Shape shape,
        in Vector3 scale, in RMatrix4x4 centerOfMassTransform,
        in CollideShapeSettings settings,
        in RVector3 baseOffset,
        CollisionCollectorType collectorType,
        ICollection<CollideShapeResult> results,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(CollideShape)}");

        JPH_CollideShapeSettings nativeSettings;
        settings.ToNative(&nativeSettings);

        GCHandle callbackHandle = GCHandle.Alloc(results);
        bool callbackResult = JPH_NarrowPhaseQuery_CollideShape2(Handle, shape.Handle,
            in scale, in centerOfMassTransform,
            &nativeSettings,
            in baseOffset,
            collectorType,
            &OnCollideShapeCallback,
            GCHandle.ToIntPtr(callbackHandle),
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0);
        callbackHandle.Free();
        return callbackResult;
    }

    [UnmanagedCallersOnly]
    private static unsafe void OnCollideShapeCallback(nint userData, CollideShapeResult* result)
    {
        ICollection<CollideShapeResult> collection = (ICollection<CollideShapeResult>)GCHandle.FromIntPtr(userData).Target!;
        collection.Add(*result);
    }
    #endregion

    #region CastShape
    public bool CastShape(Shape shape,
        in Matrix4x4 centerOfMassTransform,
        in Vector3 direction,
        in Vector3 baseOffset,
        JPH_CastShapeCollector callback,
        nint userData = 0,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(CastShape)}");

        return JPH_NarrowPhaseQuery_CastShape(Handle, shape.Handle,
            centerOfMassTransform.ToJolt(), in direction,
            null,
            in baseOffset,
            callback, userData,
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0);
    }

    public bool CastShape(Shape shape,
        in Matrix4x4 centerOfMassTransform, in Vector3 direction,
        ShapeCastSettings settings,
        in Vector3 baseOffset,
        JPH_CastShapeCollector callback,
        nint userData = 0,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(CastShape)}");

        JPH_ShapeCastSettings nativeSettings;
        settings.ToNative(&nativeSettings);

        return JPH_NarrowPhaseQuery_CastShape(Handle, shape.Handle,
            centerOfMassTransform.ToJolt(), in direction,
            &nativeSettings,
            in baseOffset,
            callback, userData,
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0);
    }

    public bool CastShape(Shape shape,
        in Matrix4x4 centerOfMassTransform, in Vector3 direction,
        in Vector3 baseOffset,
        CollisionCollectorType collectorType,
        ICollection<ShapeCastResult> results,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(CastShape)}");

        GCHandle callbackHandle = GCHandle.Alloc(results);
        bool callbackResult = JPH_NarrowPhaseQuery_CastShape2(Handle, shape.Handle,
            centerOfMassTransform.ToJolt(), in direction,
            null,
            in baseOffset,
            collectorType,
            &OnCastShapeResultCallback,
            GCHandle.ToIntPtr(callbackHandle),
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0);
        callbackHandle.Free();
        return callbackResult;
    }

    public bool CastShape(Shape shape,
        in Matrix4x4 centerOfMassTransform, in Vector3 direction,
        ShapeCastSettings castSettings,
        in Vector3 baseOffset,
        CollisionCollectorType collectorType,
        ICollection<ShapeCastResult> results,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(CastShape)}");

        JPH_ShapeCastSettings nativeSettings;
        castSettings.ToNative(&nativeSettings);

        GCHandle callbackHandle = GCHandle.Alloc(results);
        bool callbackResult = JPH_NarrowPhaseQuery_CastShape2(Handle, shape.Handle,
            centerOfMassTransform.ToJolt(), in direction,
            &nativeSettings,
            in baseOffset,
            collectorType,
            &OnCastShapeResultCallback,
            GCHandle.ToIntPtr(callbackHandle),
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0);
        callbackHandle.Free();
        return callbackResult;
    }

    public bool CastShape(Shape shape,
        in RMatrix4x4 centerOfMassTransform,
        in Vector3 direction,
        in RVector3 baseOffset,
        JPH_CastShapeCollector callback,
        nint userData = 0,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(CastShape)}");

        return JPH_NarrowPhaseQuery_CastShape(Handle, shape.Handle,
            in centerOfMassTransform, in direction,
            null,
            in baseOffset,
            callback, userData,
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0);
    }

    public bool CastShape(Shape shape,
        in RMatrix4x4 centerOfMassTransform,
        in Vector3 direction,
        ShapeCastSettings settings,
        in RVector3 baseOffset,
        JPH_CastShapeCollector callback,
        nint userData = 0,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(CastShape)}");

        JPH_ShapeCastSettings nativeSettings;
        settings.ToNative(&nativeSettings);

        return JPH_NarrowPhaseQuery_CastShape(Handle, shape.Handle,
            in centerOfMassTransform, in direction,
            &nativeSettings,
            in baseOffset,
            callback, userData,
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0);
    }

    public bool CastShape(Shape shape,
        in RMatrix4x4 centerOfMassTransform, in Vector3 direction,
        in RVector3 baseOffset,
        CollisionCollectorType collectorType,
        ICollection<ShapeCastResult> results,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(CastShape)}");

        GCHandle callbackHandle = GCHandle.Alloc(results);
        bool callbackResult = JPH_NarrowPhaseQuery_CastShape2(Handle, shape.Handle,
            in centerOfMassTransform, in direction,
            null,
            in baseOffset,
            collectorType,
            &OnCastShapeResultCallback,
            GCHandle.ToIntPtr(callbackHandle),
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0);
        callbackHandle.Free();
        return callbackResult;
    }

    public bool CastShape(Shape shape,
        in RMatrix4x4 centerOfMassTransform, in Vector3 direction,
        ShapeCastSettings castSettings,
        in RVector3 baseOffset,
        CollisionCollectorType collectorType,
        ICollection<ShapeCastResult> results,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default,
        ShapeFilter? shapeFilter = default)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(CastShape)}");

        JPH_ShapeCastSettings nativeSettings;
        castSettings.ToNative(&nativeSettings);

        GCHandle callbackHandle = GCHandle.Alloc(results);
        bool callbackResult = JPH_NarrowPhaseQuery_CastShape2(Handle, shape.Handle,
            in centerOfMassTransform, in direction,
            &nativeSettings,
            in baseOffset,
            collectorType,
            &OnCastShapeResultCallback,
            GCHandle.ToIntPtr(callbackHandle),
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0,
            shapeFilter?.Handle ?? 0);
        callbackHandle.Free();
        return callbackResult;
    }

    [UnmanagedCallersOnly]
    private static void OnCastShapeResultCallback(nint userData, ShapeCastResult* result)
    {
        ICollection<ShapeCastResult> collection = (ICollection<ShapeCastResult>)GCHandle.FromIntPtr(userData).Target!;
        collection.Add(*result);
    }
    #endregion
}
