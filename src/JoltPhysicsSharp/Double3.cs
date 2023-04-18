// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace JoltPhysicsSharp;

#pragma warning disable CS8981

/// <summary>A 3 component vector of doubles.</summary>
[Serializable]
[DebuggerDisplay("<{x}, {y}, {z}>")]
public partial struct double3 : IEquatable<double3>, IFormattable
{
    /// <summary>x component of the vector.</summary>
    public double x;
    /// <summary>y component of the vector.</summary>
    public double y;
    /// <summary>z component of the vector.</summary>
    public double z;

    /// <summary>double3 zero value.</summary>
    public static readonly double3 zero;

    /// <summary>Constructs a double3 vector from three double values.</summary>
    /// <param name="x">The constructed vector's x component will be set to this value.</param>
    /// <param name="y">The constructed vector's y component will be set to this value.</param>
    /// <param name="z">The constructed vector's z component will be set to this value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double3(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    /// <summary>Constructs a double3 vector from a double3 vector.</summary>
    /// <param name="xyz">The constructed vector's xyz components will be set to this value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double3(double3 xyz)
    {
        x = xyz.x;
        y = xyz.y;
        z = xyz.z;
    }

    /// <summary>Constructs a double3 vector from a single double value by assigning it to every component.</summary>
    /// <param name="v">double to convert to double3</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double3(double v)
    {
        x = v;
        y = v;
        z = v;
    }

    /// <summary>Constructs a double3 vector from a single bool value by converting it to double and assigning it to every component.</summary>
    /// <param name="v">bool to convert to double3</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double3(bool v)
    {
        x = v ? 1.0 : 0.0;
        y = v ? 1.0 : 0.0;
        z = v ? 1.0 : 0.0;
    }

    /// <summary>Constructs a double3 vector from a single int value by converting it to double and assigning it to every component.</summary>
    /// <param name="v">int to convert to double3</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double3(int v)
    {
        x = v;
        y = v;
        z = v;
    }

    /// <summary>Constructs a double3 vector from a single uint value by converting it to double and assigning it to every component.</summary>
    /// <param name="v">uint to convert to double3</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double3(uint v)
    {
        x = v;
        y = v;
        z = v;
    }

    /// <summary>Constructs a double3 vector from a single float value by converting it to double and assigning it to every component.</summary>
    /// <param name="v">float to convert to double3</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double3(float v)
    {
        x = v;
        y = v;
        z = v;
    }

    public double3(Vector3 v) : this()
    {
        x = v.X;
        y = v.Y;
        z = v.Z;
    }

    /// <summary>Implicitly converts a single double value to a double3 vector by assigning it to every component.</summary>
    /// <param name="v">double to convert to double3</param>
    /// <returns>Converted value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator double3(double v) => new(v);

    /// <summary>Explicitly converts a single bool value to a double3 vector by converting it to double and assigning it to every component.</summary>
    /// <param name="v">bool to convert to double3</param>
    /// <returns>Converted value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator double3(bool v) => new(v);

    /// <summary>Implicitly converts a single int value to a double3 vector by converting it to double and assigning it to every component.</summary>
    /// <param name="v">int to convert to double3</param>
    /// <returns>Converted value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator double3(int v) => new(v);

    /// <summary>Implicitly converts a single uint value to a double3 vector by converting it to double and assigning it to every component.</summary>
    /// <param name="v">uint to convert to double3</param>
    /// <returns>Converted value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator double3(uint v) => new(v);

    /// <summary>Implicitly converts a single float value to a double3 vector by converting it to double and assigning it to every component.</summary>
    /// <param name="v">float to convert to double3</param>
    /// <returns>Converted value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator double3(float v) => new(v);

    /// <summary>Implicitly converts a single float value to a double3 vector by converting it to double and assigning it to every component.</summary>
    /// <param name="v">float to convert to double3</param>
    /// <returns>Converted value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Vector3(double3 v) => new((float)v.x, (float)v.y, (float)v.z);

    /// <summary>Returns the result of a componentwise multiplication operation on two double3 vectors.</summary>
    /// <param name="lhs">Left hand side double3 to use to compute componentwise multiplication.</param>
    /// <param name="rhs">Right hand side double3 to use to compute componentwise multiplication.</param>
    /// <returns>double3 result of the componentwise multiplication.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 operator *(double3 lhs, double3 rhs) { return new double3(lhs.x * rhs.x, lhs.y * rhs.y, lhs.z * rhs.z); }

    /// <summary>Returns the result of a componentwise multiplication operation on a double3 vector and a double value.</summary>
    /// <param name="lhs">Left hand side double3 to use to compute componentwise multiplication.</param>
    /// <param name="rhs">Right hand side double to use to compute componentwise multiplication.</param>
    /// <returns>double3 result of the componentwise multiplication.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 operator *(double3 lhs, double rhs) { return new double3(lhs.x * rhs, lhs.y * rhs, lhs.z * rhs); }

    /// <summary>Returns the result of a componentwise multiplication operation on a double value and a double3 vector.</summary>
    /// <param name="lhs">Left hand side double to use to compute componentwise multiplication.</param>
    /// <param name="rhs">Right hand side double3 to use to compute componentwise multiplication.</param>
    /// <returns>double3 result of the componentwise multiplication.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 operator *(double lhs, double3 rhs) { return new double3(lhs * rhs.x, lhs * rhs.y, lhs * rhs.z); }


    /// <summary>Returns the result of a componentwise addition operation on two double3 vectors.</summary>
    /// <param name="lhs">Left hand side double3 to use to compute componentwise addition.</param>
    /// <param name="rhs">Right hand side double3 to use to compute componentwise addition.</param>
    /// <returns>double3 result of the componentwise addition.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 operator +(double3 lhs, double3 rhs) { return new double3(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z); }

    /// <summary>Returns the result of a componentwise addition operation on a double3 vector and a double value.</summary>
    /// <param name="lhs">Left hand side double3 to use to compute componentwise addition.</param>
    /// <param name="rhs">Right hand side double to use to compute componentwise addition.</param>
    /// <returns>double3 result of the componentwise addition.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 operator +(double3 lhs, double rhs) { return new double3(lhs.x + rhs, lhs.y + rhs, lhs.z + rhs); }

    /// <summary>Returns the result of a componentwise addition operation on a double value and a double3 vector.</summary>
    /// <param name="lhs">Left hand side double to use to compute componentwise addition.</param>
    /// <param name="rhs">Right hand side double3 to use to compute componentwise addition.</param>
    /// <returns>double3 result of the componentwise addition.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 operator +(double lhs, double3 rhs) { return new double3(lhs + rhs.x, lhs + rhs.y, lhs + rhs.z); }


    /// <summary>Returns the result of a componentwise subtraction operation on two double3 vectors.</summary>
    /// <param name="lhs">Left hand side double3 to use to compute componentwise subtraction.</param>
    /// <param name="rhs">Right hand side double3 to use to compute componentwise subtraction.</param>
    /// <returns>double3 result of the componentwise subtraction.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 operator -(double3 lhs, double3 rhs) { return new double3(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z); }

    /// <summary>Returns the result of a componentwise subtraction operation on a double3 vector and a double value.</summary>
    /// <param name="lhs">Left hand side double3 to use to compute componentwise subtraction.</param>
    /// <param name="rhs">Right hand side double to use to compute componentwise subtraction.</param>
    /// <returns>double3 result of the componentwise subtraction.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 operator -(double3 lhs, double rhs) { return new double3(lhs.x - rhs, lhs.y - rhs, lhs.z - rhs); }

    /// <summary>Returns the result of a componentwise subtraction operation on a double value and a double3 vector.</summary>
    /// <param name="lhs">Left hand side double to use to compute componentwise subtraction.</param>
    /// <param name="rhs">Right hand side double3 to use to compute componentwise subtraction.</param>
    /// <returns>double3 result of the componentwise subtraction.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 operator -(double lhs, double3 rhs) { return new double3(lhs - rhs.x, lhs - rhs.y, lhs - rhs.z); }


    /// <summary>Returns the result of a componentwise division operation on two double3 vectors.</summary>
    /// <param name="lhs">Left hand side double3 to use to compute componentwise division.</param>
    /// <param name="rhs">Right hand side double3 to use to compute componentwise division.</param>
    /// <returns>double3 result of the componentwise division.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 operator /(double3 lhs, double3 rhs) { return new double3(lhs.x / rhs.x, lhs.y / rhs.y, lhs.z / rhs.z); }

    /// <summary>Returns the result of a componentwise division operation on a double3 vector and a double value.</summary>
    /// <param name="lhs">Left hand side double3 to use to compute componentwise division.</param>
    /// <param name="rhs">Right hand side double to use to compute componentwise division.</param>
    /// <returns>double3 result of the componentwise division.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 operator /(double3 lhs, double rhs) { return new double3(lhs.x / rhs, lhs.y / rhs, lhs.z / rhs); }

    /// <summary>Returns the result of a componentwise division operation on a double value and a double3 vector.</summary>
    /// <param name="lhs">Left hand side double to use to compute componentwise division.</param>
    /// <param name="rhs">Right hand side double3 to use to compute componentwise division.</param>
    /// <returns>double3 result of the componentwise division.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 operator /(double lhs, double3 rhs) { return new double3(lhs / rhs.x, lhs / rhs.y, lhs / rhs.z); }


    /// <summary>Returns the result of a componentwise modulus operation on two double3 vectors.</summary>
    /// <param name="lhs">Left hand side double3 to use to compute componentwise modulus.</param>
    /// <param name="rhs">Right hand side double3 to use to compute componentwise modulus.</param>
    /// <returns>double3 result of the componentwise modulus.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 operator %(double3 lhs, double3 rhs) { return new double3(lhs.x % rhs.x, lhs.y % rhs.y, lhs.z % rhs.z); }

    /// <summary>Returns the result of a componentwise modulus operation on a double3 vector and a double value.</summary>
    /// <param name="lhs">Left hand side double3 to use to compute componentwise modulus.</param>
    /// <param name="rhs">Right hand side double to use to compute componentwise modulus.</param>
    /// <returns>double3 result of the componentwise modulus.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 operator %(double3 lhs, double rhs) { return new double3(lhs.x % rhs, lhs.y % rhs, lhs.z % rhs); }

    /// <summary>Returns the result of a componentwise modulus operation on a double value and a double3 vector.</summary>
    /// <param name="lhs">Left hand side double to use to compute componentwise modulus.</param>
    /// <param name="rhs">Right hand side double3 to use to compute componentwise modulus.</param>
    /// <returns>double3 result of the componentwise modulus.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 operator %(double lhs, double3 rhs) { return new double3(lhs % rhs.x, lhs % rhs.y, lhs % rhs.z); }


    /// <summary>Returns the result of a componentwise increment operation on a double3 vector.</summary>
    /// <param name="val">Value to use when computing the componentwise increment.</param>
    /// <returns>double3 result of the componentwise increment.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 operator ++(double3 val) { return new double3(++val.x, ++val.y, ++val.z); }


    /// <summary>Returns the result of a componentwise decrement operation on a double3 vector.</summary>
    /// <param name="val">Value to use when computing the componentwise decrement.</param>
    /// <returns>double3 result of the componentwise decrement.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 operator --(double3 val) { return new double3(--val.x, --val.y, --val.z); }

    /// <summary>Returns the result of a componentwise unary minus operation on a double3 vector.</summary>
    /// <param name="val">Value to use when computing the componentwise unary minus.</param>
    /// <returns>double3 result of the componentwise unary minus.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 operator -(double3 val) { return new double3(-val.x, -val.y, -val.z); }


    /// <summary>Returns the result of a componentwise unary plus operation on a double3 vector.</summary>
    /// <param name="val">Value to use when computing the componentwise unary plus.</param>
    /// <returns>double3 result of the componentwise unary plus.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 operator +(double3 val) { return new double3(+val.x, +val.y, +val.z); }

    /// <summary>Swizzles the vector.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public double3 xxx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return new double3(x, x, x); }
    }

    /// <summary>Swizzles the vector.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public double3 xxy
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return new double3(x, x, y); }
    }

    /// <summary>Swizzles the vector.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public double3 xxz
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return new double3(x, x, z); }
    }

    /// <summary>Swizzles the vector.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public double3 xyx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return new double3(x, y, x); }
    }

    /// <summary>Swizzles the vector.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public double3 xyy
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return new double3(x, y, y); }
    }

    /// <summary>Swizzles the vector.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public double3 xyz
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return new double3(x, y, z); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set { x = value.x; y = value.y; z = value.z; }
    }

    /// <summary>Swizzles the vector.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public double3 xzx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return new double3(x, z, x); }
    }

    /// <summary>Swizzles the vector.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public double3 xzy
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return new double3(x, z, y); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set { x = value.x; z = value.y; y = value.z; }
    }

    /// <summary>Swizzles the vector.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public double3 xzz
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return new double3(x, z, z); }
    }

    /// <summary>Swizzles the vector.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public double3 yxx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return new double3(y, x, x); }
    }


    /// <summary>Swizzles the vector.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public double3 yxy
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return new double3(y, x, y); }
    }


    /// <summary>Swizzles the vector.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public double3 yxz
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return new double3(y, x, z); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set { y = value.x; x = value.y; z = value.z; }
    }


    /// <summary>Swizzles the vector.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public double3 yyx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return new double3(y, y, x); }
    }


    /// <summary>Swizzles the vector.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public double3 yyy
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return new double3(y, y, y); }
    }


    /// <summary>Swizzles the vector.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public double3 yyz
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return new double3(y, y, z); }
    }


    /// <summary>Swizzles the vector.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public double3 yzx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return new double3(y, z, x); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set { y = value.x; z = value.y; x = value.z; }
    }


    /// <summary>Swizzles the vector.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public double3 yzy
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return new double3(y, z, y); }
    }


    /// <summary>Swizzles the vector.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public double3 yzz
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return new double3(y, z, z); }
    }


    /// <summary>Swizzles the vector.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public double3 zxx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return new double3(z, x, x); }
    }


    /// <summary>Swizzles the vector.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public double3 zxy
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return new double3(z, x, y); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set { z = value.x; x = value.y; y = value.z; }
    }


    /// <summary>Swizzles the vector.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public double3 zxz
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return new double3(z, x, z); }
    }


    /// <summary>Swizzles the vector.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public double3 zyx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return new double3(z, y, x); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set { z = value.x; y = value.y; x = value.z; }
    }


    /// <summary>Swizzles the vector.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public double3 zyy
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return new double3(z, y, y); }
    }

    /// <summary>Swizzles the vector.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public double3 zyz
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return new double3(z, y, z); }
    }

    /// <summary>Swizzles the vector.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public double3 zzx
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return new double3(z, z, x); }
    }

    /// <summary>Swizzles the vector.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public double3 zzy
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return new double3(z, z, y); }
    }

    /// <summary>Swizzles the vector.</summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public double3 zzz
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get { return new double3(z, z, z); }
    }

    /// <summary>Returns the double element at a specified index.</summary>
    public unsafe double this[int index]
    {
        get
        {
            fixed (double3* array = &this) { return ((double*)array)[index]; }
        }
        set
        {
            fixed (double* array = &x) { array[index] = value; }
        }
    }

    /// <summary>Returns true if the double3 is equal to a given double3, false otherwise.</summary>
    /// <param name="rhs">Right hand side argument to compare equality with.</param>
    /// <returns>The result of the equality comparison.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(double3 rhs) => x == rhs.x && y == rhs.y && z == rhs.z;

    /// <summary>Returns true if the double3 is equal to a given double3, false otherwise.</summary>
    /// <param name="o">Right hand side argument to compare equality with.</param>
    /// <returns>The result of the equality comparison.</returns>
    public override bool Equals(object? o) => o is double3 converted && Equals(converted);

    /// <summary>Returns a string representation of the double3.</summary>
    /// <returns>String representation of the value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => string.Format("double3({0}, {1}, {2})", x, y, z);

    /// <summary>Returns a string representation of the double3 using a specified format and culture-specific format information.</summary>
    /// <param name="format">Format string to use during string formatting.</param>
    /// <param name="formatProvider">Format provider to use during string formatting.</param>
    /// <returns>String representation of the value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToString(string? format, IFormatProvider? formatProvider) => string.Format("double3({0}, {1}, {2})", x.ToString(format, formatProvider), y.ToString(format, formatProvider), z.ToString(format, formatProvider));
    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(x, y, z);

    public static bool operator ==(double3 left, double3 right) => left.x == right.x && left.y == right.y && left.z == right.z;

    public static bool operator !=(double3 left, double3 right) => left.x != right.x || left.y != right.y || left.z != right.z;
}

public static partial class math
{
    /// <summary>Returns a double3 vector constructed from three double values.</summary>
    /// <param name="x">The constructed vector's x component will be set to this value.</param>
    /// <param name="y">The constructed vector's y component will be set to this value.</param>
    /// <param name="z">The constructed vector's z component will be set to this value.</param>
    /// <returns>double3 constructed from arguments.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 double3(double x, double y, double z) => new(x, y, z);

    /// <summary>Returns a double3 vector constructed from a double3 vector.</summary>
    /// <param name="xyz">The constructed vector's xyz components will be set to this value.</param>
    /// <returns>double3 constructed from arguments.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 double3(double3 xyz) => new(xyz);

    /// <summary>Returns a double3 vector constructed from a single double value by assigning it to every component.</summary>
    /// <param name="v">double to convert to double3</param>
    /// <returns>Converted value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 double3(double v) => new(v);

    /// <summary>Returns a double3 vector constructed from a single bool value by converting it to double and assigning it to every component.</summary>
    /// <param name="v">bool to convert to double3</param>
    /// <returns>Converted value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 double3(bool v) => new(v);

    /// <summary>Returns a double3 vector constructed from a single int value by converting it to double and assigning it to every component.</summary>
    /// <param name="v">int to convert to double3</param>
    /// <returns>Converted value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 double3(int v) => new(v);

    /// <summary>Returns a double3 vector constructed from a single uint value by converting it to double and assigning it to every component.</summary>
    /// <param name="v">uint to convert to double3</param>
    /// <returns>Converted value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 double3(uint v) => new(v);

    /// <summary>Returns a double3 vector constructed from a single float value by converting it to double and assigning it to every component.</summary>
    /// <param name="v">float to convert to double3</param>
    /// <returns>Converted value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 double3(float v) => new(v);
}
