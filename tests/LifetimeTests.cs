// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using NUnit.Framework;

namespace JoltPhysicsSharp.Tests;

[TestFixture]
public class LifetimeTests : BaseTest
{
    // A settings object whose only use is passing its handle to a native call may be collected, and its finalizer may release
    // the native settings, while that call still reads them. Collections run on another thread all through, so that without
    // the settings kept alive one lands inside a create call and the process crashes.
    [Test]
    public void SettingsAreKeptAliveThroughCreate()
    {
        Vector3[] points = new Vector3[64];
        Random random = new(1);
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = Vector3.Normalize(new Vector3(random.NextSingle() - 0.5f, random.NextSingle() - 0.5f, random.NextSingle() - 0.5f));
        }

        bool stop = false;
        Thread collector = new(() =>
        {
            while (!Volatile.Read(ref stop))
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        });
        collector.Start();
        try
        {
            for (int i = 0; i < 2000; i++)
            {
                using Shape shape = CreateCompound(points);
                Assert.That(shape.Type, Is.EqualTo(ShapeType.Compound));
            }
        }
        finally
        {
            Volatile.Write(ref stop, true);
            collector.Join();
        }
    }

    static Shape CreateCompound(Vector3[] points)
    {
        StaticCompoundShapeSettings settings = new();
        for (int i = 0; i < 16; i++)
        {
            settings.AddShape(new Vector3(3 * i, 0, 0), Quaternion.Identity, new ConvexHullShapeSettings(points));
        }
        return settings.Create();
    }
}
