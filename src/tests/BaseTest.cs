// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using NUnit.Framework;

namespace JoltPhysicsSharp.Tests;

public abstract class BaseTest : IDisposable
{
    protected BaseTest()
    {
        if (!Foundation.Init())
        {
            return;
        }
    }

    public void Dispose()
    {
        Foundation.Shutdown();
    }

    protected static void CHECK_APPROX_EQUAL(float inLHS, float inRHS, float inTolerance = 1.0e-6f)
    {
        Assert.True(MathF.Abs(inRHS - inLHS) <= inTolerance);
    }

    private static void CHECK_APPROX_EQUAL(in Vector3 inLHS, in Vector3 inRHS, float inTolerance = 1.0e-6f)
    {
        Assert.True(IsClose(inLHS, inRHS, inTolerance * inTolerance));
    }

    private static bool IsClose(in Vector3 inV1, in Vector3 inV2, float inMaxDistSq = 1.0e-12f)
    {
        return (inV2 - inV1).LengthSquared() <= inMaxDistSq;
    }
}
