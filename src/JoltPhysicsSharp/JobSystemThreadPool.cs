// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class JobSystemThreadPool(int maxJobs, int maxBarriers, int inNumThreads = -1) : JobSystem(JPH_JobSystemThreadPool_Create((uint)maxJobs, (uint)maxBarriers, inNumThreads))
{
}
