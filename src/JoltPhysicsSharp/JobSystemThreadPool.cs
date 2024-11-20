// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed unsafe class JobSystemThreadPool : JobSystem
{
    public JobSystemThreadPool()
        : base(JPH_JobSystemThreadPool_Create(null))
    {
    }

    public JobSystemThreadPool(in JobSystemThreadPoolConfig config)
        : base(JPH_JobSystemThreadPool_Create(in config))
    {
    }
}

public struct JobSystemThreadPoolConfig
{
    public uint maxJobs;
    public uint maxBarriers;
    public int numThreads;
}
