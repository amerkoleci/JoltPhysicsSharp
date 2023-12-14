// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace JoltPhysicsSharp;

public readonly struct BroadPhaseLayer(byte value) : IEquatable<BroadPhaseLayer>
{
    public byte Value { get; } = value;

    public static implicit operator BroadPhaseLayer(byte id) => new(id);
    public static implicit operator byte(in BroadPhaseLayer id) => id.Value;

    public static bool operator ==(BroadPhaseLayer left, BroadPhaseLayer right) => left.Value == right.Value;
    public static bool operator !=(BroadPhaseLayer left, BroadPhaseLayer right) => left.Value != right.Value;
    public static bool operator ==(BroadPhaseLayer left, byte right) => left.Value == right;
    public static bool operator !=(BroadPhaseLayer left, byte right) => left.Value != right;
    public bool Equals(BroadPhaseLayer other) => Value == other.Value;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is BroadPhaseLayer handle && Equals(handle);

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();
}
