// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public static class Jolt
{
    public static unsafe CollisionEstimationResult EstimateCollisionResponse(in Body body1, in Body body2, in ContactManifold manifold, float combinedFriction, float combinedRestitution, float minVelocityForRestitution = 1.0f, int numIterations = 10)
    {
        Debug.Assert(numIterations > 0);

        CollisionEstimationResult result;
        JPH_EstimateCollisionResponse(
            body1.Handle,
            body2.Handle,
            manifold.Handle,
            combinedFriction,
            combinedRestitution,
            minVelocityForRestitution,
            numIterations,
            &result);
        return result;
    }

    [SkipLocalsInit]
    public static unsafe void EstimateCollisionResponse(in Body body1, in Body body2, in ContactManifold manifold, out CollisionEstimationResult result, float combinedFriction, float combinedRestitution, float minVelocityForRestitution = 1.0f, int numIterations = 10)
    {
        Debug.Assert(numIterations > 0);
        Unsafe.SkipInit(out result);

        // Pin - Pin data in preparation for calling the P/Invoke.
        fixed (CollisionEstimationResult* __result_native = &result)
        {
            JPH_EstimateCollisionResponse(
                body1.Handle,
                body2.Handle,
                manifold.Handle,
                combinedFriction,
                combinedRestitution,
                minVelocityForRestitution,
                numIterations,
                __result_native);
        }
    }
}
