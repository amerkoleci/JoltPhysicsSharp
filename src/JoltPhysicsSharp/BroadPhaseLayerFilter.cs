// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class BroadPhaseLayerFilter : NativeObject
{
    private readonly JPH_BroadPhaseLayerFilter_Procs _procs;

    public BroadPhaseLayerFilter()
    {
        nint listenerContext = DelegateProxies.CreateUserData(this, true);
        _procs = new JPH_BroadPhaseLayerFilter_Procs
        {
            ShouldCollide = &ShouldCollideCallback,
        };
        Handle = JPH_BroadPhaseLayerFilter_Create(in _procs, listenerContext);
    }

    protected override void DisposeNative()
    {
        JPH_BroadPhaseLayerFilter_Destroy(Handle);
    }

    protected abstract bool ShouldCollide(BroadPhaseLayer layer);

    [UnmanagedCallersOnly]
    private static Bool8 ShouldCollideCallback(IntPtr context, BroadPhaseLayer layer)
    {
        BroadPhaseLayerFilter listener = DelegateProxies.GetUserData<BroadPhaseLayerFilter>(context, out _);
        return listener.ShouldCollide(layer);
    }
}
