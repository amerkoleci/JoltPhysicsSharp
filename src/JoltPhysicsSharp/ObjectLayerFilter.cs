// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class ObjectLayerFilter : NativeObject
{
    private static readonly JPH_ObjectLayerFilter_Procs _procs;
    private readonly nint _listenerUserData;

    static ObjectLayerFilter()
    {
        _procs = new JPH_ObjectLayerFilter_Procs
        {
            ShouldCollide = &ShouldCollideCallback,
        };
        JPH_ObjectLayerFilter_SetProcs(in _procs);
    }

    public ObjectLayerFilter()
    {
        _listenerUserData = DelegateProxies.CreateUserData(this, true);
        Handle = JPH_ObjectLayerFilter_Create(_listenerUserData);
    }

    protected override void DisposeNative()
    {
        DelegateProxies.GetUserData<ObjectLayerFilter>(_listenerUserData, out GCHandle gch);

        JPH_ObjectLayerFilter_Destroy(Handle);
        gch.Free();
    }

    protected abstract bool ShouldCollide(ObjectLayer layer);

    [UnmanagedCallersOnly]
    private static Bool8 ShouldCollideCallback(IntPtr context, ObjectLayer layer)
    {
        ObjectLayerFilter listener = DelegateProxies.GetUserData<ObjectLayerFilter>(context, out _);
        return listener.ShouldCollide(layer);
    }
}
