// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class BodyDrawFilter : NativeObject
{
    private readonly JPH_BodyDrawFilter_Procs _procs;

    public BodyDrawFilter()
    {
        nint context = DelegateProxies.CreateUserData(this, true);
        _procs = new JPH_BodyDrawFilter_Procs
        {
            ShouldDraw = &ShouldDrawCallback,
        };
        Handle = JPH_BodyDrawFilter_Create(_procs, context);
    }

    protected override void DisposeNative()
    {
        JPH_BodyDrawFilter_Destroy(Handle);
    }

    protected abstract bool ShouldDraw(Body body);

    [UnmanagedCallersOnly]
    private static Bool8 ShouldDrawCallback(nint context, nint body)
    {
        BodyDrawFilter listener = DelegateProxies.GetUserData<BodyDrawFilter>(context, out _);
        return listener.ShouldDraw(Body.GetObject(body)!);
    }
}
