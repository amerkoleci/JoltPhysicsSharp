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
}
