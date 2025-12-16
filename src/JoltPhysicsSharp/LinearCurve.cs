// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class LinearCurve : NativeObject
{
    public LinearCurve()
        : base(JPH_LinearCurve_Create(), true)
    {
    }

    internal LinearCurve(nint handle, bool owns = true)
        : base(handle, owns)
    {
    }

    protected override void DisposeNative()
    {
        JPH_LinearCurve_Destroy(Handle);
    }

    public void Clear() => JPH_LinearCurve_Clear(Handle);
    public void Reserve(int count) => JPH_LinearCurve_Reserve(Handle, count);
    public void Sort() => JPH_LinearCurve_Sort(Handle);
    public void AddPoint(in Vector2 point) => JPH_LinearCurve_AddPoint(Handle, point.X, point.Y);
    public void AddPoint(float x, float y) => JPH_LinearCurve_AddPoint(Handle, x, y);

    public int PointCount => JPH_LinearCurve_GetPointCount(Handle);
    public float MinX => JPH_LinearCurve_GetMinX(Handle);
    public float MaxX => JPH_LinearCurve_GetMaxX(Handle);

    public float GetValue(float x) => JPH_LinearCurve_GetValue(Handle, x);

    public Vector2 GetPoint(int index)
    {
        JPH_LinearCurve_GetPoint(Handle, index, out Vector2 result);
        return result;
    }
    public void GetPoint(int index, out Vector2 result)
    {
        JPH_LinearCurve_GetPoint(Handle, index, out result);
    }

    public unsafe void GetPoints(Span<Vector2> points)
    {
        JPH_LinearCurve_GetPoints(Handle, null, out int count);
        if (points.Length < count)
        {
            throw new ArgumentException($"The length of the span ({points.Length}) is less than the number of points in the curve ({count}).", nameof(points));
        }

        fixed (Vector2* pointsPtr = points)
        {
            JPH_LinearCurve_GetPoints(Handle, pointsPtr, out _);
        }
    }

    internal static LinearCurve? GetObject(nint handle) => GetOrAddObject(handle, h => new LinearCurve(h, false));
}
