// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class BodyFilter : NativeObject
{
    private static readonly JPH_BodyFilter_Procs _procs;
    private readonly nint _listenerUserData;

    static BodyFilter()
    {
        _procs = new JPH_BodyFilter_Procs
        {
            ShouldCollide = &ShouldCollideCallback,
            ShouldCollideLocked = &ShouldCollideLockedCallback,
        };
        JPH_BodyFilter_SetProcs(in _procs);
    }

    public BodyFilter()
    {
        _listenerUserData = DelegateProxies.CreateUserData(this, true);
        Handle = JPH_BodyFilter_Create(_listenerUserData);
    }

    protected override void DisposeNative()
    {
        DelegateProxies.GetUserData<BodyFilter>(_listenerUserData, out GCHandle gch);

        JPH_BodyFilter_Destroy(Handle);
        gch.Free();
    }

    protected abstract bool ShouldCollide(BodyID bodyID);
    protected abstract bool ShouldCollideLocked(Body body);

    [UnmanagedCallersOnly]
    private static Bool8 ShouldCollideCallback(nint context, BodyID bodyID)
    {
        BodyFilter listener = DelegateProxies.GetUserData<BodyFilter>(context, out _);
        return listener.ShouldCollide(bodyID);
    }

    [UnmanagedCallersOnly]
    private static Bool8 ShouldCollideLockedCallback(nint context, nint body)
    {
        BodyFilter listener = DelegateProxies.GetUserData<BodyFilter>(context, out _);
        return listener.ShouldCollideLocked(Body.GetObject(body)!);
    }
}
