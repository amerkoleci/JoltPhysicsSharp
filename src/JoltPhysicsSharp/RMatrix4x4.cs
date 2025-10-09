// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace JoltPhysicsSharp;

/// <summary>
/// Represents a 4x4 matrix.
/// </summary>
public struct RMatrix4x4 : IEquatable<RMatrix4x4>, IFormattable
{
    /// <summary>
    /// The size of the <see cref="Matrix"/> type, in bytes.
    /// </summary>
    public static int SizeInBytes => Unsafe.SizeOf<RMatrix4x4>();

    /// <summary>
    /// Value at row 1 column 1 of the matrix.
    /// </summary>
    public float M11;

    /// <summary>
    /// Value at row 2 column 1 of the matrix.
    /// </summary>
    public float M21;

    /// <summary>
    /// Value at row 3 column 1 of the matrix.
    /// </summary>
    public float M31;

    /// <summary>
    /// Value at row 4 column 1 of the matrix.
    /// </summary>
    public float M41;

    /// <summary>
    /// Value at row 1 column 2 of the matrix.
    /// </summary>
    public float M12;

    /// <summary>
    /// Value at row 2 column 2 of the matrix.
    /// </summary>
    public float M22;

    /// <summary>
    /// Value at row 3 column 2 of the matrix.
    /// </summary>
    public float M32;

    /// <summary>
    /// Value at row 4 column 2 of the matrix.
    /// </summary>
    public float M42;

    /// <summary>
    /// Value at row 1 column 3 of the matrix.
    /// </summary>
    public float M13;

    /// <summary>
    /// Value at row 2 column 3 of the matrix.
    /// </summary>
    public float M23;

    /// <summary>
    /// Value at row 3 column 3 of the matrix.
    /// </summary>
    public float M33;

    /// <summary>
    /// Value at row 4 column 3 of the matrix.
    /// </summary>
    public float M43;

    /// <summary>
    /// Value at row 1 column 4 of the matrix.
    /// </summary>
    public double M14;

    /// <summary>
    /// Value at row 2 column 4 of the matrix.
    /// </summary>
    public double M24;

    /// <summary>
    /// Value at row 3 column 4 of the matrix.
    /// </summary>
    public double M34;

    /// <summary>
    /// Initializes a new instance of the <see cref="RMatrix4x4"/> struct.
    /// </summary>
    /// <param name="value">The value that will be assigned to all components.</param>
    public RMatrix4x4(float value)
    {
        M11 = M21 = M31 = M41 =
        M12 = M22 = M32 = M42 =
        M13 = M23 = M33 = M43 = value;
        M14 = M24 = M34 = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RMatrix4x4"/> struct.
    /// </summary>
    /// <param name="m11">The value to assign to the first element in the first row.</param>
    /// <param name="m12">The value to assign to the second element in the first row.</param>
    /// <param name="m13">The value to assign to the third element in the first row.</param>
    /// <param name="m14">The value to assign to the fourth element in the first row.</param>
    /// <param name="m21">The value to assign to the first element in the second row.</param>
    /// <param name="m22">The value to assign to the second element in the second row.</param>
    /// <param name="m23">The value to assign to the third element in the second row.</param>
    /// <param name="m24">The value to assign to the third element in the second row.</param>
    /// <param name="m31">The value to assign to the first element in the third row.</param>
    /// <param name="m32">The value to assign to the second element in the third row.</param>
    /// <param name="m33">The value to assign to the third element in the third row.</param>
    /// <param name="m34">The value to assign to the fourth element in the third row.</param>
    /// <param name="m41">The value to assign to the first element in the fourth row.</param>
    /// <param name="m42">The value to assign to the second element in the fourth row.</param>
    /// <param name="m43">The value to assign to the third element in the fourth row.</param>
    public RMatrix4x4(float m11, float m12, float m13, double m14,
                      float m21, float m22, float m23, double m24,
                      float m31, float m32, float m33, double m34,
                      float m41, float m42, float m43)
    {
        M11 = m11; M12 = m12; M13 = m13; M14 = m14;
        M21 = m21; M22 = m22; M23 = m23; M24 = m24;
        M31 = m31; M32 = m32; M33 = m33; M34 = m34;
        M41 = m41; M42 = m42; M43 = m43;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RMatrix4x4"/> struct.
    /// </summary>
    /// <param name="matrix">The <see cref="Matrix4x4"/> value to assign.</param>
    public RMatrix4x4(Matrix4x4 matrix)
    {
        matrix = Matrix4x4.Transpose(matrix);

        M11 = matrix.M11; M12 = matrix.M12; M13 = matrix.M13; M14 = matrix.M14;
        M21 = matrix.M21; M22 = matrix.M22; M23 = matrix.M23; M24 = matrix.M24;
        M31 = matrix.M31; M32 = matrix.M32; M33 = matrix.M33; M34 = matrix.M34;
        M41 = matrix.M41; M42 = matrix.M42; M43 = matrix.M43;
    }

    /// <summary>
    /// Gets or sets the first row in the matrix; that is M11, M12, M13, and M14.
    /// </summary>
    public Vector4 Row1
    {
        readonly get => new(M11, M12, M13, (float)M14);
        set { M11 = value.X; M12 = value.Y; M13 = value.Z; M14 = value.W; }
    }

    /// <summary>
    /// Gets or sets the second row in the matrix; that is M21, M22, M23, and M24.
    /// </summary>
    public Vector4 Row2
    {
        readonly get => new(M21, M22, M23, (float)M24);
        set { M21 = value.X; M22 = value.Y; M23 = value.Z; M24 = value.W; }
    }

    /// <summary>
    /// Gets or sets the third row in the matrix; that is M31, M32, M33, and M34.
    /// </summary>
    public Vector4 Row3
    {
        readonly get => new(M31, M32, M33, (float)M34);
        set { M31 = value.X; M32 = value.Y; M33 = value.Z; M34 = value.W; }
    }

    /// <summary>
    /// Gets or sets the fourth row in the matrix; that is M41, M42, M43 and 1.0f as M44
    /// </summary>
    public Vector4 Row4
    {
        readonly get => new(M41, M42, M43, 1.0f);
        set { M41 = value.X; M42 = value.Y; M43 = value.Z; }
    }

    /// <summary>
    /// Gets or sets the first column in the matrix; that is M11, M21, M31, and M41.
    /// </summary>
    public Vector4 Column1
    {
        readonly get { return new Vector4(M11, M21, M31, M41); }
        set { M11 = value.X; M21 = value.Y; M31 = value.Z; M41 = value.W; }
    }

    /// <summary>
    /// Gets or sets the second column in the matrix; that is M12, M22, M32, and M42.
    /// </summary>
    public Vector4 Column2
    {
        readonly get { return new Vector4(M12, M22, M32, M42); }
        set { M12 = value.X; M22 = value.Y; M32 = value.Z; M42 = value.W; }
    }

    /// <summary>
    /// Gets or sets the third column in the matrix; that is M13, M23, M33, and M43.
    /// </summary>
    public Vector4 Column3
    {
        readonly get { return new Vector4(M13, M23, M33, M43); }
        set { M13 = value.X; M23 = value.Y; M33 = value.Z; M43 = value.W; }
    }

    /// <summary>
    /// Gets or sets the fourth column in the matrix; that is M14, M24, M34, and M44.
    /// </summary>
    public RVector3 Column4
    {
        readonly get { return new RVector3(M14, M24, M34); }
        set { M14 = value.X; M24 = value.Y; M34 = value.Z; }
    }

    /// <summary>
    /// Gets or sets the translation of the matrix; that is M14, M24, and M34.
    /// </summary>
    public RVector3 Translation
    {
        readonly get => new(M14, M24, M34);
        set { M14 = value.X; M24 = value.Y; M34 = value.Z; }
    }

    /// <summary>
    /// Casts a <see cref="Matrix4x4"/> value to a <see cref="RMatrix4x4"/> one.
    /// </summary>
    /// <param name="xyz">The input <see cref="Matrix4x4"/> value to cast.</param>
    public static implicit operator RMatrix4x4(Matrix4x4 matrix) => new(matrix);

    /// <summary>
    /// Casts a <see cref="RMatrix4x4"/> value to a <see cref="Matrix4x4"/> one.
    /// </summary>
    /// <param name="xyz">The input <see cref="RMatrix4x4"/> value to cast.</param>
    public static explicit operator Matrix4x4(in RMatrix4x4 matrix)
    {
        // Note the order. Matrices from has to be transposed
        return new (
            matrix.M11, matrix.M21, matrix.M31, matrix.M41,
            matrix.M12, matrix.M22, matrix.M32, matrix.M42,
            matrix.M13, matrix.M23, matrix.M33, matrix.M43,
            (float)matrix.M14, (float)matrix.M24, (float)matrix.M34, 1.0f);
    }

    /// <inheritdoc/>
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is RMatrix4x4 value && Equals(value);

    /// <summary>
    /// Determines whether the specified <see cref="RMatrix4x4"/> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="RMatrix4x4"/> to compare with this instance.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(RMatrix4x4 other)
    {
        return (M11 == other.M11) && (M12 == other.M12) && (M13 == other.M13) && (M14 == other.M14)
            && (M21 == other.M21) && (M22 == other.M22) && (M23 == other.M23) && (M24 == other.M24)
            && (M31 == other.M31) && (M32 == other.M32) && (M33 == other.M33) && (M34 == other.M34)
            && (M41 == other.M41) && (M42 == other.M42) && (M43 == other.M43);
    }

    /// <summary>
    /// Compares two <see cref="RMatrix4x4"/> objects for equality.
    /// </summary>
    /// <param name="left">The <see cref="RMatrix4x4"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="RMatrix4x4"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is equal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(RMatrix4x4 left, RMatrix4x4 right) => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="RMatrix4x4"/> objects for inequality.
    /// </summary>
    /// <param name="left">The <see cref="RMatrix4x4"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="RMatrix4x4"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is unequal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(RMatrix4x4 left, RMatrix4x4 right) => !left.Equals(right);

    /// <inheritdoc/>
    public override readonly int GetHashCode()
    {
        HashCode hashCode = new();
        {
            hashCode.Add(M11);
            hashCode.Add(M12);
            hashCode.Add(M13);
            hashCode.Add(M14);
            hashCode.Add(M21);
            hashCode.Add(M22);
            hashCode.Add(M23);
            hashCode.Add(M24);
            hashCode.Add(M31);
            hashCode.Add(M32);
            hashCode.Add(M33);
            hashCode.Add(M34);
            hashCode.Add(M41);
            hashCode.Add(M42);
            hashCode.Add(M43);
        }
        return hashCode.ToHashCode();
    }


    /// <inheritdoc />
    public override readonly string ToString() => ToString(format: null, formatProvider: null);

    /// <inheritdoc />
    public readonly string ToString(string? format, IFormatProvider? formatProvider)
    {
        string separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;

        return new StringBuilder()
            .Append('<')
            .Append(M11.ToString(format, formatProvider)).Append(separator).Append(' ')
            .Append(M12.ToString(format, formatProvider)).Append(separator).Append(' ')
            .Append(M13.ToString(format, formatProvider)).Append(separator).Append(' ')
            .Append(M14.ToString(format, formatProvider)).Append(separator).Append(' ')
            .Append(M21.ToString(format, formatProvider)).Append(separator).Append(' ')
            .Append(M22.ToString(format, formatProvider)).Append(separator).Append(' ')
            .Append(M23.ToString(format, formatProvider)).Append(separator).Append(' ')
            .Append(M24.ToString(format, formatProvider)).Append(separator).Append(' ')
            .Append(M31.ToString(format, formatProvider)).Append(separator).Append(' ')
            .Append(M32.ToString(format, formatProvider)).Append(separator).Append(' ')
            .Append(M33.ToString(format, formatProvider)).Append(separator).Append(' ')
            .Append(M34.ToString(format, formatProvider)).Append(separator).Append(' ')
            .Append(M41.ToString(format, formatProvider)).Append(separator).Append(' ')
            .Append(M42.ToString(format, formatProvider)).Append(separator).Append(' ')
            .Append(M43.ToString(format, formatProvider)).Append(separator).Append(' ')
            .Append('>')
            .ToString();
    }
}
