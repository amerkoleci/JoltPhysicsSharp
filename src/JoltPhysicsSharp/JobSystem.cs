// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class JobSystem : NativeObject
{
    public JobSystem(nint handle)
        : base(handle)
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="JobSystem" /> class.
    /// </summary>
    ~JobSystem() => Dispose(disposing: false);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            JPH_JobSystem_Destroy(Handle);
        }
    }
}
