// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace JoltPhysicsSharp;

public readonly partial struct SubShapeID(uint value) : IComparable, IComparable<SubShapeID>, IEquatable<SubShapeID>, IFormattable
{
    public readonly uint Value = value;

    public static bool operator ==(SubShapeID left, SubShapeID right) => left.Value == right.Value;

    public static bool operator !=(SubShapeID left, SubShapeID right) => left.Value != right.Value;

    public static bool operator <(SubShapeID left, SubShapeID right) => left.Value < right.Value;

    public static bool operator <=(SubShapeID left, SubShapeID right) => left.Value <= right.Value;

    public static bool operator >(SubShapeID left, SubShapeID right) => left.Value > right.Value;

    public static bool operator >=(SubShapeID left, SubShapeID right) => left.Value >= right.Value;


    public static implicit operator SubShapeID(uint value) => new (value);

    public static explicit operator uint(SubShapeID value) => value.Value;

    public int CompareTo(object? obj)
    {
        if (obj is SubShapeID other)
        {
            return CompareTo(other);
        }

        return (obj is null) ? 1 : throw new ArgumentException("obj is not an instance of SubShapeID.");
    }

    public int CompareTo(SubShapeID other) => Value.CompareTo(other.Value);

    public override bool Equals(object? obj) => (obj is SubShapeID other) && Equals(other);

    public bool Equals(SubShapeID other) => Value.Equals(other.Value);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();

    public string ToString(string? format, IFormatProvider? formatProvider) => Value.ToString(format, formatProvider);
}
