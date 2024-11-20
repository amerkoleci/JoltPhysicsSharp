// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class ShapeFilter : NativeObject
{
    private readonly JPH_ShapeFilter_Procs _bodyFilter_Procs;

    public unsafe ShapeFilter()
    {
        nint context = DelegateProxies.CreateUserData(this, true);
        _bodyFilter_Procs = new JPH_ShapeFilter_Procs
        {
            ShouldCollide = &ShouldCollideCallback,
            ShouldCollide2 = &ShouldCollide2Callback,
        };
        Handle = JPH_ShapeFilter_Create(_bodyFilter_Procs, context);
    }

    public BodyID BodyID2
    {
        get => JPH_ShapeFilter_GetBodyID2(Handle);
        set => JPH_ShapeFilter_SetBodyID2(Handle, value);
    }

    protected override void DisposeNative()
    {
        JPH_ShapeFilter_Destroy(Handle);
    }

    protected virtual bool ShouldCollide(/*Shape*/nint shape2, in SubShapeID subShapeIDOfShape2)
    {
        return true;
    }

    protected virtual bool ShouldCollide(/*Shape*/nint shape1, in SubShapeID subShapeIDOfShape1, /*Shape*/nint shape2, in SubShapeID subShapeIDOfShape2)
    {
        return true;
    }

    [UnmanagedCallersOnly]
    private static unsafe Bool8 ShouldCollideCallback(nint context, nint shape2, SubShapeID* subShapeIDOfShape2)
    {
        // TODO: Add cache for Shape
        ShapeFilter listener = DelegateProxies.GetUserData<ShapeFilter>(context, out _);
        //return listener.ShouldCollide(new Shape(shape2), *subShapeIDOfShape2);
        return listener.ShouldCollide(shape2, *subShapeIDOfShape2);
    }

    [UnmanagedCallersOnly]
    private static unsafe Bool8 ShouldCollide2Callback(nint context, nint shape1, SubShapeID* subShapeIDOfShape1, nint shape2, SubShapeID* subShapeIDOfShape2)
    {
        // TODO: Add cache for Shape
        ShapeFilter listener = DelegateProxies.GetUserData<ShapeFilter>(context, out _);
        return listener.ShouldCollide(shape1, *subShapeIDOfShape1, shape2, *subShapeIDOfShape2);
    }
}
