// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

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

        Unsafe.SkipInit(out hit);
        fixed (Vector3* originPtr = &origin)
        fixed (Vector3* directionPtr = &direction)
        fixed (RayCastResult* hitPtr = &hit)
        {
            Bool32 result = JPH_NarrowPhaseQuery_CastRay(Handle, originPtr, directionPtr, hitPtr, broadPhaseFilter.Handle, objectLayerFilter.Handle, bodyFilter.Handle);
            return result;
        }
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

        Unsafe.SkipInit(out hit);
        fixed (Double3* originPtr = &origin)
        fixed (Vector3* directionPtr = &direction)
        fixed (RayCastResult* hitPtr = &hit)
        {
            Bool32 result = JPH_NarrowPhaseQuery_CastRayDouble(Handle, originPtr, directionPtr, hitPtr, broadPhaseFilter.Handle, objectLayerFilter.Handle, bodyFilter.Handle);
            return result;
        }
    }

    public unsafe bool CastRay(
        in Vector3 origin,
        in Vector3 direction,
        delegate* unmanaged<void*, RayCastResult*, float> callback, void* userData,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(CastRay)}");

        fixed (Vector3* originPtr = &origin)
        fixed (Vector3* directionPtr = &direction)
        {
            Bool32 result = JPH_NarrowPhaseQuery_CastRay2(Handle, originPtr, directionPtr,
                callback, userData,
                broadPhaseFilter?.Handle ?? 0,
                objectLayerFilter?.Handle ?? 0,
                bodyFilter?.Handle ?? 0);
            return result;
        }
    }

    public unsafe bool CastRay(
        in Double3 origin,
        in Vector3 direction,
        delegate* unmanaged<void*, RayCastResult*, float> callback, void* userData,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(CastRay)}");

        fixed (Double3* originPtr = &origin)
        fixed (Vector3* directionPtr = &direction)
        {
            Bool32 result = JPH_NarrowPhaseQuery_CastRay2Double(Handle, originPtr, directionPtr,
                callback, userData,
                broadPhaseFilter?.Handle ?? 0,
                objectLayerFilter?.Handle ?? 0,
                bodyFilter?.Handle ?? 0);
            return result;
        }
    } 
    #endregion

    #region CollidePoint
    public unsafe bool CollidePoint(
        in Vector3 point,
        delegate* unmanaged<void*, CollidePointResult*, float> callback, void* userData,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(CollidePoint)}");

        fixed (Vector3* pointPtr = &point)
        {
            Bool32 result = JPH_NarrowPhaseQuery_CollidePoint(Handle,
                pointPtr,
                callback, userData,
                broadPhaseFilter?.Handle ?? 0,
                objectLayerFilter?.Handle ?? 0,
                bodyFilter?.Handle ?? 0);
            return result;
        }
    }

    public unsafe bool CollidePoint(
        in Double3 point,
        delegate* unmanaged<void*, CollidePointResult*, float> callback, void* userData,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(CollidePoint)}");

        fixed (Double3* pointPtr = &point)
        {
            Bool32 result = JPH_NarrowPhaseQuery_CollidePointDouble(Handle,
                pointPtr,
                callback, userData,
                broadPhaseFilter?.Handle ?? 0,
                objectLayerFilter?.Handle ?? 0,
                bodyFilter?.Handle ?? 0);
            return result;
        }
    }
    #endregion

    #region CollideShape
    public unsafe bool CollideShape(Shape shape, 
        in Vector3 scale, in Matrix4x4 centerOfMassTransform, in Vector3 baseOffset,
        delegate* unmanaged<void*, CollideShapeResult*, float> callback, void* userData,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(CollideShape)}");

        fixed (Vector3* scalePtr = &scale)
        fixed (Matrix4x4* centerOfMassTransformPtr = &centerOfMassTransform)
        fixed (Vector3* baseOffsetPtr = &baseOffset)
        {
            Bool32 result = JPH_NarrowPhaseQuery_CollideShape(Handle, shape.Handle,
                scalePtr, centerOfMassTransformPtr, baseOffsetPtr,
                callback, userData,
                broadPhaseFilter?.Handle ?? 0,
                objectLayerFilter?.Handle ?? 0,
                bodyFilter?.Handle ?? 0);
            return result;
        }
    }

    public unsafe bool CollideShape(Shape shape,
        in Vector3 scale, in RMatrix4x4 centerOfMassTransform, in Double3 baseOffset,
        delegate* unmanaged<void*, CollideShapeResult*, float> callback, void* userData,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(CollideShape)}");

        fixed (Vector3* scalePtr = &scale)
        fixed (RMatrix4x4* centerOfMassTransformPtr = &centerOfMassTransform)
        fixed (Double3* baseOffsetPtr = &baseOffset)
        {
            Bool32 result = JPH_NarrowPhaseQuery_CollideShapeDouble(Handle, shape.Handle,
                scalePtr, centerOfMassTransformPtr, baseOffsetPtr,
                callback, userData,
                broadPhaseFilter?.Handle ?? 0,
                objectLayerFilter?.Handle ?? 0,
                bodyFilter?.Handle ?? 0);
            return result;
        }
    }
    #endregion

    #region CastShape
    public unsafe bool CastShape(Shape shape,
        in Matrix4x4 centerOfMassTransform, in Vector3 direction, in Vector3 baseOffset,
        delegate* unmanaged<void*, ShapeCastResult*, float> callback, void* userData,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default)
    {
        if (DoublePrecision)
            throw new InvalidOperationException($"Double precision is enabled: use {nameof(CastShape)}");

        fixed (Matrix4x4* centerOfMassTransformPtr = &centerOfMassTransform)
        fixed (Vector3* directionPtr = &direction)
        fixed (Vector3* baseOffsetPtr = &baseOffset)
        {
            Bool32 result = JPH_NarrowPhaseQuery_CastShape(Handle, shape.Handle,
                centerOfMassTransformPtr, directionPtr, baseOffsetPtr,
                callback, userData,
                broadPhaseFilter?.Handle ?? 0,
                objectLayerFilter?.Handle ?? 0,
                bodyFilter?.Handle ?? 0);
            return result;
        }
    }

    public unsafe bool CastShape(Shape shape,
        in RMatrix4x4 centerOfMassTransform, in Vector3 direction, in Double3 baseOffset,
        delegate* unmanaged<void*, ShapeCastResult*, float> callback, void* userData,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default,
        BodyFilter? bodyFilter = default)
    {
        if (!DoublePrecision)
            throw new InvalidOperationException($"Double precision is disabled: use {nameof(CastShape)}");

        fixed (RMatrix4x4* centerOfMassTransformPtr = &centerOfMassTransform)
        fixed (Vector3* directionPtr = &direction)
        fixed (Double3* baseOffsetPtr = &baseOffset)
        {
            Bool32 result = JPH_NarrowPhaseQuery_CastShapeDouble(Handle, shape.Handle,
                centerOfMassTransformPtr, directionPtr, baseOffsetPtr,
                callback, userData,
                broadPhaseFilter?.Handle ?? 0,
                objectLayerFilter?.Handle ?? 0,
                bodyFilter?.Handle ?? 0);
            return result;
        }
    }
    #endregion
}
