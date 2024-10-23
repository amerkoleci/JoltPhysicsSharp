// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class JobSystem : NativeObject
{
    protected JobSystem(nint handle)
    {
        Handle = handle;
    }


    protected override void Dispose(bool disposing)
    {
        if (Handle != 0)
        {
            JPH_JobSystem_Destroy(Handle);
            Handle = 0;
        }
    }
}
