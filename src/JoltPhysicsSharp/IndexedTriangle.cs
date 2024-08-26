// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace JoltPhysicsSharp;

public readonly struct IndexedTriangle : IEquatable<IndexedTriangle>
{
    public IndexedTriangle(in uint i1, in uint i2, in uint i3, uint materialIndex = 0, uint userData = 0)
    {
        I1 = i1;
        I2 = i2;
        I3 = i3;
        MaterialIndex = materialIndex;
        UserData = userData;
    }

    public IndexedTriangle(in int i1, in int i2, in int i3, uint materialIndex = 0, uint userData = 0)
    {
        I1 = (uint)i1;
        I2 = (uint)i2;
        I3 = (uint)i3;
        MaterialIndex = materialIndex;
        UserData = userData;
    }

    public uint I1 { get; }
    public uint I2 { get; }
    public uint I3 { get; }
    public uint MaterialIndex { get; }
    public uint UserData { get; }

    public static bool operator ==(IndexedTriangle left, IndexedTriangle right)
    {
        return
            left.I1 == right.I1
            && left.I2 == right.I2
            && left.I3 == right.I3
            && left.MaterialIndex == right.MaterialIndex
            && left.UserData == right.UserData;
    }

    public static bool operator !=(IndexedTriangle left, IndexedTriangle right)
    {
        return left.I1 != right.I1 || left.I2 != right.I2 || left.I3 != right.I3 || left.MaterialIndex != right.MaterialIndex || left.UserData != right.UserData;
    }

    public bool Equals(IndexedTriangle other) => this == other;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is IndexedTriangle handle && Equals(handle);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(I1, I2, I3, MaterialIndex);

    public override string ToString() => $"I1: {I1}, I2: {I2}, I3: {I3}, MaterialIndex: {MaterialIndex}, UserData: {UserData}";
}
