// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class ConvexHullShapeSettings : ConvexShapeSettings
{
    public unsafe ConvexHullShapeSettings(Vector3* points, int pointsCount, float maxConvexRadius = Foundation.DefaultConvexRadius)
       : base(JPH_ConvexHullShapeSettings_Create(points, pointsCount, maxConvexRadius))
    {
    }

    public unsafe ConvexHullShapeSettings(Vector3[] points, float maxConvexRadius = Foundation.DefaultConvexRadius)
    {
        fixed (Vector3* pointsPtr = points)
        {
            Handle = JPH_ConvexHullShapeSettings_Create(pointsPtr, points.Length, maxConvexRadius);
        }
    }

    public unsafe ConvexHullShapeSettings(ReadOnlySpan<Vector3> points, float maxConvexRadius = Foundation.DefaultConvexRadius)
    {
        fixed (Vector3* pointsPtr = points)
        {
            Handle = JPH_ConvexHullShapeSettings_Create(pointsPtr, points.Length, maxConvexRadius);
        }
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="ConvexHullShapeSettings" /> class.
    /// </summary>
    ~ConvexHullShapeSettings() => Dispose(disposing: false);
}

public unsafe class ConvexHullShape : ConvexShape
{
    public ConvexHullShape(ConvexHullShapeSettings settings)
        : base(JPH_ConvexHullShapeSettings_CreateShape(settings.Handle))
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="ConvexHullShape" /> class.
    /// </summary>
    ~ConvexHullShape() => Dispose(isDisposing: false);

    public uint GetNumPoints()
    {
        return JPH_ConvexHullShape_GetNumPoints(Handle);
    }

    public Vector3 GetPoint(uint index)
    {
        Vector3 result;
        JPH_ConvexHullShape_GetPoint(Handle, index, &result);
        return result;
    }

    public void GetPoint(uint index, out Vector3 result)
    {
        Unsafe.SkipInit(out result);

        fixed (Vector3* resultPtr = &result)
            JPH_ConvexHullShape_GetPoint(Handle, index, resultPtr);
    }

    public uint GetNumFaces()
    {
        return JPH_ConvexHullShape_GetNumFaces(Handle);
    }

    public uint GetNumVerticesInFace(uint faceIndex)
    {
        return JPH_ConvexHullShape_GetNumVerticesInFace(Handle, faceIndex);
    }

    public uint GetNumVerticesInFace(uint faceIndex)
    {
        return JPH_ConvexHullShape_GetFaceVertices(Handle, faceIndex);
    }
}
