// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace JoltPhysicsSharp;

public readonly struct ObjectLayer : IEquatable<ObjectLayer>
{
    /// <summary>
    /// Constant value used to indicate an invalid object layer
    /// </summary>
    public const ushort ObjectLayerInvalid = 0xffff;

    public ObjectLayer(ushort value)
    {
        Value = value;
    }

    public ushort Value { get; }
    public bool IsValid => Value != ObjectLayerInvalid;
    public bool IsInvalid => Value == ObjectLayerInvalid;

    public static ObjectLayer Invalid = new ObjectLayer(ObjectLayerInvalid);

    public static implicit operator ObjectLayer(byte id) => new(id);
    public static implicit operator ushort(in ObjectLayer id) => id.Value;

    public static bool operator ==(ObjectLayer left, ObjectLayer right) => left.Value == right.Value;
    public static bool operator !=(ObjectLayer left, ObjectLayer right) => left.Value != right.Value;
    public static bool operator ==(ObjectLayer left, ushort right) => left.Value == right;
    public static bool operator !=(ObjectLayer left, ushort right) => left.Value != right;
    public bool Equals(ObjectLayer other) => Value == other.Value;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ObjectLayer handle && Equals(handle);

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();
}
