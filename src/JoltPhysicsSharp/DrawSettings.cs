// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;

namespace JoltPhysicsSharp;

public struct DrawSettings
{
    /// <summary>
    /// Draw the GetSupport() function, used for convex collision detection
    /// </summary>
    public Bool8 DrawGetSupportFunction = false;
    /// <summary>
    /// When drawing the support function, also draw which direction mapped to a specific support point
    /// </summary>
    public Bool8 DrawSupportDirection = false;
    /// <summary>
    /// Draw the faces that were found colliding during collision detection
    /// </summary>
    public Bool8 DrawGetSupportingFace = false;
    /// <summary>
    /// Draw the shapes of all bodies
    /// </summary>
    public Bool8 DrawShape = true;
    /// <summary>
    /// When mDrawShape is true and this is true, the shapes will be drawn in wireframe instead of solid.
    /// </summary>
    public Bool8 DrawShapeWireframe = false;
    /// <summary>
    /// Coloring scheme to use for shapes
    /// </summary>
    public ShapeColor DrawShapeColor = ShapeColor.MotionTypeColor;
    /// <summary>
    /// Draw a bounding box per body
    /// </summary>
    public Bool8 DrawBoundingBox = false;
    /// <summary>
    /// Draw the center of mass for each body
    /// </summary>
    public Bool8 DrawCenterOfMassTransform = false;
    /// <summary>
    /// Draw the world transform (which can be different than the center of mass) for each body
    /// </summary>
    public Bool8 DrawWorldTransform = false;
    /// <summary>
    /// Draw the velocity vector for each body
    /// </summary>
    public Bool8 DrawVelocity = false;
    /// <summary>
    /// Draw the mass and inertia (as the box equivalent) for each body
    /// </summary>
    public Bool8 DrawMassAndInertia = false;
    /// <summary>
    /// Draw stats regarding the sleeping algorithm of each body
    /// </summary>
    public Bool8 DrawSleepStats = false;
    /// <summary>
    /// Draw the vertices of soft bodies
    /// </summary>
    public Bool8 DrawSoftBodyVertices = false;
    /// <summary>
    /// Draw the velocities of the vertices of soft bodies
    /// </summary>
    public Bool8 DrawSoftBodyVertexVelocities = false;
    /// <summary>
    /// Draw the edge constraints of soft bodies
    /// </summary>
    public Bool8 DrawSoftBodyEdgeConstraints = false;
    /// <summary>
    /// Draw the bend constraints of soft bodies
    /// </summary>
    public Bool8 DrawSoftBodyBendConstraints = false;
    /// <summary>
    /// Draw the volume constraints of soft bodies
    /// </summary>
    public Bool8 DrawSoftBodyVolumeConstraints = false;
    /// <summary>
    /// Draw the skin constraints of soft bodies
    /// </summary>
    public Bool8 DrawSoftBodySkinConstraints = false;
    /// <summary>
    /// Draw the LRA constraints of soft bodies
    /// </summary>
    public Bool8 DrawSoftBodyLRAConstraints = false;
    /// <summary>
    /// Draw the predicted bounds of soft bodies
    /// </summary>
    public Bool8 DrawSoftBodyPredictedBounds = false;
    /// <summary>
    /// Coloring scheme to use for soft body constraints
    /// </summary>
    public SoftBodyConstraintColor DrawSoftBodyConstraintColor = SoftBodyConstraintColor.ConstraintType;

    public DrawSettings()
    {

    }
}
