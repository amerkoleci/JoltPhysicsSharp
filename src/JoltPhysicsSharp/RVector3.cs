// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace JoltPhysicsSharp;

/// <summary>
/// Vector type containing three 64 bit floating point components.
/// </summary>
[DebuggerDisplay("X={X}, Y={Y}, Z={Z}")]
public struct RVector3 : IEquatable<RVector3>, IFormattable
{
    /// <summary>
    /// The X component of the vector.
    /// </summary>
    public double X;

    /// <summary>
    /// The Y component of the vector.
    /// </summary>
    public double Y;

    /// <summary>
    /// The Z component of the vector.
    /// </summary>
    public double Z;

    internal const int Count = 3;

    /// <summary>
    /// Initializes a new instance of the <see cref="RVector3"/> struct.
    /// </summary>
    /// <param name="value">The value that will be assigned to all components.</param>
    public RVector3(double value) 
    {
        X = value;
        Y = value;
        Z = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RVector3" /> struct.
    /// </summary>
    /// <param name="x">Initial value for the X component of the vector.</param>
    /// <param name="y">Initial value for the Y component of the vector.</param>
    /// <param name="z">Initial value for the Z component of the vector.</param>
    public RVector3(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RVector3" /> struct.
    /// </summary>
    /// <param name="x">Initial value for the X component of the vector.</param>
    /// <param name="y">Initial value for the Y component of the vector.</param>
    /// <param name="z">Initial value for the Z component of the vector.</param>
    public RVector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RVector3" /> struct.
    /// </summary>
    /// <param name="xyz">Initial value for the X, Y and Z component of the vector.</param>
    public RVector3(in Vector3 xyz)
    {
        X = xyz.X;
        Y = xyz.Y;
        Z = xyz.Z;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RVector3" /> struct.
    /// </summary>
    /// <param name="xy">Initial value for the X and Y component of the vector.</param>
    /// <param name="z">Initial value for the Z component of the vector.</param>
    public RVector3(in Vector2 xy, double z)
    {
        X = xy.X;
        Y = xy.Y;
        Z = z;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RVector3" /> struct.
    /// </summary>
    /// <param name="values">The span of elements to assign to the vector.</param>
    public RVector3(ReadOnlySpan<double> values)
    {
        if (values.Length < 3)
        {
            throw new ArgumentOutOfRangeException(nameof(values), "There must be 3 uint values.");
        }

        this = Unsafe.ReadUnaligned<RVector3>(ref Unsafe.As<double, byte>(ref MemoryMarshal.GetReference(values)));
    }

    /// <summary>
    /// A <see cref="RVector3"/> with all of its components set to zero.
    /// </summary>
    public static RVector3 Zero
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => default;
    }

    /// <summary>
    /// A <see cref="Int3"/> with all of its components set to one.
    /// </summary>
    public static RVector3 One
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(1.0, 1.0, 1.0);
    }

    /// <summary>
    /// The X unit <see cref="RVector3"/> (1, 0, 0).
    /// </summary>
    public static RVector3 UnitX
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(1.0, 0.0, 0.0);
    }

    /// <summary>
    /// The Y unit <see cref="RVector3"/> (0, 1, 0).
    /// </summary>
    public static RVector3 UnitY
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(0.0, 1.0, 0.0);
    }

    /// <summary>
    /// The Y unit <see cref="RVector3"/> (0, 0, 1).
    /// </summary>
    public static RVector3 UnitZ
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(0, 0, 1);
    }

    public readonly double this[int index] => GetElement(this, index);

    public readonly void Deconstruct(out double x, out double y, out double z)
    {
        x = X;
        y = Y;
        z = Z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CopyTo(double[] array)
    {
        CopyTo(array, 0);
    }

    public readonly void CopyTo(double[] array, int index)
    {
        if (array is null)
        {
            throw new NullReferenceException(nameof(array));
        }

        if ((index < 0) || (index >= array.Length))
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        if ((array.Length - index) < 3)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        array[index] = X;
        array[index + 1] = Y;
        array[index + 2] = Z;
    }

    /// <summary>Copies the vector to the given <see cref="Span{T}" />.The length of the destination span must be at least 2.</summary>
    /// <param name="destination">The destination span which the values are copied into.</param>
    /// <exception cref="ArgumentException">If number of elements in source vector is greater than those available in destination span.</exception>
    public readonly void CopyTo(Span<double> destination)
    {
        if (destination.Length < 3)
        {
            throw new ArgumentOutOfRangeException(nameof(destination));
        }

        Unsafe.WriteUnaligned(ref Unsafe.As<double, byte>(ref MemoryMarshal.GetReference(destination)), this);
    }

    /// <summary>Attempts to copy the vector to the given <see cref="Span{Double}" />. The length of the destination span must be at least 2.</summary>
    /// <param name="destination">The destination span which the values are copied into.</param>
    /// <returns><see langword="true" /> if the source vector was successfully copied to <paramref name="destination" />. <see langword="false" /> if <paramref name="destination" /> is not large enough to hold the source vector.</returns>
    public readonly bool TryCopyTo(Span<double> destination)
    {
        if (destination.Length < 3)
        {
            return false;
        }

        Unsafe.WriteUnaligned(ref Unsafe.As<double, byte>(ref MemoryMarshal.GetReference(destination)), this);
        return true;
    }

    /// <summary>Adds two vectors together.</summary>
    /// <param name="left">The first vector to add.</param>
    /// <param name="right">The second vector to add.</param>
    /// <returns>The summed vector.</returns>
    /// <remarks>The <see cref="op_Addition" /> method defines the addition operation for <see cref="RVector3" /> objects.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RVector3 operator +(RVector3 left, RVector3 right)
    {
        return new RVector3(
            left.X + right.X,
            left.Y + right.Y,
            left.Z + right.Z
        );
    }

    /// <summary>Divides the first vector by the second.</summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>The vector that results from dividing <paramref name="left" /> by <paramref name="right" />.</returns>
    /// <remarks>The <see cref="RVector3.op_Division" /> method defines the division operation for <see cref="RVector3" /> objects.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RVector3 operator /(RVector3 left, RVector3 right)
    {
        return new RVector3(
            left.X / right.X,
            left.Y / right.Y,
            left.Z / right.Z
        );
    }

    /// <summary>Divides the specified vector by a specified scalar value.</summary>
    /// <param name="value1">The vector.</param>
    /// <param name="value2">The scalar value.</param>
    /// <returns>The result of the division.</returns>
    /// <remarks>The <see cref="RVector3.op_Division" /> method defines the division operation for <see cref="RVector3" /> objects.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RVector3 operator /(RVector3 value1, double value2)
    {
        return value1 / new RVector3(value2);
    }

    /// <summary>Returns a new vector whose values are the product of each pair of elements in two specified vectors.</summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>The element-wise product vector.</returns>
    /// <remarks>The <see cref="RVector3.op_Multiply" /> method defines the multiplication operation for <see cref="RVector3" /> objects.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RVector3 operator *(RVector3 left, RVector3 right)
    {
        return new RVector3(
            left.X * right.X,
            left.Y * right.Y,
            left.Z * right.Z
        );
    }

    /// <summary>Multiplies the specified vector by the specified scalar value.</summary>
    /// <param name="left">The vector.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>The scaled vector.</returns>
    /// <remarks>The <see cref="RVector3.op_Multiply" /> method defines the multiplication operation for <see cref="RVector3" /> objects.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RVector3 operator *(RVector3 left, double right)
    {
        return left * new RVector3(right);
    }

    /// <summary>Multiplies the scalar value by the specified vector.</summary>
    /// <param name="left">The vector.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>The scaled vector.</returns>
    /// <remarks>The <see cref="RVector3.op_Multiply" /> method defines the multiplication operation for <see cref="RVector3" /> objects.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RVector3 operator *(double left, RVector3 right)
    {
        return right * left;
    }

    /// <summary>Subtracts the second vector from the first.</summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>The vector that results from subtracting <paramref name="right" /> from <paramref name="left" />.</returns>
    /// <remarks>The <see cref="op_Subtraction" /> method defines the subtraction operation for <see cref="RVector3" /> objects.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RVector3 operator -(RVector3 left, RVector3 right)
    {
        return new RVector3(
            left.X - right.X,
            left.Y - right.Y,
            left.Z - right.Z
        );
    }

    /// <summary>Negates the specified vector.</summary>
    /// <param name="value">The vector to negate.</param>
    /// <returns>The negated vector.</returns>
    /// <remarks>The <see cref="op_UnaryNegation" /> method defines the unary negation operation for <see cref="RVector3" /> objects.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RVector3 operator -(RVector3 value)
    {
        return Zero - value;
    }

    /// <summary>
    /// Creates a new <see cref="RVector3"/> value with the same value for all its components.
    /// </summary>
    /// <param name="x">The value to use for the components of the new <see cref="RVector3"/> instance.</param>
    public static implicit operator RVector3(double x) => new(x, x, x);

    /// <summary>
    /// Casts a <see cref="Vector3"/> value to a <see cref="RVector3"/> one.
    /// </summary>
    /// <param name="xyz">The input <see cref="Vector3"/> value to cast.</param>
    public static implicit operator RVector3(in Vector3 xyz) => new(xyz);

    /// <summary>
    /// Casts a <see cref="RVector3"/> value to a <see cref="Vector3"/> one.
    /// </summary>
    /// <param name="xyz">The input <see cref="RVector3"/> value to cast.</param>
    public static explicit operator Vector3(in RVector3 xyz) => new((float)xyz.X, (float)xyz.Y, (float)xyz.Z);

    /// <summary>Returns a vector whose elements are the absolute values of each of the specified vector's elements.</summary>
    /// <param name="value">A vector.</param>
    /// <returns>The absolute value vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RVector3 Abs(RVector3 value)
    {
        return new RVector3(
            Math.Abs(value.X),
            Math.Abs(value.Y),
            Math.Abs(value.Z)
        );
    }

    /// <inheritdoc/>
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is RVector3 value && Equals(value);

    /// <summary>
    /// Determines whether the specified <see cref="RVector3"/> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="RVector3"/> to compare with this instance.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(RVector3 other)
    {
        return X == other.X
            && Y == other.Y
            && Z == other.Z;
    }

    /// <summary>
    /// Compares two <see cref="RVector3"/> objects for equality.
    /// </summary>
    /// <param name="left">The <see cref="RVector3"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="RVector3"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is equal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(RVector3 left, RVector3 right) => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="RVector3"/> objects for inequality.
    /// </summary>
    /// <param name="left">The <see cref="RVector3"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="RVector3"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is unequal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(RVector3 left, RVector3 right) => !left.Equals(right);

    /// <inheritdoc/>
    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z);

    /// <inheritdoc />
    public override readonly string ToString() => ToString(format: null, formatProvider: null);

    /// <inheritdoc />
    public readonly string ToString(string? format, IFormatProvider? formatProvider)
        => $"{nameof(RVector3)} {{ {nameof(X)} = {X.ToString(format, formatProvider)}, {nameof(Y)} = {Y.ToString(format, formatProvider)}, {nameof(Z)} = {Z.ToString(format, formatProvider)} }}";

    internal static double GetElement(RVector3 vector, int index)
    {
        return index >= Count ? throw new ArgumentOutOfRangeException(nameof(index)) : GetElementUnsafe(ref vector, index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double GetElementUnsafe(ref RVector3 vector, int index)
    {
        Debug.Assert(index is >= 0 and < Count);

        return Unsafe.Add(ref Unsafe.As<RVector3, double>(ref vector), index);
    }
}
