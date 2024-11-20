// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed unsafe class ConvexHullShapeSettings : ConvexShapeSettings
{
    public ConvexHullShapeSettings(Vector3* points, int pointsCount, float maxConvexRadius = Foundation.DefaultConvexRadius)
       : base(JPH_ConvexHullShapeSettings_Create(points, pointsCount, maxConvexRadius))
    {
    }

    public ConvexHullShapeSettings(Vector3[] points, float maxConvexRadius = Foundation.DefaultConvexRadius)
    {
        fixed (Vector3* pointsPtr = points)
        {
            Handle = JPH_ConvexHullShapeSettings_Create(pointsPtr, points.Length, maxConvexRadius);
        }
    }

    public ConvexHullShapeSettings(ReadOnlySpan<Vector3> points, float maxConvexRadius = Foundation.DefaultConvexRadius)
    {
        fixed (Vector3* pointsPtr = points)
        {
            Handle = JPH_ConvexHullShapeSettings_Create(pointsPtr, points.Length, maxConvexRadius);
        }
    }

    public override Shape Create() => new ConvexHullShape(this);
}

public unsafe class ConvexHullShape : ConvexShape
{
    public ConvexHullShape(ConvexHullShapeSettings settings)
        : base(JPH_ConvexHullShapeSettings_CreateShape(settings.Handle))
    {
    }

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

    /// <summary>
    /// Get the vertices indices of a face
    /// </summary>
    /// <param name="faceIndex">Index of the face.</param>
    /// <param name="maxVertices">Maximum number of vertices to return.</param>
    /// <param name="outVertices">
    /// Array of vertices indices, must be at least maxVertices in size, the vertices are returned in counter clockwise order and the positions can be obtained using <see cref="GetPoint(uint)"/>.
    /// </param>
    /// <returns>
    /// Number of vertices in face, if this is bigger than inMaxVertices, not all vertices were retrieved.
    /// </returns>
    public uint GetFaceVertices(uint faceIndex, uint maxVertices, uint* outVertices)
    {
        return JPH_ConvexHullShape_GetFaceVertices(Handle, faceIndex, maxVertices, outVertices);
    }

    public uint GetFaceVertices(uint faceIndex, uint maxVertices, ReadOnlySpan<uint> outVertices)
    {
        fixed (uint* outVerticesPtr = outVertices)
        {
            return JPH_ConvexHullShape_GetFaceVertices(Handle, faceIndex, maxVertices, outVerticesPtr);
        }
    }
}
