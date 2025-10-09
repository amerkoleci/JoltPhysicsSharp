// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class DebugRenderer : NativeObject
{
    private static readonly  JPH_DebugRenderer_Procs _procs;
    private readonly nint _listenerUserData;

    public enum CastShadow : uint
    {
        /// <summary>
        /// This shape should cast a shadow
        /// </summary>
        On,
        /// <summary>
        /// This shape should not cast a shadow
        /// </summary>
        Off
    }

    public enum DrawMode : uint
    {
        /// <summary>
        /// Draw as a solid shape
        /// </summary>
        Solid = 0,
        /// <summary>
        /// Draw as wireframe
        /// </summary>
        Wireframe = 1,
    }

    static unsafe DebugRenderer()
    {
        _procs = new()
        {
            DrawLine = &OnDrawLine,
            DrawTriangle = &OnDrawTriangle,
            DrawText3D = &OnDrawText3D,
        };
        JPH_DebugRenderer_SetProcs(in _procs);
    }

    protected DebugRenderer()
    {
        _listenerUserData = DelegateProxies.CreateUserData(this, true);
        Handle = JPH_DebugRenderer_Create(_listenerUserData);
    }

    protected DebugRenderer(nint handle)
        : base(handle)
    {
    }

    protected override void DisposeNative()
    {
        DelegateProxies.GetUserData<DebugRenderer>(_listenerUserData, out GCHandle gch);
        JPH_DebugRenderer_Destroy(Handle);
        gch.Free();
    }

    public void NextFrame() => JPH_DebugRenderer_NextFrame(Handle);

    public void SetCameraPosition(in Vector3 value) => JPH_DebugRenderer_SetCameraPos(Handle, value);

    public void DrawLine(in Vector3 from, in Vector3 to, JoltColor color)
    {
        JPH_DebugRenderer_DrawLine(Handle, from, to, color.PackedValue);
    }

    public void DrawWireBox(in BoundingBox box, JoltColor color)
    {
        JPH_DebugRenderer_DrawWireBox(Handle, in box, color.PackedValue);
    }

    public void DrawWireBox(in Matrix4x4 matrix, in BoundingBox box, JoltColor color)
    {
        JPH_DebugRenderer_DrawWireBox2(Handle, matrix, in box, color.PackedValue);
    }

    /// <summary>
    /// Draw a marker on a position
    /// </summary>
    /// <param name="position"></param>
    /// <param name="color"></param>
    /// <param name="size"></param>
    public void DrawMarker(in Vector3 position, JoltColor color, float size)
    {
        JPH_DebugRenderer_DrawMarker(Handle, position, color.PackedValue, size);
    }

    /// <summary>
    /// Draw an arrow
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="color"></param>
    /// <param name="size"></param>
    public void DrawArrow(in Vector3 from, in Vector3 to, JoltColor color, float size)
    {
        JPH_DebugRenderer_DrawArrow(Handle, from, to, color.PackedValue, size);
    }

    /// <summary>
    /// Draw coordinate system (3 arrows, x = red, y = green, z = blue)
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="size"></param>
	public void DrawCoordinateSystem(in Matrix4x4 transform, float size = 1.0f)
    {
        JPH_DebugRenderer_DrawCoordinateSystem(Handle, transform.ToJolt(), size);
    }

    /// <summary>
    /// Draw a plane through inPoint with normal inNormal
    /// </summary>
    /// <param name="point"></param>
    /// <param name="normal"></param>
    /// <param name="color"></param>
    /// <param name="size"></param>
    public void DrawPlane(in Vector3 point, in Vector3 normal, JoltColor color, float size)
    {
        JPH_DebugRenderer_DrawPlane(Handle, point, normal, color.PackedValue, size);
    }

    /// <summary>
    /// Draw wireframe triangle
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="v3"></param>
    /// <param name="color"></param>
    public void DrawWireTriangle(in Vector3 v1, in Vector3 v2, in Vector3 v3, JoltColor color)
    {
        JPH_DebugRenderer_DrawWireTriangle(Handle, v1, v2, v3, color.PackedValue);
    }

    /// Draw wireframe sphere
	public void DrawWireSphere(in Vector3 center, float radius, JoltColor color, int level = 3)
    {
        JPH_DebugRenderer_DrawWireSphere(Handle, center, radius, color.PackedValue, level);
    }

    public void DrawWireUnitSphere(in Matrix4x4 matrix, JoltColor color, int level = 3)
    {
        JPH_DebugRenderer_DrawWireUnitSphere(Handle, matrix.ToJolt(), color.PackedValue, level);
    }

    /// Draw a box
	public void DrawBox(in BoundingBox box, JoltColor color, CastShadow castShadow = CastShadow.On, DrawMode drawMode = DrawMode.Solid)
    {
        JPH_DebugRenderer_DrawBox(Handle, in box, color.PackedValue, castShadow, drawMode);
    }

    public void DrawBox(in Matrix4x4 matrix, in BoundingBox box, JoltColor color, CastShadow castShadow = CastShadow.On, DrawMode drawMode = DrawMode.Solid)
    {
        JPH_DebugRenderer_DrawBox2(Handle, matrix.ToJolt(), in box, color.PackedValue, castShadow, drawMode);
    }

    /// Draw a sphere
    public void DrawSphere(in Vector3 center, float radius, JoltColor color, CastShadow castShadow = CastShadow.On, DrawMode drawMode = DrawMode.Solid)
    {
        JPH_DebugRenderer_DrawSphere(Handle, center, radius, color.PackedValue, castShadow, drawMode);
    }

    public void DrawUnitSphere(in Matrix4x4 matrix, JoltColor color, CastShadow castShadow = CastShadow.On, DrawMode drawMode = DrawMode.Solid)
    {
        JPH_DebugRenderer_DrawUnitSphere(Handle, matrix.ToJolt(), color.PackedValue, castShadow, drawMode);
    }

    /// <summary>
    /// Draw a capsule with one half sphere at (0, -halfHeightOfCylinder, 0) and the other half sphere at (0, halfHeightOfCylinder, 0) and radius inRadius.
    /// The capsule will be transformed by inMatrix.
    /// </summary>
    /// <param name="matrix"></param>
    /// <param name="halfHeightOfCylinder"></param>
    /// <param name="radius"></param>
    /// <param name="color"></param>
    /// <param name="castShadow"></param>
    /// <param name="drawMode"></param>
    public void DrawCapsule(in Matrix4x4 matrix, float halfHeightOfCylinder, float radius, JoltColor color, CastShadow castShadow = CastShadow.On, DrawMode drawMode = DrawMode.Solid)
    {
        JPH_DebugRenderer_DrawCapsule(Handle, matrix.ToJolt(), halfHeightOfCylinder, radius, color.PackedValue, castShadow, drawMode);
    }

    /// <summary>
    /// Draw a cylinder with top (0, halfHeight, 0) and bottom (0, -halfHeight, 0) and radius.
    /// The cylinder will be transformed by matrix.
    /// </summary>
    /// <param name="matrix"></param>
    /// <param name="halfHeight"></param>
    /// <param name="radius"></param>
    /// <param name="color"></param>
    /// <param name="castShadow"></param>
    /// <param name="drawMode"></param>
    public void DrawCylinder(in Matrix4x4 matrix, float halfHeight, float radius, JoltColor color, CastShadow castShadow = CastShadow.On, DrawMode drawMode = DrawMode.Solid)
    {
        JPH_DebugRenderer_DrawCylinder(Handle, matrix.ToJolt(), halfHeight, radius, color.PackedValue, castShadow, drawMode);
    }

    /// <summary>
    /// Draw a bottomless cone.
    /// </summary>
    /// <param name="top">Top of cone, center of base is at top + axis.</param>
    /// <param name="axis">Height and direction of cone.</param>
    /// <param name="perpendicular">Perpendicular vector to axis.</param>
    /// <param name="halfAngle">Specifies the cone angle in radians (angle measured between axis and cone surface).</param>
    /// <param name="length">The length of the cone.</param>
    /// <param name="color">Color to use for drawing the cone.</param>
    /// <param name="castShadow">Determines if this geometry should cast a shadow or not.</param>
    /// <param name="drawMode">Determines if we draw the geometry solid or in wireframe.</param>
    public void DrawOpenCone(in Vector3 top, in Vector3 axis, in Vector3 perpendicular, float halfAngle, float length, JoltColor color, CastShadow castShadow = CastShadow.On, DrawMode drawMode = DrawMode.Solid)
    {
        JPH_DebugRenderer_DrawOpenCone(Handle, top, axis, perpendicular, halfAngle, length, color.PackedValue, castShadow, drawMode);
    }

    /// <summary>
    /// Draws cone rotation limits as used by the SwingTwistConstraintPart.
    /// </summary>
    /// <param name="matrix">Matrix that transforms from constraint space to world space</param>
    /// <param name="swingYHalfAngle">See SwingTwistConstraintPart</param>
    /// <param name="swingZHalfAngle">See SwingTwistConstraintPart</param>
    /// <param name="edgeLength">Size of the edge of the cone shape</param>
    /// <param name="color">Color to use for drawing the cone.</param>
    /// <param name="castShadow">Determines if this geometry should cast a shadow or not.</param>
    /// <param name="drawMode">Determines if we draw the geometry solid or in wireframe.</param>
    public void DrawSwingConeLimits(in Matrix4x4 matrix, float swingYHalfAngle, float swingZHalfAngle, float edgeLength, JoltColor color, CastShadow castShadow = CastShadow.On, DrawMode drawMode = DrawMode.Solid)
    {
        JPH_DebugRenderer_DrawSwingConeLimits(Handle, matrix.ToJolt(), swingYHalfAngle, swingZHalfAngle, edgeLength, color.PackedValue, castShadow, drawMode);
    }

    /// <summary>
    /// Draws rotation limits as used by the SwingTwistConstraintPart.
    /// </summary>
    /// <param name="matrix">Matrix that transforms from constraint space to world space</param>
    /// <param name="minSwingYAngle">See SwingTwistConstraintPart</param>
    /// <param name="maxSwingYAngle">See SwingTwistConstraintPart</param>
    /// <param name="minSwingZAngle">See SwingTwistConstraintPart</param>
    /// <param name="maxSwingZAngle">See SwingTwistConstraintPart</param>
    /// <param name="edgeLength">Size of the edge of the cone shape</param>
    /// <param name="color">Color to use for drawing the pyramid.</param>
    /// <param name="castShadow">Determines if this geometry should cast a shadow or not.</param>
    /// <param name="drawMode">Determines if we draw the geometry solid or in wireframe.</param>
    public void DrawSwingPyramidLimits(in Matrix4x4 matrix, float minSwingYAngle, float maxSwingYAngle, float minSwingZAngle, float maxSwingZAngle, float edgeLength, JoltColor color, CastShadow castShadow = CastShadow.On, DrawMode drawMode = DrawMode.Solid)
    {
        JPH_DebugRenderer_DrawSwingPyramidLimits(Handle, matrix.ToJolt(), minSwingYAngle, maxSwingYAngle, minSwingZAngle, maxSwingZAngle, edgeLength, color.PackedValue, castShadow, drawMode);
    }

    /// <summary>
    /// Draw a pie (part of a circle).
    /// </summary>
    /// <param name="center">The center of the circle.</param>
    /// <param name="radius">Radius of the circle.</param>
    /// <param name="normal">The plane normal in which the pie resides.</param>
    /// <param name="axis">The axis that defines an angle of 0 radians.</param>
    /// <param name="minAngle">The pie will be drawn between [inMinAngle, inMaxAngle] (in radians).</param>
    /// <param name="maxAngle">The pie will be drawn between [inMinAngle, inMaxAngle] (in radians).</param>
    /// <param name="color">Color to use for drawing the pie.</param>
    /// <param name="castShadow">Determines if this geometry should cast a shadow or not.</param>
    /// <param name="drawMode">Determines if we draw the geometry solid or in wireframe.</param>
    public void DrawPie(in Vector3 center, float radius, in Vector3 normal, in Vector3 axis, float minAngle, float maxAngle, JoltColor color, CastShadow castShadow = CastShadow.On, DrawMode drawMode = DrawMode.Solid)
    {
        JPH_DebugRenderer_DrawPie(Handle, center, radius, normal, axis, minAngle, maxAngle, color.PackedValue, castShadow, drawMode);
    }

    /// <summary>
    /// Draw a tapered cylinder
    /// </summary>
    /// <param name="matrix">Matrix that transforms the cylinder to world space.</param>
    /// <param name="top">Top of cylinder (along Y axis)</param>
    /// <param name="bottom">Bottom of cylinder (along Y axis)</param>
    /// <param name="topRadius">Radius at the top</param>
    /// <param name="bottomRadius">Radius at the bottom</param>
    /// <param name="color">Color to use for drawing the pie.</param>
    /// <param name="castShadow">Determines if this geometry should cast a shadow or not.</param>
    /// <param name="drawMode">Determines if we draw the geometry solid or in wireframe.</param>
    public void DrawTaperedCylinder(in Matrix4x4 matrix, float top, float bottom, float topRadius, float bottomRadius, JoltColor color, CastShadow castShadow = CastShadow.On, DrawMode drawMode = DrawMode.Solid)
    {
        JPH_DebugRenderer_DrawTaperedCylinder(Handle, matrix.ToJolt(), top, bottom, topRadius, bottomRadius, color.PackedValue, castShadow, drawMode);
    }


    protected abstract void DrawLine(Vector3 from, Vector3 to, JoltColor color);

    protected virtual void DrawTriangle(Vector3 v1, Vector3 v2, Vector3 v3, JoltColor color, CastShadow castShadow = CastShadow.Off)
    {

    }

    protected abstract void DrawText3D(Vector3 position, string? text, JoltColor color, float height = 0.5f);

    #region DebugRendererListener
    [UnmanagedCallersOnly]
    private static unsafe void OnDrawLine(nint context, Vector3* from, Vector3* to, uint color)
    {
        DebugRenderer listener = DelegateProxies.GetUserData<DebugRenderer>(context, out _);

        listener.DrawLine(*from, *to, color);
    }

    [UnmanagedCallersOnly]
    private static unsafe void OnDrawTriangle(nint context, Vector3* v1, Vector3* v2, Vector3* v3, uint color, CastShadow castShadow)
    {
        DebugRenderer listener = DelegateProxies.GetUserData<DebugRenderer>(context, out _);

        listener.DrawTriangle(*v1, *v2, *v3, color, castShadow);
    }

    [UnmanagedCallersOnly]
    private static unsafe void OnDrawText3D(nint context, Vector3* position, byte* textPtr, uint color, float height)
    {
        DebugRenderer listener = DelegateProxies.GetUserData<DebugRenderer>(context, out _);

        listener.DrawText3D(*position, ConvertToManaged(textPtr), color, height);
    }
    #endregion
}
