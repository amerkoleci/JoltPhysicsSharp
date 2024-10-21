// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
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

    public bool CastRay(
        in Ray ray,
        Memory<BroadPhaseCastResult> results,
        RayCastBodyCollector callback,
        nint userData = 0,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default)
    {
        return JPH_BroadPhaseQuery_CastRay(Handle,
            in ray.Position, in ray.Direction,
            callback, userData,
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0);
    }

    public bool CollideAABox(
        in BoundingBox box,
        CollideShapeBodyCollector callback,
        nint userData = 0,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default)
    {
        return JPH_BroadPhaseQuery_CollideAABox(Handle,
            in box,
            callback, userData,
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0);
    }

    public bool CollideSphere(
        in BoundingSphere sphere,
        CollideShapeBodyCollector callback,
        nint userData = 0,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default)
    {
        return JPH_BroadPhaseQuery_CollideSphere(Handle,
            sphere.Center, sphere.Radius,
            callback, userData,
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0);
    }

    public unsafe bool CollidePoint(
        in Vector3 point,
        CollideShapeBodyCollector callback,
        nint userData = 0,
        BroadPhaseLayerFilter? broadPhaseFilter = default,
        ObjectLayerFilter? objectLayerFilter = default)
    {
        return JPH_BroadPhaseQuery_CollidePoint(Handle,
            in point,
            callback, userData,
            broadPhaseFilter?.Handle ?? 0,
            objectLayerFilter?.Handle ?? 0);
    }
}
