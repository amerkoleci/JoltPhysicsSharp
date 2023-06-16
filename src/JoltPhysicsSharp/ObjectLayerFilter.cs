// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class ObjectLayerFilter : NativeObject
{
    private static readonly Dictionary<IntPtr, ObjectLayerFilter> s_listeners = new();
    private static readonly JPH_ObjectLayerFilter_Procs s_objectLayerFilter_Procs;

    static unsafe ObjectLayerFilter()
    {
        s_objectLayerFilter_Procs = new JPH_ObjectLayerFilter_Procs
        {
#if NET6_0_OR_GREATER
            ShouldCollide = &ShouldCollideCallback,
#else
            ShouldCollide = ShouldCollideCallback,
#endif
        };
        JPH_ObjectLayerFilter_SetProcs(s_objectLayerFilter_Procs);
    }

    public ObjectLayerFilter()
        : base(JPH_ObjectLayerFilter_Create())
    {
        s_listeners.Add(Handle, this);
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="BodyFilter" /> class.
    /// </summary>
    ~ObjectLayerFilter() => Dispose(isDisposing: false);

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            s_listeners.Remove(Handle);

            JPH_ObjectLayerFilter_Destroy(Handle);
        }
    }

    protected abstract bool ShouldCollide(ObjectLayer layer);

#if NET6_0_OR_GREATER
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#else
    [MonoPInvokeCallback(typeof(ObjectLayerFilterShouldCollideDelegate))]
#endif
    private static uint ShouldCollideCallback(IntPtr listenerPtr, ObjectLayer layer)
    {
        ObjectLayerFilter listener = s_listeners[listenerPtr];
        return (uint)(listener.ShouldCollide(layer) ? 1 : 0);
    }
}
