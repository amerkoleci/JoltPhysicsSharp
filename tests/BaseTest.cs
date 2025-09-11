﻿// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics;
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

        Foundation.SetTraceHandler((message) =>
        {
            Console.WriteLine(message);
        });

#if DEBUG
        Foundation.SetAssertFailureHandler((inExpression, inMessage, inFile, inLine) =>
        {
            string message = inMessage ?? inExpression;

            string outMessage = $"[JoltPhysics] Assertion failure at {inFile}:{inLine}: {message}";

            Debug.WriteLine(outMessage);

            throw new Exception(outMessage);
        });
#endif
    }

    public void Dispose()
    {
        Foundation.Shutdown();
    }

    protected static void CHECK_APPROX_EQUAL(float inLHS, float inRHS, float inTolerance = 1.0e-6f)
    {
        Assert.That(MathF.Abs(inRHS - inLHS) <= inTolerance, Is.True);
    }

    private static void CHECK_APPROX_EQUAL(in Vector3 inLHS, in Vector3 inRHS, float inTolerance = 1.0e-6f)
    {
        Assert.That(IsClose(inLHS, inRHS, inTolerance * inTolerance), Is.True);
    }

    private static bool IsClose(in Vector3 inV1, in Vector3 inV2, float inMaxDistSq = 1.0e-12f)
    {
        return (inV2 - inV1).LengthSquared() <= inMaxDistSq;
    }
}
