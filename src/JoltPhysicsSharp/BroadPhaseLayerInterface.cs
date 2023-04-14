// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class BroadPhaseLayerInterface : NativeObject
{
    private static readonly Dictionary<IntPtr, BroadPhaseLayerInterface> s_listeners = new();
    private static readonly JPH_BroadPhaseLayerInterface_Procs s_broadPhaseLayerInterface_Procs;

    static unsafe BroadPhaseLayerInterface()
    {
        s_broadPhaseLayerInterface_Procs = new JPH_BroadPhaseLayerInterface_Procs
        {
#if NET6_0_OR_GREATER
            GetNumBroadPhaseLayers = &GetNumBroadPhaseLayersCallback,
            GetBroadPhaseLayer = &GetBroadPhaseLayerCallback,
            GetBroadPhaseLayerName = &GetBroadPhaseLayerNameCallback
#else
            GetNumBroadPhaseLayers = GetNumBroadPhaseLayersCallback,
            GetBroadPhaseLayer = GetBroadPhaseLayerCallback,
            GetBroadPhaseLayerName = GetBroadPhaseLayerNameCallback
#endif
        };
        JPH_BroadPhaseLayerInterface_SetProcs(s_broadPhaseLayerInterface_Procs);
    }

    public BroadPhaseLayerInterface()
        : base(JPH_BroadPhaseLayerInterface_Create())
    {
        s_listeners.Add(Handle, this);
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="BroadPhaseLayerInterface" /> class.
    /// </summary>
    ~BroadPhaseLayerInterface() => Dispose(isDisposing: false);

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            s_listeners.Remove(Handle);

            JPH_BroadPhaseLayerInterface_Destroy(Handle);
        }
    }

    protected abstract int GetNumBroadPhaseLayers();
    protected abstract BroadPhaseLayer GetBroadPhaseLayer(ObjectLayer layer);
    protected abstract string GetBroadPhaseLayerName(BroadPhaseLayer layer);

#if NET6_0_OR_GREATER
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#else
    [MonoPInvokeCallback(typeof(GetNumBroadPhaseLayersDelegate))]
#endif
    private static uint GetNumBroadPhaseLayersCallback(IntPtr listenerPtr)
    {
        BroadPhaseLayerInterface listener = s_listeners[listenerPtr];
        return (uint)listener.GetNumBroadPhaseLayers();
    }

#if NET6_0_OR_GREATER
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#else
    [MonoPInvokeCallback(typeof(GetBroadPhaseLayerDelegate))]
#endif
    private static BroadPhaseLayer GetBroadPhaseLayerCallback(IntPtr listenerPtr, ObjectLayer layer)
    {
        BroadPhaseLayerInterface listener = s_listeners[listenerPtr];
        return listener.GetBroadPhaseLayer(layer);
    }

#if NET6_0_OR_GREATER
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#else
    [MonoPInvokeCallback(typeof(GetBroadPhaseLayerNameDelegate))]
#endif
    private static unsafe IntPtr GetBroadPhaseLayerNameCallback(IntPtr listenerPtr, BroadPhaseLayer layer)
    {
        BroadPhaseLayerInterface listener = s_listeners[listenerPtr];
        string layerName = listener.GetBroadPhaseLayerName(layer);
        return Marshal.StringToHGlobalAnsi(layerName);
    }
}
