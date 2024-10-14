// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

using unsafe JPH_CastRayCollector = delegate* unmanaged<nint, RayCastResult*, float>;
using unsafe JPH_CollidePointCollector = delegate* unmanaged<nint, CollidePointResult*, float>;
using unsafe JPH_CollideShapeCollector = delegate* unmanaged<nint, CollidePointResult*, float>;
using unsafe JPH_CastShapeCollector = delegate* unmanaged<nint, ShapeCastResult*, float>;

public readonly struct NarrowPhaseQuery : IEquatable<NarrowPhaseQuery>
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
    public unsafe bool CastRay(
        in Vector3 origin,
        in Vector3 direction,
        out RayCastResult hit,
        BroadPhaseLayerFilter broadPhaseFilter,
        ObjectLayerFilter objectLayerFilter,
        BodyFilter bodyFilter)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(CastRay)}");

        return JPH_NarrowPhaseQuery_CastRay(Handle, in origin, in direction, out hit, broadPhaseFilter.Handle, objectLayerFilter.Handle, bodyFilter.Handle);
    }

    public unsafe bool CastRay(
        in Double3 origin,
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

    public unsafe bool CastRay(
        in Vector3 origin,
        in Vector3 direction,
        JPH_CastRayCollector callback,
        nint userData,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(CastRay)}");

        return JPH_NarrowPhaseQuery_CastRay2(Handle, in origin, in direction,
            callback, userData,
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0);
    }

    public unsafe bool CastRay(
        in Double3 origin,
        in Vector3 direction,
        JPH_CastRayCollector callback,
        nint userData,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(CastRay)}");

        return JPH_NarrowPhaseQuery_CastRay2(Handle, in origin, in direction,
            callback, userData,
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0);
    }
    #endregion

    #region CollidePoint
    public unsafe bool CollidePoint(
        in Vector3 point,
        JPH_CollidePointCollector callback,
        nint userData,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(CollidePoint)}");

        return JPH_NarrowPhaseQuery_CollidePoint(Handle,
            in point,
            callback, userData,
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0);
    }

    public unsafe bool CollidePoint(
        in Double3 point,
        JPH_CollidePointCollector callback,
        nint userData,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(CollidePoint)}");

        return JPH_NarrowPhaseQuery_CollidePoint(Handle,
            in point,
            callback, userData,
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0);
    }
    #endregion

    #region CollideShape
    public unsafe bool CollideShape(Shape shape,
        in Vector3 scale, in Matrix4x4 centerOfMassTransform, in Vector3 baseOffset,
        JPH_CollideShapeCollector callback,
        nint userData,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(CollideShape)}");

        return JPH_NarrowPhaseQuery_CollideShape(Handle, shape.Handle,
            in scale, in centerOfMassTransform, in baseOffset,
            callback, userData,
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0);
    }

    public unsafe bool CollideShape(Shape shape,
        in Vector3 scale, in RMatrix4x4 centerOfMassTransform, in Double3 baseOffset,
        JPH_CollideShapeCollector callback,
        nint userData,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(CollideShape)}");

        return JPH_NarrowPhaseQuery_CollideShape(Handle, shape.Handle,
            in scale, in centerOfMassTransform, in baseOffset,
            callback, userData,
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0);
    }
    #endregion

    #region CastShape
    public unsafe bool CastShape(Shape shape,
        in Matrix4x4 centerOfMassTransform, in Vector3 direction, in Vector3 baseOffset,
        JPH_CastShapeCollector callback,
        nint userData,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(CastShape)}");

        return JPH_NarrowPhaseQuery_CastShape(Handle, shape.Handle,
            in centerOfMassTransform, in direction, in baseOffset,
            callback, userData,
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0);
    }

    public unsafe bool CastShape(Shape shape,
        in RMatrix4x4 centerOfMassTransform, in Vector3 direction, in Double3 baseOffset,
        JPH_CastShapeCollector callback,
        nint userData,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(CastShape)}");

        return JPH_NarrowPhaseQuery_CastShape(Handle, shape.Handle,
            in centerOfMassTransform, in direction, in baseOffset,
            callback, userData,
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0,
            bodyFilter?.Handle ?? 0);
    }
    #endregion
}
