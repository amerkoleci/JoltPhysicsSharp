// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class BodyFilter : NativeObject
{
    private readonly JPH_BodyFilter_Procs _bodyFilter_Procs;

    public BodyFilter()
        : base(JPH_BodyFilter_Create())
    {
        nint context = DelegateProxies.CreateUserData(this, true);
        _bodyFilter_Procs = new JPH_BodyFilter_Procs
        {
            ShouldCollide = &ShouldCollideCallback,
            ShouldCollideLocked = &ShouldCollideLockedCallback,
        };
        JPH_BodyFilter_SetProcs(Handle, _bodyFilter_Procs, context);
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="BodyFilter" /> class.
    /// </summary>
    ~BodyFilter() => Dispose(disposing: false);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            JPH_BodyFilter_Destroy(Handle);
        }
    }

    protected abstract bool ShouldCollide(BodyID bodyID);
    protected abstract bool ShouldCollideLocked(Body body);

    [UnmanagedCallersOnly]
    private static Bool32 ShouldCollideCallback(nint context, BodyID bodyID)
    {
        BodyFilter listener = DelegateProxies.GetUserData<BodyFilter>(context, out _);
        return listener.ShouldCollide(bodyID);
    }

    [UnmanagedCallersOnly]
    private static Bool32 ShouldCollideLockedCallback(nint context, nint body)
    {
        BodyFilter listener = DelegateProxies.GetUserData<BodyFilter>(context, out _);
        return listener.ShouldCollideLocked(body);
    }
}
