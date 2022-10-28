// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class MeshShapeSettings : ConvexShapeSettings
{
    public unsafe MeshShapeSettings(Triangle* triangles, int triangleCount)
       : base(JPH_MeshShapeSettings_Create(triangles, triangleCount))
    {
    }

    public unsafe MeshShapeSettings(Vector3* vertices, int verticesCount, IndexedTriangle* triangles, int triangleCount)
      : base(JPH_MeshShapeSettings_Create2(vertices, verticesCount, triangles, triangleCount))
    {
    }

    public MeshShapeSettings(Triangle[] triangles)
        : this(triangles.AsSpan())
    {
    }

    public unsafe MeshShapeSettings(ReadOnlySpan<Triangle> triangles)
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

    public unsafe MeshShapeSettings(ReadOnlySpan<Vector3> vertices, ReadOnlySpan<IndexedTriangle> triangles)
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
    ~MeshShapeSettings() => Dispose(isDisposing: false);

    public void Sanitize() => JPH_MeshShapeSettings_Sanitize(Handle);
}
