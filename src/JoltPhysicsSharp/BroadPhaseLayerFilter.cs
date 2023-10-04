// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class BroadPhaseLayerFilter : NativeObject
{
    private static readonly Dictionary<IntPtr, BroadPhaseLayerFilter> s_listeners = new();
    private static readonly JPH_BroadPhaseLayerFilter_Procs s_broadPhaseLayerFilter_Procs;

    static unsafe BroadPhaseLayerFilter()
    {
        s_broadPhaseLayerFilter_Procs = new JPH_BroadPhaseLayerFilter_Procs
        {
            ShouldCollide = &ShouldCollideCallback,
        };
        JPH_BroadPhaseLayerFilter_SetProcs(s_broadPhaseLayerFilter_Procs);
    }

    public BroadPhaseLayerFilter()
        : base(JPH_BroadPhaseLayerFilter_Create())
    {
        s_listeners.Add(Handle, this);
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="BodyFilter" /> class.
    /// </summary>
    ~BroadPhaseLayerFilter() => Dispose(isDisposing: false);

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            s_listeners.Remove(Handle);

            JPH_BroadPhaseLayerFilter_Destroy(Handle);
        }
    }

    protected abstract bool ShouldCollide(BroadPhaseLayer layer);

    [UnmanagedCallersOnly]
    private static Bool32 ShouldCollideCallback(IntPtr listenerPtr, BroadPhaseLayer layer)
    {
        BroadPhaseLayerFilter listener = s_listeners[listenerPtr];
        return listener.ShouldCollide(layer);
    }
}
