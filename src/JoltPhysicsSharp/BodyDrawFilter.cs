// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class BodyDrawFilter : NativeObject
{
    private static readonly JPH_BodyDrawFilter_Procs _procs;
    private readonly nint _listenerUserData;

    static BodyDrawFilter()
    {
        unsafe
        {
            _procs = new JPH_BodyDrawFilter_Procs
            {
                ShouldDraw = &ShouldDrawCallback,
            };
            JPH_BodyDrawFilter_SetProcs(in _procs);
        }
    }

    public BodyDrawFilter()
    {
        _listenerUserData = DelegateProxies.CreateUserData(this, true);

        Handle = JPH_BodyDrawFilter_Create(_listenerUserData);
    }

    protected override void DisposeNative()
    {
        DelegateProxies.GetUserData<BodyDrawFilter>(_listenerUserData, out GCHandle gch);

        JPH_BodyDrawFilter_Destroy(Handle);
        gch.Free();
    }

    protected abstract bool ShouldDraw(Body body);

    [UnmanagedCallersOnly]
    private static Bool8 ShouldDrawCallback(nint context, nint body)
    {
        BodyDrawFilter listener = DelegateProxies.GetUserData<BodyDrawFilter>(context, out _);
        return listener.ShouldDraw(Body.GetObject(body)!);
    }
}
