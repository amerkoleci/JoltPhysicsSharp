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
        // Transpose the matrix due to the different row/column major layout
        return Matrix4x4.Transpose(Unsafe.As<Mat4, Matrix4x4>(ref matrix));
    }

    internal static Mat4 ToJolt(this Matrix4x4 matrix)
    {
        // Transpose the matrix due to the different row/column major layout
        matrix = Matrix4x4.Transpose(matrix);
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
