// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;

namespace JoltPhysicsSharp;

public static class Matrix4x4Extensions
{
    public static Vector4 GetColumn(this Matrix4x4 matrix, int j)
    {
        return new(matrix[0, j], matrix[1, j], matrix[2,j], matrix[3,j]);
    }

    public static void SetColumn(this Matrix4x4 matrix, int j, Vector4 value)
    {
        matrix[0, j] = value.X;
        matrix[1, j] = value.Y;
        matrix[2, j] = value.Z;
        matrix[3, j] = value.W;
    }
}
