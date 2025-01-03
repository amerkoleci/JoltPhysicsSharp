// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public unsafe struct ShapeCastSettings
{
    // CollideSettingsBase
    /// <summary>
    /// How active edges (edges that a moving object should bump into) are handled
    /// </summary>
	public ActiveEdgeMode ActiveEdgeMode;

    /// <summary>
    /// If colliding faces should be collected or only the collision point
    /// </summary>
    public CollectFacesMode CollectFacesMode;

    /// <summary>
    /// If objects are closer than this distance, they are considered to be colliding (used for GJK) (unit: meter)
    /// </summary>
    public float CollisionTolerance;

    /// <summary>
    /// A factor that determines the accuracy of the penetration depth calculation. If the change of the squared distance is less than tolerance * current_penetration_depth^2 the algorithm will terminate. (unit: dimensionless)
    /// </summary>
    public float PenetrationTolerance;

    /// <summary>
    /// When <see cref="ActiveEdgeMode"/> is <see cref="ActiveEdgeMode.CollideOnlyWithActive"/> a movement direction can be provided. When hitting an inactive edge, the system will select the triangle normal as penetration depth only if it impedes the movement less than with the calculated penetration depth.
    /// </summary>
    public Vector3 ActiveEdgeMovementDirection;

    /// <summary>
    /// How backfacing triangles should be treated (should we report moving from back to front for triangle based shapes, e.g. for MeshShape/HeightFieldShape?)
    /// </summary>
	public BackFaceMode BackFaceModeTriangles;

    /// <summary>
    /// How backfacing convex objects should be treated (should we report starting inside an object and moving out?)
    /// </summary>
    public BackFaceMode BackFaceModeConvex;

    /// <summary>
    /// Indicates if we want to shrink the shape by the convex radius and then expand it again. This speeds up collision detection and gives a more accurate normal at the cost of a more 'rounded' shape.
    /// </summary>
    public bool UseShrunkenShapeAndConvexRadius;

    /// <summary>
    /// When true, and the shape is intersecting at the beginning of the cast (fraction = 0) then this will calculate the deepest penetration point (costing additional CPU time)
    /// </summary>
    public bool ReturnDeepestPoint;

    public ShapeCastSettings()
    {
        JPH_ShapeCastSettings native;
        JPH_ShapeCastSettings_Init(&native);

        ActiveEdgeMode = native.@base.activeEdgeMode;
        CollectFacesMode = native.@base.collectFacesMode;
        CollisionTolerance = native.@base.collisionTolerance;
        PenetrationTolerance = native.@base.penetrationTolerance;
        ActiveEdgeMovementDirection = native.@base.activeEdgeMovementDirection;

        BackFaceModeTriangles = native.backFaceModeTriangles;
        BackFaceModeConvex = native.backFaceModeConvex;
        UseShrunkenShapeAndConvexRadius = native.useShrunkenShapeAndConvexRadius;
        ReturnDeepestPoint = native.returnDeepestPoint;
    }

    internal static ShapeCastSettings FromNative(in JPH_ShapeCastSettings native)
    {
        ShapeCastSettings result = default;
        result.ActiveEdgeMode = native.@base.activeEdgeMode;
        result.CollectFacesMode = native.@base.collectFacesMode;
        result.CollisionTolerance = native.@base.collisionTolerance;
        result.PenetrationTolerance = native.@base.penetrationTolerance;
        result.ActiveEdgeMovementDirection = native.@base.activeEdgeMovementDirection;

        result.BackFaceModeTriangles = native.backFaceModeTriangles;
        result.BackFaceModeConvex = native.backFaceModeConvex;
        result.UseShrunkenShapeAndConvexRadius = native.useShrunkenShapeAndConvexRadius;
        result.ReturnDeepestPoint = native.returnDeepestPoint;
        return result;
    }

    internal readonly void ToNative(JPH_ShapeCastSettings* native)
    {
        native->@base.activeEdgeMode = ActiveEdgeMode;
        native->@base.collectFacesMode = CollectFacesMode;
        native->@base.collisionTolerance = CollisionTolerance;
        native->@base.penetrationTolerance = PenetrationTolerance;
        native->@base.activeEdgeMovementDirection = ActiveEdgeMovementDirection;

        native->backFaceModeTriangles = BackFaceModeTriangles;
        native->backFaceModeConvex = BackFaceModeConvex;
        native->useShrunkenShapeAndConvexRadius = UseShrunkenShapeAndConvexRadius;
        native->returnDeepestPoint = ReturnDeepestPoint;
    }
}
