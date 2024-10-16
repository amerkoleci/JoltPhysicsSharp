// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace JoltPhysicsSharp;

/// <summary>Defines a bounding box.</summary>
public struct BoundingBox : IEquatable<BoundingBox>
{
    /// <summary>
    /// Specifies the total number of corners (8) in the BoundingBox.
    /// </summary>
    public const int CornerCount = 8;

    /// <summary>
    /// A <see cref="BoundingBox"/> which represents an empty space.
    /// </summary>
    public static BoundingBox Zero => new(Vector3.Zero, Vector3.Zero);

    private Vector3 _min;
    private Vector3 _max;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundingBox"/> struct.
    /// </summary>
    /// <param name="min">The minimum vertex of the bounding box.</param>
    /// <param name="max">The maximum vertex of the bounding box.</param>
    public BoundingBox(Vector3 min, Vector3 max)
    {
        _min = min;
        _max = max;
    }

    /// <summary>
    /// The minimum point of the box.
    /// </summary>
    public Vector3 Min
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _min;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _min = value;
    }

    /// <summary>
    /// The maximum point of the box.
    /// </summary>
    public Vector3 Max
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => _max;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _max = value;
    }

    /// <summary>
    /// Gets the center of this bouding box.
    /// </summary>
    public readonly Vector3 Center
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (_min + _max) / 2;
    }

    /// <summary>
    /// Gets the extent of this bouding box.
    /// </summary>
    public readonly Vector3 Extent
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (_max - _min) / 2;
    }

    /// <summary>
    /// Gets size  of this bouding box.
    /// </summary>
    public readonly Vector3 Size => _max - _min;

    /// <summary>
    /// Gets the width of the bounding box.
    /// </summary>
    public readonly float Width
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Extent.X * 2.0f;
    }

    /// <summary>
    /// Gets the height of the bounding box.
    /// </summary>
    public readonly float Height
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Extent.Y * 2.0f;

    }

    /// <summary>
    /// Gets the depth of the bounding box.
    /// </summary>
    public readonly float Depth
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Extent.Z * 2.0f;
    }

    /// <summary>
    /// Gets the volume of the bounding box.
    /// </summary>
    public readonly float Volume
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            Vector3 sides = _max - _min;
            return sides.X * sides.Y * sides.Z;
        }
    }

    /// <summary>
    /// Get the perimeter length.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float GetPerimeter()
    {
        Vector3 sides = _max - _min;
        return 4 * (sides.X + sides.Y + sides.Z);
    }

    /// <summary>
    /// Get the surface area.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float GetSurfaceArea()
    {
        Vector3 sides = _max - _min;
        return 2 * (sides.X * sides.Y + sides.X * sides.Z + sides.Y * sides.Z);
    }

    /// <inheritdoc/>
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => (obj is BoundingBox other) && Equals(other);

    /// <summary>
    /// Determines whether the specified <see cref="BoundingBox"/> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="Int4"/> to compare with this instance.</param>
    public readonly bool Equals(BoundingBox other)
    {
        return _min.Equals(other._min)
            && _max.Equals(other._max);
    }

    /// <summary>
    /// Compares two <see cref="BoundingBox"/> objects for equality.
    /// </summary>
    /// <param name="left">The <see cref="BoundingBox"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="BoundingBox"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is equal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(BoundingBox left, BoundingBox right)
    {
        return (left._min == right._min)
            && (left._max == right._max);
    }

    /// <summary>
    /// Compares two <see cref="BoundingBox"/> objects for inequality.
    /// </summary>
    /// <param name="left">The <see cref="BoundingBox"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="BoundingBox"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is unequal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(BoundingBox left, BoundingBox right)
    {
        return (left._min != right._min)
            || (left._max != right._max);
    }

    /// <inheritdoc/>
    public override readonly int GetHashCode() => HashCode.Combine(_min, _max);

    /// <inheritdoc />
    public override string ToString() => ToString(format: null, formatProvider: null);

    /// <inheritdoc />
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return $"{nameof(BoundingBox)} {{ {nameof(Min)} = {Min.ToString(format, formatProvider)}, {nameof(Max)} = {Max.ToString(format, formatProvider)} }}";
    }
}
