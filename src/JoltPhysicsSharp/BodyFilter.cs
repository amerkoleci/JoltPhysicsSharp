// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class BodyFilter : NativeObject
{
    private readonly JPH_BodyFilter_Procs _procs;

    public BodyFilter()
    {
        nint context = DelegateProxies.CreateUserData(this, true);
        _procs = new JPH_BodyFilter_Procs
        {
            ShouldCollide = &ShouldCollideCallback,
            ShouldCollideLocked = &ShouldCollideLockedCallback,
        };
        Handle = JPH_BodyFilter_Create(in _procs, context);
    }

    protected override void DisposeNative()
    {
        JPH_BodyFilter_Destroy(Handle);
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
