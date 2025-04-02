// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics.CodeAnalysis;

namespace JoltPhysicsSharp;

public readonly struct ObjectLayer(uint value) : IEquatable<ObjectLayer>
{
    public const int Bits = 32; // Must be updated according to JPH_OBJECT_LAYER_BITS

    /// <summary>
    /// Constant value used to indicate an invalid object layer
    /// </summary>
    public const uint ObjectLayerInvalid = ~0U;
    public uint Value { get; } = value;
    public bool IsValid => Value != ObjectLayerInvalid;
    public bool IsInvalid => Value == ObjectLayerInvalid;

    public static ObjectLayer Invalid => new(ObjectLayerInvalid);

    public static implicit operator ObjectLayer(uint id) => new(id);
    public static implicit operator uint(in ObjectLayer id) => id.Value;

    public static bool operator ==(ObjectLayer left, ObjectLayer right) => left.Value == right.Value;
    public static bool operator !=(ObjectLayer left, ObjectLayer right) => left.Value != right.Value;
    public static bool operator ==(ObjectLayer left, uint right) => left.Value == right;
    public static bool operator !=(ObjectLayer left, uint right) => left.Value != right;
    public bool Equals(ObjectLayer other) => Value == other.Value;

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ObjectLayer handle && Equals(handle);

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();
}
