// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
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

    public bool CastRay(in Double3 origin, in Vector3 direction, ref RayCastResult hit, BroadPhaseLayerFilter broadPhaseFilter,
        ObjectLayerFilter objectLayerFilter, BodyFilter bodyFilter)
    {
        Bool32 result = JPH_NarrowPhaseQuery_CastRay(Handle, origin, direction, ref hit, broadPhaseFilter.Handle, objectLayerFilter.Handle, bodyFilter.Handle);
        return result;
    }
}
