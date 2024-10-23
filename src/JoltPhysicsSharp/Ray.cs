// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace JoltPhysicsSharp;

/// <summary>
/// Defines a ray.
/// </summary>
public struct Ray
    : IEquatable<Ray>
    , IFormattable
{
    /// <summary>
    /// The position (origin) in three dimensional space where the ray starts.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    /// The normalized direction in which the ray points.
    /// </summary>
    public Vector3 Direction;

    /// <summary>
    /// Initializes a new instance of the <see cref="Ray"/> struct.
    /// </summary>
    /// <param name="position">The position in three dimensional space of the origin of the ray.</param>
    /// <param name="direction">The normalized direction of the ray.</param>
    public Ray(in Vector3 position, in Vector3 direction)
    {
        Position = position;
        Direction = direction;
    }

    /// <summary>
    /// Translate ray using translation
    /// </summary>
    /// <param name="translation"></param>
    /// <returns></returns>
	public Ray Translated(in Vector3 translation)
    {
        return new(translation + Position, Direction);
    }

    /// <summary>
    /// Get point with fraction inFraction on ray (0 = start of ray, 1 = end of ray)
    /// </summary>
    /// <param name="fraction"></param>
    /// <returns></returns>
	public readonly Vector3 GetPointOnRay(float fraction)
    {
        return Position + fraction * Direction;
    }

    /// <summary>
    /// Transform this ray using transform
    /// </summary>
    /// <param name="transform"></param>
    /// <returns></returns>
	public static Ray Transform(in Ray ray, in Matrix4x4 transform)
    {
        Vector3 rayOrigin = Vector3.Transform(ray.Position, transform);
        Vector3 rayDirection = Vector3.Transform(ray.Position + ray.Direction, transform) - rayOrigin;
        return new(rayOrigin, rayDirection);
    }

    /// <inheritdoc/>
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Ray value && Equals(value);

    /// <summary>
    /// Determines whether the specified <see cref="Ray"/> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="Int4"/> to compare with this instance.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Ray other)
    {
        return Position.Equals(other.Position) && Direction.Equals(other.Direction);
    }

    /// <summary>
    /// Compares two <see cref="Ray"/> objects for equality.
    /// </summary>
    /// <param name="left">The <see cref="Ray"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="Ray"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is equal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Ray left, Ray right) => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="Ray"/> objects for inequality.
    /// </summary>
    /// <param name="left">The <see cref="Ray"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="Ray"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is unequal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Ray left, Ray right) => !left.Equals(right);

    /// <inheritdoc/>
	public override readonly int GetHashCode()
    {
        var hashCode = new HashCode();
        {
            hashCode.Add(Position);
            hashCode.Add(Direction);
        }
        return hashCode.ToHashCode();
    }

    /// <inheritdoc />
    public override readonly string ToString() => ToString(format: null, formatProvider: null);

    /// <inheritdoc />
    public readonly string ToString(string? format, IFormatProvider? formatProvider)
    {
        return $"Position:{Position.ToString(format, formatProvider)} Direction:{Direction.ToString(format, formatProvider)}";
    }
}
