// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class BodyFilter : NativeObject
{
    private static readonly Dictionary<IntPtr, BodyFilter> s_listeners = new();
    private static readonly JPH_BodyFilter_Procs s_bodyFilter_Procs;

    static unsafe BodyFilter()
    {
        s_bodyFilter_Procs = new JPH_BodyFilter_Procs
        {
#if NET6_0_OR_GREATER
            ShouldCollide = &ShouldCollideCallback,
            ShouldCollideLocked = &ShouldCollideLockedCallback,
#else
            ShouldCollide = ShouldCollideCallback,
            ShouldCollideLocked = ShouldCollideLockedCallback,
#endif
        };
        JPH_BodyFilter_SetProcs(s_bodyFilter_Procs);
    }

    public BodyFilter()
        : base(JPH_BodyFilter_Create())
    {
        s_listeners.Add(Handle, this);
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="BodyFilter" /> class.
    /// </summary>
    ~BodyFilter() => Dispose(isDisposing: false);

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            s_listeners.Remove(Handle);

            JPH_BodyFilter_Destroy(Handle);
        }
    }

    protected abstract bool ShouldCollide(BodyID bodyID);
    protected abstract bool ShouldCollideLocked(Body body);
    
#if NET6_0_OR_GREATER
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#else
    [MonoPInvokeCallback(typeof(BodyFilterShouldCollideDelegate))]
#endif
    private static uint ShouldCollideCallback(IntPtr listenerPtr, BodyID bodyID)
    {
        BodyFilter listener = s_listeners[listenerPtr];
        return (uint)(listener.ShouldCollide(bodyID) ? 1 : 0);
    }

#if NET6_0_OR_GREATER
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
#else
    [MonoPInvokeCallback(typeof(BodyFilterShouldCollideLockedDelegate))]
#endif
    private static uint ShouldCollideLockedCallback(IntPtr listenerPtr, IntPtr body)
    {
        BodyFilter listener = s_listeners[listenerPtr];
        return (uint)(listener.ShouldCollideLocked(body) ? 1 : 0);
    }
}
