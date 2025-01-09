// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics.CodeAnalysis;

namespace JoltPhysicsSharp;

public readonly partial struct CharacterID(uint value) : IComparable, IComparable<CharacterID>, IEquatable<CharacterID>, IFormattable
{
    public readonly uint Value = value;

    public static bool operator ==(CharacterID left, CharacterID right) => left.Value == right.Value;

    public static bool operator !=(CharacterID left, CharacterID right) => left.Value != right.Value;

    public static bool operator <(CharacterID left, CharacterID right) => left.Value < right.Value;

    public static bool operator <=(CharacterID left, CharacterID right) => left.Value <= right.Value;

    public static bool operator >(CharacterID left, CharacterID right) => left.Value > right.Value;

    public static bool operator >=(CharacterID left, CharacterID right) => left.Value >= right.Value;


    public static implicit operator CharacterID(uint value) => new (value);

    public static explicit operator uint(CharacterID value) => value.Value;

    public int CompareTo(object? obj)
    {
        if (obj is CharacterID other)
        {
            return CompareTo(other);
        }

        return (obj is null) ? 1 : throw new ArgumentException($"obj is not an instance of {nameof(CharacterID)}.");
    }

    public int CompareTo(CharacterID other) => Value.CompareTo(other.Value);

    public override bool Equals([NotNullWhen(true)] object? obj) => (obj is CharacterID other) && Equals(other);

    public bool Equals(CharacterID other) => Value.Equals(other.Value);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();

    public string ToString(string? format, IFormatProvider? formatProvider) => Value.ToString(format, formatProvider);
}
