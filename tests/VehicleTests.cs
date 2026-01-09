// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using NUnit.Framework;

namespace JoltPhysicsSharp.Tests;

[TestFixture(TestOf = typeof(VehicleConstraint))]
public class VehicleTests : BaseTest
{
    [Test]
    public void TestTrackSideEnum()
    {
        Assert.That((int)TrackSide.Left, Is.EqualTo(0));
        Assert.That((int)TrackSide.Right, Is.EqualTo(1));
    }

    [Test]
    public void TestVehicleTrackSettingsDefault()
    {
        // Test native initialization with default values
        VehicleTrackSettings settings = new();

        Assert.That(settings.DrivenWheel, Is.EqualTo(0u));
        Assert.That(settings.Inertia, Is.GreaterThan(0f));
        Assert.That(settings.AngularDamping, Is.GreaterThanOrEqualTo(0f));
        Assert.That(settings.MaxBrakeTorque, Is.GreaterThan(0f));
        Assert.That(settings.DifferentialRatio, Is.GreaterThan(0f));
    }

    [Test]
    public void TestVehicleTrackSettingsManualInit()
    {
        // Test manual initialization without calling native Init
        VehicleTrackSettings settings = default;
        settings.DrivenWheel = 1;
        settings.Wheels = [0, 1, 2, 3];
        settings.Inertia = 5.0f;
        settings.AngularDamping = 0.5f;
        settings.MaxBrakeTorque = 1000.0f;
        settings.DifferentialRatio = 3.5f;

        Assert.That(settings.DrivenWheel, Is.EqualTo(1u));
        Assert.That(settings.Wheels, Is.Not.Null);
        Assert.That(settings.Wheels!.Length, Is.EqualTo(4));
        Assert.That(settings.Wheels[0], Is.EqualTo(0u));
        Assert.That(settings.Wheels[3], Is.EqualTo(3u));
        Assert.That(settings.Inertia, Is.EqualTo(5.0f));
        Assert.That(settings.DifferentialRatio, Is.EqualTo(3.5f));
    }

    [Test]
    public void TestVehicleTrackSettingsNullWheels()
    {
        VehicleTrackSettings settings = default;
        Assert.That(settings.Wheels, Is.Null);
        Assert.That(settings.DrivenWheel, Is.EqualTo(0u));
    }

    [Test]
    public void TestWheelSettingsTV()
    {
        using WheelSettingsTV settings = new();

        // Test default values and property access
        float longitudinalFriction = settings.LongitudinalFriction;
        float lateralFriction = settings.LateralFriction;

        Assert.That(longitudinalFriction, Is.GreaterThanOrEqualTo(0f));
        Assert.That(lateralFriction, Is.GreaterThanOrEqualTo(0f));

        // Test setters
        settings.LongitudinalFriction = 2.0f;
        settings.LateralFriction = 1.5f;

        Assert.That(settings.LongitudinalFriction, Is.EqualTo(2.0f));
        Assert.That(settings.LateralFriction, Is.EqualTo(1.5f));
    }

    [Test]
    public void TestTrackedVehicleControllerSettings()
    {
        using TrackedVehicleControllerSettings settings = new();

        // Test Engine property
        VehicleEngineSettings engine = settings.Engine;
        Assert.That(engine.MaxRPM, Is.GreaterThan(0f));

        // Test Transmission property
        VehicleTransmissionSettings transmission = settings.Transmission;
        Assert.That(transmission, Is.Not.Null);
    }
}
