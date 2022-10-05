// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using JoltPhysicsSharp;

namespace HelloWorld;

public static class Program
{
    private const uint MaxBodies = 1024;

    private const uint NumBodyMutexes = 0;
    private const uint MaxBodyPairs = 1024;
    private const uint MaxContactConstraints = 1024;

    public static void Main()
    {
        if (!Foundation.Init())
            return;

        {
            using TempAllocator tempAllocator = new(10 * 1024 * 1024);
            using JobSystemThreadPool jobSystem = new(Foundation.MaxPhysicsJobs, Foundation.MaxPhysicsBarriers);
            using BroadPhaseLayer broadPhaseLayer = new();
            using PhysicsSystem physicsSystem = new();
            physicsSystem.Init(MaxBodies, NumBodyMutexes, MaxBodyPairs, MaxContactConstraints, broadPhaseLayer);
        }

        Foundation.Shutdown();
    }
}
