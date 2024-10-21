// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class DebugRenderer : NativeObject
{
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


    private readonly unsafe JPH_DebugRenderer_Procs _listener_Procs = new()
    {
        DrawLine = &OnDrawLine,
        DrawTriangle = &OnDrawTriangle,
        DrawText3D = &OnDrawText3D,
    };

    protected DebugRenderer()
    {
        nint listenerContext = DelegateProxies.CreateUserData(this, true);
        Handle = JPH_DebugRenderer_Create(_listener_Procs, listenerContext);
    }

    protected DebugRenderer(nint handle)
        : base(handle)
    {
    }

    ~DebugRenderer() => Dispose(disposing: false);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            JPH_DebugRenderer_Destroy(Handle);
        }
    }

    public void NextFrame() => JPH_DebugRenderer_NextFrame(Handle);

    public void DrawWireBox(in BoundingBox box, Color color)
    {
        JPH_DebugRenderer_DrawWireBox(Handle, in box, color.PackedValue);
    }

    public void DrawWireBox(in Matrix4x4 matrix, in BoundingBox box, Color color)
    {
        JPH_DebugRenderer_DrawWireBox2(Handle, matrix, in box, color.PackedValue);
    }

    public void DrawWireBox(in RMatrix4x4 matrix, in BoundingBox box, Color color)
    {
        JPH_DebugRenderer_DrawWireBox2(Handle, matrix, in box, color.PackedValue);
    }

    protected abstract void DrawLine(Vector3 from, Vector3 to, Color color);

    protected virtual void DrawTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Color color, CastShadow castShadow = CastShadow.Off)
    {

    }

    protected abstract void DrawText3D(Vector3 position, string? text, Color color, float height = 0.5f);

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
