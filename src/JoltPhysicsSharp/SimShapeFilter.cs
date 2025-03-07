// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class SimShapeFilter : NativeObject
{
    private static readonly JPH_SimShapeFilter_Procs _procs;
    private readonly nint _listenerUserData;

    static unsafe SimShapeFilter()
    {
        _procs = new JPH_SimShapeFilter_Procs
        {
            ShouldCollide = &ShouldCollideCallback
        };
        JPH_SimShapeFilter_SetProcs(in _procs);
    }

    public SimShapeFilter()
    {
        _listenerUserData = DelegateProxies.CreateUserData(this, true);
        Handle = JPH_SimShapeFilter_Create(_listenerUserData);
    }

    protected override void DisposeNative()
    {
        DelegateProxies.GetUserData<SimShapeFilter>(_listenerUserData, out GCHandle gch);
        JPH_SimShapeFilter_Destroy(Handle);
        gch.Free();
    }

    protected virtual bool ShouldCollide(
        Body body1, Shape shape1, in SubShapeID subShapeIDOfShape1,
        Body inBody2, Shape shape2, in SubShapeID subShapeIDOfShape2)
    {
        return true;
    }

    [UnmanagedCallersOnly]
    private static unsafe Bool8 ShouldCollideCallback(nint context,
        nint body1, nint shape1, SubShapeID* subShapeIDOfShape1,
        nint body2, nint shape2, SubShapeID* subShapeIDOfShape2)
    {
        SimShapeFilter listener = DelegateProxies.GetUserData<SimShapeFilter>(context, out _);
        return listener.ShouldCollide(
            Body.GetObject(body1)!, Shape.GetObject(shape1)!, *subShapeIDOfShape1,
            Body.GetObject(body2)!, Shape.GetObject(shape2)!, *subShapeIDOfShape2);
    }
}
