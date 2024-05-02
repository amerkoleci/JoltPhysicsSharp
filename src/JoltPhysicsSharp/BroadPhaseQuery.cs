// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public readonly struct BroadPhaseQuery : IEquatable<BroadPhaseQuery>
{
    public BroadPhaseQuery(nint handle) { Handle = handle; }
    public nint Handle { get; }
    public bool IsNull => Handle == 0;
    public static BroadPhaseQuery Null => new(0);
    public static implicit operator BroadPhaseQuery(nint handle) => new(handle);
    public static bool operator ==(BroadPhaseQuery left, BroadPhaseQuery right) => left.Handle == right.Handle;
    public static bool operator !=(BroadPhaseQuery left, BroadPhaseQuery right) => left.Handle != right.Handle;
    public static bool operator ==(BroadPhaseQuery left, nint right) => left.Handle == right;
    public static bool operator !=(BroadPhaseQuery left, nint right) => left.Handle != right;
    public bool Equals(BroadPhaseQuery other) => Handle == other.Handle;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is BroadPhaseQuery handle && Equals(handle);

    /// <inheritdoc/>
    public override int GetHashCode() => Handle.GetHashCode();

    public unsafe bool CastRay(
        in Vector3 origin,
        in Vector3 direction,
        delegate* unmanaged<void*, BroadPhaseCastResult*, float> callback, void* userData,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default)
    {
        fixed (Vector3* originPtr = &origin)
        fixed (Vector3* directionPtr = &direction)
        {
            Bool32 result = JPH_BroadPhaseQuery_CastRay(Handle,
                originPtr, directionPtr,
                callback, userData,
                broadPhaseFilter?.Handle ?? 0,
                objectLayerFilter?.Handle ?? 0);
            return result;
        }
    }

    public unsafe bool CollideAABox(
        in BoundingBox box,
        delegate* unmanaged<void*, in BodyID, void>* callback, void* userData,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default)
    {
        fixed (BoundingBox* boxPtr = &box)
        {
            Bool32 result = JPH_BroadPhaseQuery_CollideAABox(Handle,
                boxPtr,
                callback, userData,
                broadPhaseFilter?.Handle ?? 0,
                objectLayerFilter?.Handle ?? 0);
            return result;
        }
    }

    public unsafe bool CollideSphere(
        in Vector3 center, float radius,
        delegate* unmanaged<void*, in BodyID, void> callback, void* userData,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default)
    {
        fixed (Vector3* centerPtr = &center)
        {
            Bool32 result = JPH_BroadPhaseQuery_CollideSphere(Handle,
                centerPtr, radius,
                callback, userData,
                broadPhaseFilter?.Handle ?? 0,
                objectLayerFilter?.Handle ?? 0);
            return result;
        }
    }

    public unsafe bool CollidePoint(
        in Vector3 point,
        delegate* unmanaged<void*, in BodyID, void> callback, void* userData,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default)
    {
        fixed (Vector3* pointPtr = &point)
        {
            Bool32 result = JPH_BroadPhaseQuery_CollidePoint(Handle,
                pointPtr,
                callback, userData,
                broadPhaseFilter?.Handle ?? 0,
                objectLayerFilter?.Handle ?? 0);
            return result;
        }
    }
}
