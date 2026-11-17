// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class JobSystem : NativeObject
{
    protected JobSystem(nint handle, bool ownsHandle = true)
        : base(handle, ownsHandle)
    {
    }


    protected override void DisposeNative()
    {
        JPH_JobSystem_Destroy(Handle);
    }
}
