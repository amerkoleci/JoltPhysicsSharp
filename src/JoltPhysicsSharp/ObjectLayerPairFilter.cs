// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class ObjectLayerPairFilter : NativeObject
{
    private static readonly Dictionary<IntPtr, ObjectLayerPairFilter> s_listeners = new();
    private static readonly JPH_ObjectLayerPairFilter_Procs s_ObjectLayerPairFilter_Procs;

    static unsafe ObjectLayerPairFilter()
    {
        s_ObjectLayerPairFilter_Procs = new JPH_ObjectLayerPairFilter_Procs
        {
#if NET6_0_OR_GREATER
            ShouldCollide = &ShouldCollideCallback
#else
            ShouldCollide = ShouldCollideCallback
#endif
        };
        JPH_ObjectLayerPairFilter_SetProcs(s_ObjectLayerPairFilter_Procs);
    }

    public ObjectLayerPairFilter()
        : base(JPH_ObjectLayerPairFilter_Create())
    {
        s_listeners.Add(Handle, this);
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="ObjectLayerPairFilter" /> class.
    /// </summary>
    ~ObjectLayerPairFilter() => Dispose(isDisposing: false);

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            s_listeners.Remove(Handle);

            JPH_ObjectLayerPairFilter_Destroy(Handle);
        }
    }

    protected abstract bool ShouldCollide(ObjectLayer object1, ObjectLayer object2);

#if NET6_0_OR_GREATER
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#else
    [MonoPInvokeCallback(typeof(ObjectLayerPairFilterShouldCollideDelegate))]
#endif
    private static Bool32 ShouldCollideCallback(IntPtr listenerPtr, ObjectLayer object1, ObjectLayer object2)
    {
        ObjectLayerPairFilter listener = s_listeners[listenerPtr];
        return listener.ShouldCollide(object1, object2);
    }
}
