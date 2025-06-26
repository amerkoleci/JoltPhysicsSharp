// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class PhysicsStepListener : NativeObject, IPhysicsStepListener
{
    private static readonly JPH_PhysicsStepListener_Procs s_procs;
    private readonly nint _listenerUserData;

    static unsafe PhysicsStepListener()
    {
        s_procs = new JPH_PhysicsStepListener_Procs
        {
            OnStep = &OnStepCallback
        };
        JPH_PhysicsStepListener_SetProcs(in s_procs);
    }

    public PhysicsStepListener()
    {
        _listenerUserData = DelegateProxies.CreateUserData(this, true);
        Handle = JPH_PhysicsStepListener_Create(_listenerUserData);
    }

    public PhysicsStepListener(nint handle, bool ownsHandle)
        : base(handle, ownsHandle)
    {
    }

    protected override void DisposeNative()
    {
        DelegateProxies.GetUserData<PhysicsStepListener>(_listenerUserData, out GCHandle gch);

        JPH_PhysicsStepListener_Destroy(Handle);
        gch.Free();
    }

    nint IPhysicsStepListener.Handle => Handle;

    protected virtual void OnStep(in PhysicsStepListenerContext context)
    {

    }

    [UnmanagedCallersOnly]
    private static unsafe void OnStepCallback(nint userData, PhysicsStepListenerContextNative* nativeContext)
    {
        PhysicsStepListener listener = DelegateProxies.GetUserData<PhysicsStepListener>(userData, out _);

        PhysicsStepListenerContext context = new()
        {
            DeltaTime = nativeContext->deltaTime,
            IsFirstStep = nativeContext->isFirstStep,
            IsLastStep = nativeContext->isLastStep,
            System = PhysicsSystem.GetObject(nativeContext->physicsSystem)!
        };

        listener.OnStep(in context);
    }
}

public interface IPhysicsStepListener
{
    public nint Handle { get; } 
}

public struct PhysicsStepListenerContext
{
    public float DeltaTime;
    public bool IsFirstStep;
    public bool IsLastStep;
    public PhysicsSystem System;
}
