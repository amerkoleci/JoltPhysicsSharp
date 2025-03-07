// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class ShapeFilter : NativeObject
{
    private static readonly JPH_ShapeFilter_Procs _procs;
    private readonly nint _listenerUserData;

    static unsafe ShapeFilter()
    {
        _procs = new JPH_ShapeFilter_Procs
        {
            ShouldCollide = &ShouldCollideCallback,
            ShouldCollide2 = &ShouldCollide2Callback,
        };
        JPH_ShapeFilter_SetProcs(in _procs);
    }

    public unsafe ShapeFilter()
    {
        _listenerUserData = DelegateProxies.CreateUserData(this, true);
        Handle = JPH_ShapeFilter_Create(_listenerUserData);
    }

    public BodyID BodyID2
    {
        get => JPH_ShapeFilter_GetBodyID2(Handle);
        set => JPH_ShapeFilter_SetBodyID2(Handle, value);
    }

    protected override void DisposeNative()
    {
        DelegateProxies.GetUserData<ShapeFilter>(_listenerUserData, out GCHandle gch);
        JPH_ShapeFilter_Destroy(Handle);
        gch.Free();
    }

    protected virtual bool ShouldCollide(Shape shape2, in SubShapeID subShapeIDOfShape2)
    {
        return true;
    }

    protected virtual bool ShouldCollide(Shape shape1, in SubShapeID subShapeIDOfShape1, Shape shape2, in SubShapeID subShapeIDOfShape2)
    {
        return true;
    }

    [UnmanagedCallersOnly]
    private static unsafe Bool8 ShouldCollideCallback(nint context, nint shape2, SubShapeID* subShapeIDOfShape2)
    {
        // TODO: Add cache for Shape
        ShapeFilter listener = DelegateProxies.GetUserData<ShapeFilter>(context, out _);
        return listener.ShouldCollide(Shape.GetObject(shape2)!, *subShapeIDOfShape2);
    }

    [UnmanagedCallersOnly]
    private static unsafe Bool8 ShouldCollide2Callback(nint context, nint shape1, SubShapeID* subShapeIDOfShape1, nint shape2, SubShapeID* subShapeIDOfShape2)
    {
        // TODO: Add cache for Shape
        ShapeFilter listener = DelegateProxies.GetUserData<ShapeFilter>(context, out _);
        return listener.ShouldCollide(Shape.GetObject(shape1)!, *subShapeIDOfShape1, Shape.GetObject(shape2)!, *subShapeIDOfShape2);
    }
}
