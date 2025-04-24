// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics.CodeAnalysis;

namespace JoltPhysicsSharp;

public readonly partial struct CollisionSubGroupID(uint value) : IComparable, IComparable<CollisionSubGroupID>, IEquatable<CollisionSubGroupID>, IFormattable
{
    public readonly uint Value = value;

    public static CollisionSubGroupID Invalid => new(~0U);

    public static bool operator ==(CollisionSubGroupID left, CollisionSubGroupID right) => left.Value == right.Value;

    public static bool operator !=(CollisionSubGroupID left, CollisionSubGroupID right) => left.Value != right.Value;

    public static bool operator <(CollisionSubGroupID left, CollisionSubGroupID right) => left.Value < right.Value;

    public static bool operator <=(CollisionSubGroupID left, CollisionSubGroupID right) => left.Value <= right.Value;

    public static bool operator >(CollisionSubGroupID left, CollisionSubGroupID right) => left.Value > right.Value;

    public static bool operator >=(CollisionSubGroupID left, CollisionSubGroupID right) => left.Value >= right.Value;


    public static implicit operator CollisionSubGroupID(uint value) => new (value);

    public static explicit operator uint(CollisionSubGroupID value) => value.Value;

    public int CompareTo(object? obj)
    {
        if (obj is CollisionSubGroupID other)
        {
            return CompareTo(other);
        }

        return (obj is null) ? 1 : throw new ArgumentException($"obj is not an instance of {nameof(CollisionSubGroupID)}.");
    }

    public int CompareTo(CollisionSubGroupID other) => Value.CompareTo(other.Value);

    public override bool Equals([NotNullWhen(true)] object? obj) => (obj is CollisionSubGroupID other) && Equals(other);

    public bool Equals(CollisionSubGroupID other) => Value.Equals(other.Value);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();

    public string ToString(string? format, IFormatProvider? formatProvider) => Value.ToString(format, formatProvider);
}
