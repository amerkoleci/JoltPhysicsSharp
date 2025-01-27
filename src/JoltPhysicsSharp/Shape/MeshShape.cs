// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public enum MeshShapeBuildQuality
{
    /// <summary>
    /// Favor runtime performance, takes more time to build the MeshShape but performs better
    /// </summary>
    FavorRuntimePerformance,
    /// <summary>
    /// Favor build speed, build the tree faster but the <see cref="MeshShape"/> will be slower
    /// </summary>
    FavorBuildSpeed
}

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

    public MeshShapeSettings(Span<Triangle> triangles)
    {
        fixed (Triangle* trianglePtr = triangles)
        {
            Handle = JPH_MeshShapeSettings_Create(trianglePtr, triangles.Length);
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

    public uint MaxTrianglesPerLeaf
    {
        get => JPH_MeshShapeSettings_GetMaxTrianglesPerLeaf(Handle);
        set => JPH_MeshShapeSettings_SetMaxTrianglesPerLeaf(Handle, value);
    }

    public float ActiveEdgeCosThresholdAngle
    {
        get => JPH_MeshShapeSettings_GetActiveEdgeCosThresholdAngle(Handle);
        set => JPH_MeshShapeSettings_SetActiveEdgeCosThresholdAngle(Handle, value);
    }

    public bool PerTriangleUserData
    {
        get => JPH_MeshShapeSettings_GetPerTriangleUserData(Handle);
        set => JPH_MeshShapeSettings_SetPerTriangleUserData(Handle, value);
    }

    public MeshShapeBuildQuality BuildQuality
    {
        get => JPH_MeshShapeSettings_GetBuildQuality(Handle);
        set => JPH_MeshShapeSettings_SetBuildQuality(Handle, value);
    }

    public override Shape Create() => new MeshShape(this);

    public void Sanitize() => JPH_MeshShapeSettings_Sanitize(Handle);
}

public sealed class MeshShape : Shape
{
    public MeshShape(in MeshShapeSettings settings)
        : base(JPH_MeshShapeSettings_CreateShape(settings.Handle))
    {
    }

    public uint GetTriangleUserData(uint triangleIndex) => JPH_MeshShape_GetTriangleUserData(Handle, triangleIndex);
}
