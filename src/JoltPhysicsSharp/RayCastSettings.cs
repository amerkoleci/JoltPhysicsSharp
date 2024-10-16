// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace JoltPhysicsSharp;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct RayCastSettings : IEquatable<RayCastSettings>
{
    /// <summary>
    /// How backfacing triangles should be treated (should we report back facing hits for triangle based shapes, e.g. MeshShape/HeightFieldShape?)
    /// </summary>
	public BackFaceMode BackFaceModeTriangles = BackFaceMode.IgnoreBackFaces;

    /// <summary>
    /// How backfacing convex objects should be treated (should we report back facing hits for convex shapes?)
    /// </summary>
    public BackFaceMode BackFaceModeConvex = BackFaceMode.IgnoreBackFaces;

    /// <summary>
    /// If convex shapes should be treated as solid. When true, a ray starting inside a convex shape will generate a hit at fraction 0.
    /// </summary>
    public bool TreatConvexAsSolid = true;

    public RayCastSettings()
    {

    }

    /// <summary>
    /// Determines whether the specified <see cref="Int4"/> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="Int4"/> to compare with this instance.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(RayCastSettings other)
    {
        return
            BackFaceModeTriangles == other.BackFaceModeTriangles
            && BackFaceModeConvex == other.BackFaceModeConvex
            && TreatConvexAsSolid == other.TreatConvexAsSolid;
    }

    public static bool operator ==(RayCastSettings left, RayCastSettings right) => left.Equals(right);
    public static bool operator !=(RayCastSettings left, RayCastSettings right) => !left.Equals(right);

    /// <inheritdoc/>
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is RayCastSettings handle && Equals(handle);

    /// <inheritdoc/>
    public override readonly int GetHashCode() => HashCode.Combine(BackFaceModeTriangles, BackFaceModeConvex, TreatConvexAsSolid);
}
