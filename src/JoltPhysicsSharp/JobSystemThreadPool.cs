// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class JobSystemThreadPool : NativeObject
{
    public JobSystemThreadPool(int maxJobs, int maxBarriers, int inNumThreads = -1)
        : base(JPH_JobSystemThreadPool_Create((uint)maxJobs, (uint)maxBarriers, inNumThreads))
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="JobSystemThreadPool" /> class.
    /// </summary>
    ~JobSystemThreadPool() => Dispose(isDisposing: false);

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            JPH_JobSystemThreadPool_Destroy(Handle);
        }
    }
}
