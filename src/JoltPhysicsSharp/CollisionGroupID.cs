// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics.CodeAnalysis;

namespace JoltPhysicsSharp;

public readonly partial struct CollisionGroupID(uint value) : IComparable, IComparable<CollisionGroupID>, IEquatable<CollisionGroupID>, IFormattable
{
    public readonly uint Value = value;

    public static CollisionGroupID Invalid => new(~0U);

    public static bool operator ==(CollisionGroupID left, CollisionGroupID right) => left.Value == right.Value;

    public static bool operator !=(CollisionGroupID left, CollisionGroupID right) => left.Value != right.Value;

    public static bool operator <(CollisionGroupID left, CollisionGroupID right) => left.Value < right.Value;

    public static bool operator <=(CollisionGroupID left, CollisionGroupID right) => left.Value <= right.Value;

    public static bool operator >(CollisionGroupID left, CollisionGroupID right) => left.Value > right.Value;

    public static bool operator >=(CollisionGroupID left, CollisionGroupID right) => left.Value >= right.Value;


    public static implicit operator CollisionGroupID(uint value) => new (value);

    public static explicit operator uint(CollisionGroupID value) => value.Value;

    public int CompareTo(object? obj)
    {
        if (obj is CollisionGroupID other)
        {
            return CompareTo(other);
        }

        return (obj is null) ? 1 : throw new ArgumentException($"obj is not an instance of {nameof(CollisionGroupID)}.");
    }

    public int CompareTo(CollisionGroupID other) => Value.CompareTo(other.Value);

    public override bool Equals([NotNullWhen(true)] object? obj) => (obj is CollisionGroupID other) && Equals(other);

    public bool Equals(CollisionGroupID other) => Value.Equals(other.Value);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();

    public string ToString(string? format, IFormatProvider? formatProvider) => Value.ToString(format, formatProvider);
}
