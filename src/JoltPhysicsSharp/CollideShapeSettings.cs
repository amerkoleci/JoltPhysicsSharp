// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct CollideShapeSettings
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
    /// When > 0 contacts in the vicinity of the query shape can be found. All nearest contacts that are not further away than this distance will be found (unit: meter)
    /// </summary>
	public float MaxSeparationDistance;

    /// <summary>
    /// How backfacing triangles should be treated
    /// </summary>
    public BackFaceMode BackFaceMode;

    public unsafe CollideShapeSettings()
    {
        JPH_CollideShapeSettings native;
        JPH_CollideShapeSettings_Init(&native);

        ActiveEdgeMode = native.@base.activeEdgeMode;
        CollectFacesMode = native.@base.collectFacesMode;
        CollisionTolerance = native.@base.collisionTolerance;
        PenetrationTolerance = native.@base.penetrationTolerance;
        ActiveEdgeMovementDirection = native.@base.activeEdgeMovementDirection;

        MaxSeparationDistance = native.maxSeparationDistance;
        BackFaceMode = native.backFaceMode;
    }

    internal static CollideShapeSettings FromNative(in JPH_CollideShapeSettings native)
    {
        CollideShapeSettings result = default;
        result.ActiveEdgeMode = native.@base.activeEdgeMode;
        result.CollectFacesMode = native.@base.collectFacesMode;
        result.CollisionTolerance = native.@base.collisionTolerance;
        result.PenetrationTolerance = native.@base.penetrationTolerance;
        result.ActiveEdgeMovementDirection = native.@base.activeEdgeMovementDirection;
        result.MaxSeparationDistance = native.maxSeparationDistance;
        result.BackFaceMode = native.backFaceMode;
        return result;
    }

    internal readonly unsafe void ToNative(JPH_CollideShapeSettings* native)
    {
        native->@base.activeEdgeMode = ActiveEdgeMode;
        native->@base.collectFacesMode = CollectFacesMode;
        native->@base.collisionTolerance = CollisionTolerance;
        native->@base.penetrationTolerance = PenetrationTolerance;
        native->@base.activeEdgeMovementDirection = ActiveEdgeMovementDirection;

        native->maxSeparationDistance = MaxSeparationDistance;
        native->backFaceMode = BackFaceMode;
    }
}
