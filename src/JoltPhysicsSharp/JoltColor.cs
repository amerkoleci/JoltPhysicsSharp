// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace JoltPhysicsSharp;

/// <summary>
/// Represents a 32-bit RGBA color (4 bytes).
/// </summary>
/// <remarks>Equivalent of XMUBYTEN4.</remarks>
[StructLayout(LayoutKind.Explicit)]
public readonly struct JoltColor : IEquatable<JoltColor>
{
    /// <summary>
    /// The red component of the color.
    /// </summary>
    [FieldOffset(0)]
    public readonly byte R;

    /// <summary>
    /// The green component of the color.
    /// </summary>
    [FieldOffset(1)]
    public readonly byte G;

    /// <summary>
    /// The blue component of the color.
    /// </summary>
    [FieldOffset(2)]
    public readonly byte B;

    /// <summary>
    /// The alpha component of the color.
    /// </summary>
    [FieldOffset(3)]
    public readonly byte A;

    /// <summary>
    /// Gets or Sets the current color as a packed value.
    /// </summary>
    [field: FieldOffset(0)]
    public uint PackedValue { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JoltColor"/> struct.
    /// </summary>
    /// <param name="packedValue">The packed value to assign.</param>
    public JoltColor(uint packedValue)
    {
        PackedValue = packedValue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JoltColor"/> struct.
    /// </summary>
    /// <param name="value">The value that will be assigned to all components.</param>
    public JoltColor(float value)
        : this(value, value, value, value)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JoltColor"/> struct.
    /// </summary>
    /// <param name="r">Red component.</param>
    /// <param name="g">Green component.</param>
    /// <param name="b">Blue component.</param>
    /// <param name="a">Alpha component.</param>
    public JoltColor(float r, float g, float b, float a = 1.0f)
    {
        Vector128<float> result = Saturate(Vector128.Create(r, g, b, a));
        result = Vector128.Multiply(result, /*UByteMax*/ Vector128.Create(255.0f, 255.0f, 255.0f, 255.0f));
        result = Truncate(result);

        R = (byte)result.GetElement(0);
        G = (byte)result.GetElement(1);
        B = (byte)result.GetElement(2);
        A = (byte)result.GetElement(3);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JoltColor"/> struct.  Passed values are clamped within byte range.
    /// </summary>
    /// <param name="red">The red component of the color.</param>
    /// <param name="green">The green component of the color.</param>
    /// <param name="blue">The blue component of the color.</param>
    /// <param name="alpha">The alpha component of the color</param>
    public JoltColor(int red, int green, int blue, int alpha = 255)
    {
        R = ToByte(red);
        G = ToByte(green);
        B = ToByte(blue);
        A = ToByte(alpha);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JoltColor"/> struct.
    /// </summary>
    /// <param name="vector">The red, green, and blue components of the color.</param>
    /// <param name="alpha">The alpha component of the color.</param>
    public JoltColor(in Vector3 vector, float alpha = 1.0f)
        : this(vector.X, vector.Y, vector.Z, alpha)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JoltColor"/> struct.
    /// </summary>
    /// <param name="vector">A four-component color.</param>
    public JoltColor(Vector4 vector)
        : this(vector.X, vector.Y, vector.Z, vector.W)
    {
    }

    public readonly void Deconstruct(out byte red, out byte green, out byte blue, out byte alpha)
    {
        red = R;
        green = G;
        blue = B;
        alpha = A;
    }

    /// <summary>
    /// Converts the color into a packed integer.
    /// </summary>
    /// <returns>A packed integer containing all four color components.</returns>
    public int ToBgra()
    {
        int value = B;
        value |= G << 8;
        value |= R << 16;
        value |= A << 24;

        return value;
    }

    /// <summary>
    /// Converts the color into a packed integer.
    /// </summary>
    /// <returns>A packed integer containing all four color components.</returns>
    public int ToRgba()
    {
        int value = R;
        value |= G << 8;
        value |= B << 16;
        value |= A << 24;
        return value;
    }

    /// <summary>
    /// Converts the color into a packed integer.
    /// </summary>
    /// <returns>A packed integer containing all four color components.</returns>
    public int ToArgb()
    {
        int value = A;
        value |= R << 8;
        value |= G << 16;
        value |= B << 24;

        return value;
    }

    /// <summary>
    /// Converts the color into a packed integer.
    /// </summary>
    /// <returns>A packed integer containing all four color components.</returns>
    public int ToAbgr()
    {
        int value = A;
        value |= B << 8;
        value |= G << 16;
        value |= R << 24;

        return value;
    }

    /// <summary>
    /// Converts the color into a three component vector.
    /// </summary>
    /// <returns>A three component vector containing the red, green, and blue components of the color.</returns>
    public Vector3 ToVector3()
    {
        return new Vector3(R / 255.0f, G / 255.0f, B / 255.0f);
    }

    /// <summary>Reinterprets the current instance as a new <see cref="Vector128{Single}" />.</summary>
    /// <returns>The current instance reinterpreted as a new <see cref="Vector128{Single}" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector128<float> AsVector128() => Vector128.Create(R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f);

    /// <summary>
    /// Gets a four-component vector representation for this object.
    /// </summary>
    public Vector4 ToVector4()
    {
        return new(R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f);
    }

    /// Get grayscale intensity of color
	public byte GetIntensity() => (byte)((R * 54 + G * 183 + B * 19) >> 8);

    /// <summary>
    /// Performs an implicit conversion from <see cref="JoltColor"/> to <see cref="System.Drawing.Color"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator System.Drawing.Color(JoltColor value) => System.Drawing.Color.FromArgb(value.A, value.R, value.G, value.B);

    /// <summary>
    /// Performs an explicit conversion from <see cref="System.Drawing.Color"/> to <see cref="JoltColor"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator JoltColor(in System.Drawing.Color value) => new(value.R, value.G, value.B, value.A);

    /// <summary>
    /// Performs an implicit conversion from <see cref="JoltColor"/> to <see cref="Vector4"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Vector4(JoltColor value) => value.ToVector4();

    /// <summary>
    /// Performs an explicit conversion from <see cref="Vector3"/> to <see cref="JoltColor"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator JoltColor(in Vector3 value) => new(value.X, value.Y, value.Z, 1.0f);

    /// <summary>
    /// Performs an explicit conversion from <see cref="Vector4"/> to <see cref="JoltColor"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator JoltColor(in Vector4 value) => new(value.X, value.Y, value.Z, value.W);

    /// <summary>
    /// Performs an explicit conversion from <see cref="JoltColor"/> to <see cref="int"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator int(in JoltColor value) => value.ToRgba();

    /// <summary>
    /// Performs an explicit conversion from <see cref="int"/> to <see cref="JoltColor"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator JoltColor(int value) => new(value);

    /// <summary>
    /// Performs an explicit conversion from <see cref="int"/> to <see cref="JoltColor"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator JoltColor(uint value) => new(value);

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is JoltColor color && Equals(color);

    /// <summary>
    /// Determines whether the specified <see cref="JoltColor"/> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="JoltColor"/> to compare with this instance.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(JoltColor other)
    {
        return R == other.R
            && G == other.G
            && B == other.B
            && A == other.A;
    }

    /// <summary>
    /// Compares two <see cref="JoltColor"/> objects for equality.
    /// </summary>
    /// <param name="left">The <see cref="JoltColor"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="JoltColor"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is equal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(JoltColor left, JoltColor right) => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="JoltColor"/> objects for inequality.
    /// </summary>
    /// <param name="left">The <see cref="JoltColor"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="JoltColor"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is unequal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(JoltColor left, JoltColor right) => !left.Equals(right);

    /// <inheritdoc/>
    public override int GetHashCode() => PackedValue.GetHashCode();

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"R={R}, G={G}, B={B}, A={A}";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<float> Clamp(Vector128<float> vector, Vector128<float> min, Vector128<float> max)
    {
        Vector128<float> result = Vector128.Max(min, vector);
        result = Vector128.Min(max, result);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<float> Saturate(Vector128<float> vector)
    {
        return Clamp(vector, Vector128<float>.Zero, Vector128<float>.One);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<float> Truncate(Vector128<float> vector)
    {
        if (Sse.IsSupported)
        {
            return Sse41.RoundToZero(vector);
        }
        else if (AdvSimd.IsSupported)
        {
            return AdvSimd.RoundToZero(vector);
        }
        else
        {
            return SoftwareFallback(vector);
        }

        static Vector128<float> SoftwareFallback(Vector128<float> vector)
            => Vector128.Create(
                MathF.Truncate(vector.GetElement(0)),
                MathF.Truncate(vector.GetElement(1)),
                MathF.Truncate(vector.GetElement(2)),
                MathF.Truncate(vector.GetElement(3))
            );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte ToByte(int value)
    {
        return (byte)(value < 0 ? 0 : value > 255 ? 255 : value);
    }
}
