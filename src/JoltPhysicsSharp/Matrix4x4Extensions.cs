// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace JoltPhysicsSharp;

public static class Matrix4x4Extensions
{
    internal static Matrix4x4 FromJolt(this Mat4 matrix)
    {
        // No transpose needed: Jolt's Mat44 is column-major with a column-vector
        // convention (M * v), while System.Numerics.Matrix4x4 is row-major with a
        // row-vector convention (v * M). The two differences cancel out, so the
        // memory layouts are identical (translation at offsets 48/52/56 in both).
        return Unsafe.As<Mat4, Matrix4x4>(ref matrix);
    }

    internal static Mat4 ToJolt(this Matrix4x4 matrix)
    {
        // See FromJolt: the memory layouts are identical, reinterpret directly.
        return Unsafe.As<Matrix4x4, Mat4>(ref matrix);
    }

    public static Vector4 GetColumn(in this Matrix4x4 matrix, int j)
    {
        return new(matrix[0, j], matrix[1, j], matrix[2, j], matrix[3, j]);
    }

    public static void SetColumn(ref this Matrix4x4 matrix, int j, Vector4 value)
    {
        matrix[0, j] = value.X;
        matrix[1, j] = value.Y;
        matrix[2, j] = value.Z;
        matrix[3, j] = value.W;
    }
}
