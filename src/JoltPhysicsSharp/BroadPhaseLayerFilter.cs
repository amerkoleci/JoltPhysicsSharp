// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class BroadPhaseLayerFilter : NativeObject
{
    private readonly JPH_BroadPhaseLayerFilter_Procs _broadPhaseLayerFilter_Procs;

    public BroadPhaseLayerFilter()
        : base(JPH_BroadPhaseLayerFilter_Create())
    {
        nint ctx = DelegateProxies.CreateUserData(this, true);
        _broadPhaseLayerFilter_Procs = new JPH_BroadPhaseLayerFilter_Procs
        {
            ShouldCollide = &ShouldCollideCallback,
        };
        JPH_BroadPhaseLayerFilter_SetProcs(Handle, _broadPhaseLayerFilter_Procs, ctx);
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="BodyFilter" /> class.
    /// </summary>
    ~BroadPhaseLayerFilter() => Dispose(disposing: false);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            JPH_BroadPhaseLayerFilter_Destroy(Handle);
        }
    }

    protected abstract bool ShouldCollide(BroadPhaseLayer layer);

    [UnmanagedCallersOnly]
    private static Bool32 ShouldCollideCallback(IntPtr context, BroadPhaseLayer layer)
    {
        BroadPhaseLayerFilter listener = DelegateProxies.GetUserData<BroadPhaseLayerFilter>(context, out _);
        return listener.ShouldCollide(layer);
    }
}
