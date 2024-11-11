// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using NUnit.Framework;

namespace JoltPhysicsSharp.Tests;

[TestFixture(TestOf = typeof(JoltColor))]
public class JoltColorTests
{
    [Test]
    public static unsafe void DefaultsTest()
    {
        Assert.That(() => sizeof(JoltColor), Is.EqualTo(4));

        var color = new JoltColor(255, 0, 255, 127);

        Assert.That(color.R, Is.EqualTo(255));
        Assert.That(color.G, Is.EqualTo(0));
        Assert.That(color.B, Is.EqualTo(255));
        Assert.That(color.A, Is.EqualTo(127));
        Assert.That(color.PackedValue, Is.EqualTo(2147418367));
    }
}
