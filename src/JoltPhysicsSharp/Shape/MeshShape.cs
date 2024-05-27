// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed unsafe class MeshShapeSettings : ConvexShapeSettings
{
    public MeshShapeSettings(Triangle* triangles, int triangleCount)
       : base(JPH_MeshShapeSettings_Create(triangles, triangleCount))
    {
    }

    public MeshShapeSettings(Vector3* vertices, int verticesCount, IndexedTriangle* triangles, int triangleCount)
      : base(JPH_MeshShapeSettings_Create2(vertices, verticesCount, triangles, triangleCount))
    {
    }

    public MeshShapeSettings(Triangle[] triangles)
        : this(triangles.AsSpan())
    {
    }

    public MeshShapeSettings(ReadOnlySpan<Triangle> triangles)
    {
        fixed (Triangle* trianglePtr = triangles)
        {
            Handle = JPH_MeshShapeSettings_Create(trianglePtr, triangles.Length);
        }
    }

    public MeshShapeSettings(Vector3[] vertices, IndexedTriangle[] triangles)
        : this(vertices.AsSpan(), triangles.AsSpan())
    {
    }

    public MeshShapeSettings(ReadOnlySpan<Vector3> vertices, ReadOnlySpan<IndexedTriangle> triangles)
    {
        fixed (Vector3* verticesPtr = vertices)
        fixed (IndexedTriangle* trianglePtr = triangles)
        {
            Handle = JPH_MeshShapeSettings_Create2(verticesPtr, vertices.Length, trianglePtr, triangles.Length);
        }
    }

    public MeshShapeSettings(Span<Vector3> vertices, Span<IndexedTriangle> triangles)
    {
        fixed (Vector3* verticesPtr = vertices)
        fixed (IndexedTriangle* trianglePtr = triangles)
        {
            Handle = JPH_MeshShapeSettings_Create2(verticesPtr, vertices.Length, trianglePtr, triangles.Length);
        }
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="TriangleShapeSettings" /> class.
    /// </summary>
    ~MeshShapeSettings() => Dispose(disposing: false);

    public override Shape Create() => new MeshShape(this);

    public void Sanitize() => JPH_MeshShapeSettings_Sanitize(Handle);
}

public sealed class MeshShape : Shape
{
    public MeshShape(in MeshShapeSettings settings)
        : base(JPH_MeshShapeSettings_CreateShape(settings.Handle))
    {
    }
}
