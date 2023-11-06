// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;

namespace JoltPhysicsSharp;

public readonly struct Triangle : IEquatable<Triangle>
{
    public Triangle(in Vector3 v1, in Vector3 v2, in Vector3 v3, uint materialIndex = 0)
    {
        V1 = v1;
        V2 = v2;
        V3 = v3;
        MaterialIndex = materialIndex;
    }

    public Vector3 V1 { get; }
    public Vector3 V2 { get; }
    public Vector3 V3 { get; }
    public uint MaterialIndex { get; }

    public static bool operator ==(Triangle left, Triangle right)
    {
        return left.V1 == right.V1 && left.V2 == right.V2 && left.V3 == right.V3 && left.MaterialIndex == right.MaterialIndex;
    }

    public static bool operator !=(Triangle left, Triangle right)
    {
        return left.V1 != right.V1 || left.V2 != right.V2 || left.V3 != right.V3 || left.MaterialIndex != right.MaterialIndex;
    }

    public bool Equals(Triangle other) => this == other;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Triangle handle && Equals(handle);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(V1, V2, V3, MaterialIndex);

    public override string ToString() => $"V1: {V1}, V2: {V2}, V3: {V3}, MaterialIndex: {MaterialIndex}";
}
