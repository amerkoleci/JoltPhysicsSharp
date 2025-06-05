// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class BroadPhaseLayerFilter : NativeObject
{
    private static readonly JPH_BroadPhaseLayerFilter_Procs _procs;
    private readonly nint _listenerUserData;

    static BroadPhaseLayerFilter()
    {
        unsafe
        {
            _procs = new JPH_BroadPhaseLayerFilter_Procs
            {
                ShouldCollide = &ShouldCollideCallback,
            };
            JPH_BroadPhaseLayerFilter_SetProcs(in _procs);
        }
    }

    public BroadPhaseLayerFilter()
    {
        _listenerUserData = DelegateProxies.CreateUserData(this, true);
        Handle = JPH_BroadPhaseLayerFilter_Create(_listenerUserData);
    }

    protected override void DisposeNative()
    {
        DelegateProxies.GetUserData<BroadPhaseLayerFilter>(_listenerUserData, out GCHandle gch);

        JPH_BroadPhaseLayerFilter_Destroy(Handle);
        gch.Free();
    }

    protected abstract bool ShouldCollide(BroadPhaseLayer layer);

    [UnmanagedCallersOnly]
    private static Bool8 ShouldCollideCallback(IntPtr context, BroadPhaseLayer layer)
    {
        BroadPhaseLayerFilter listener = DelegateProxies.GetUserData<BroadPhaseLayerFilter>(context, out _);
        return listener.ShouldCollide(layer);
    }
}
