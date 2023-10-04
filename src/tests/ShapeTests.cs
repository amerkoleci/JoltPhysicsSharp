// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using NUnit.Framework;

namespace JoltPhysicsSharp.Tests;

[TestFixture(TestOf = typeof(Shape))]
public class ShapeTests : BaseTest
{
    [Test]
    public static void TestConvexHullShape()
    {
        const float cDensity = 1.5f;

        // Create convex hull shape of a box
        ReadOnlySpan<Vector3> box = [
            new(5, 6, 7),
            new(5, 6, 14),
            new(5, 12, 7),
            new(5, 12, 14),
            new(10, 6, 7),
            new(10, 6, 14),
            new(10, 12, 7),
            new(10, 12, 14)
        ];

        ConvexHullShapeSettings settings = new(box);
        settings.Density = cDensity;

        using ConvexHullShape shape = new(settings);

        //RefConst<Shape> shape = settings.Create().Get();

        // Validate calculated center of mass
        //Vec3 com = shape->GetCenterOfMass();
        //CHECK_APPROX_EQUAL(Vec3(7.5f, 9.0f, 10.5f), com, 1.0e-5f);

        // Calculate reference value of mass and inertia of a box
        //MassProperties reference;
        //reference.SetMassAndInertiaOfSolidBox(Vec3(5, 6, 7), cDensity);

        // Mass is easy to calculate, double check if SetMassAndInertiaOfSolidBox calculated it correctly
        //CHECK_APPROX_EQUAL(5.0f * 6.0f * 7.0f * cDensity, reference.mMass, 1.0e-6f);
    }
}
