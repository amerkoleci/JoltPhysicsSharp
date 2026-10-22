// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using NUnit.Framework;

namespace JoltPhysicsSharp.Tests;

[TestFixture(TestOf = typeof(PhysicsSystem))]
public class PhysicsSystemTests : BaseTest
{
    [Test]
    public void BorrowedSystem_DisposeDoesNotDestroyNativeHandle()
    {
        // A sentinel handle is safe only because a borrowed wrapper must never pass
        // it to native destruction. No native operations are performed on it.
        using PhysicsSystem system = new((nint)146, false);
        Assert.That(system.OwnsHandle, Is.False);
        system.Dispose();
        system.Dispose();
        AssertDisposed(system);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void StandaloneBroadPhaseInterface_OwnsItsHandle(bool useMask)
    {
        using BroadPhaseLayerInterface broadPhase = useMask ? new BroadPhaseLayerInterfaceMask(1) : new BroadPhaseLayerInterfaceTable(1, 1);
        Assert.That(broadPhase.OwnsHandle, Is.True);
        broadPhase.Dispose();
        broadPhase.Dispose();
        AssertDisposed(broadPhase);
    }

    [Test]
    public void Constructor_RejectsBorrowedFilter()
    {
        PhysicsSystemSettings settings = CreateSettings();
        using ObjectLayerPairFilter pairFilter = settings.ObjectLayerPairFilter!;
        using BroadPhaseLayerInterface broadPhase = settings.BroadPhaseLayerInterface!;
        using ObjectVsBroadPhaseLayerFilter broadPhaseFilter = settings.ObjectVsBroadPhaseLayerFilter!;
        using ObjectLayerPairFilterTable borrowed = new((nint)147, false);
        PhysicsSystemSettings invalid = settings;
        invalid.ObjectLayerPairFilter = borrowed;

        Assert.Throws<ArgumentException>(() => new PhysicsSystem(invalid));
        Assert.That(borrowed.IsDisposed, Is.False);
        using PhysicsSystem system = new(settings);
        Assert.That(system.OwnsHandle, Is.True);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Dispose_InvalidatesTransferredFilters(bool useMask)
    {
        PhysicsSystemSettings settings = CreateSettings(useMask);
        using ObjectLayerPairFilter pairFilter = settings.ObjectLayerPairFilter!;
        using BroadPhaseLayerInterface broadPhase = settings.BroadPhaseLayerInterface!;
        using ObjectVsBroadPhaseLayerFilter broadPhaseFilter = settings.ObjectVsBroadPhaseLayerFilter!;
        using PhysicsSystem system = new(settings);

        Assert.That(system.OwnsHandle, Is.True);
        Assert.That(pairFilter.OwnsHandle, Is.False);
        Assert.That(broadPhase.OwnsHandle, Is.False);
        Assert.That(broadPhaseFilter.OwnsHandle, Is.False);
        Assert.That(system.MaxBodies, Is.EqualTo(16));

        // Disposal of a transferred wrapper must not destroy a filter still in use.
        pairFilter.Dispose();
        broadPhase.Dispose();
        broadPhaseFilter.Dispose();
        Assert.That(pairFilter.IsDisposed, Is.False);
        Assert.That(broadPhase.IsDisposed, Is.False);
        Assert.That(broadPhaseFilter.IsDisposed, Is.False);
        ConfigureFilters(settings);
        system.OptimizeBroadPhase();

        system.Dispose();
        system.Dispose();

        AssertDisposed(system);
        AssertDisposed(pairFilter);
        AssertDisposed(broadPhase);
        AssertDisposed(broadPhaseFilter);
        Assert.Throws<ObjectDisposedException>(() => _ = system.BodiesCount);
        Assert.Throws<ObjectDisposedException>(() => ConfigureFilters(settings));
        Assert.Throws<ObjectDisposedException>(() => new ObjectVsBroadPhaseLayerFilterMask(broadPhase));
        if (pairFilter is ObjectLayerPairFilterTable table)
        {
            Assert.Throws<ObjectDisposedException>(() => table.EnableCollision(new ObjectLayer(0), new ObjectLayer(0)));
            Assert.Throws<ObjectDisposedException>(() => table.DisableCollision(new ObjectLayer(0), new ObjectLayer(0)));
        }
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void Constructor_RejectsDisposedFilterWithoutConsumingOtherFilters(int filterIndex)
    {
        PhysicsSystemSettings settings = CreateSettings();
        using ObjectLayerPairFilter pairFilter = settings.ObjectLayerPairFilter!;
        using BroadPhaseLayerInterface broadPhase = settings.BroadPhaseLayerInterface!;
        using ObjectVsBroadPhaseLayerFilter broadPhaseFilter = settings.ObjectVsBroadPhaseLayerFilter!;
        NativeObject[] filters = [pairFilter, broadPhase, broadPhaseFilter];
        filters[filterIndex].Dispose();

        Assert.Throws<ObjectDisposedException>(() => new PhysicsSystem(settings));
        for (int i = 0; i < filters.Length; ++i)
            Assert.That(filters[i].IsDisposed, Is.EqualTo(i == filterIndex));
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void Constructor_RejectsMissingFilterWithoutTransferringOwnership(int filterIndex)
    {
        PhysicsSystemSettings settings = CreateSettings();
        using ObjectLayerPairFilter pairFilter = settings.ObjectLayerPairFilter!;
        using BroadPhaseLayerInterface broadPhase = settings.BroadPhaseLayerInterface!;
        using ObjectVsBroadPhaseLayerFilter broadPhaseFilter = settings.ObjectVsBroadPhaseLayerFilter!;
        PhysicsSystemSettings invalid = settings;
        if (filterIndex == 0)
            invalid.ObjectLayerPairFilter = null;
        else if (filterIndex == 1)
            invalid.BroadPhaseLayerInterface = null;
        else
            invalid.ObjectVsBroadPhaseLayerFilter = null;

        Assert.Throws<ArgumentNullException>(() => new PhysicsSystem(invalid));
        // All original filters must still be available for a valid construction.
        using PhysicsSystem system = new(settings);
        Assert.That(system.MaxBodies, Is.EqualTo(16));
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void Constructor_RejectsFilterAlreadyOwnedByAnotherSystem(int filterIndex)
    {
        PhysicsSystemSettings first = CreateSettings();
        using PhysicsSystem system = new(first);
        PhysicsSystemSettings second = CreateSettings();
        using ObjectLayerPairFilter pairFilter = second.ObjectLayerPairFilter!;
        using BroadPhaseLayerInterface broadPhase = second.BroadPhaseLayerInterface!;
        using ObjectVsBroadPhaseLayerFilter broadPhaseFilter = second.ObjectVsBroadPhaseLayerFilter!;
        PhysicsSystemSettings invalid = second;
        if (filterIndex == 0)
            invalid.ObjectLayerPairFilter = first.ObjectLayerPairFilter;
        else if (filterIndex == 1)
            invalid.BroadPhaseLayerInterface = first.BroadPhaseLayerInterface;
        else
            invalid.ObjectVsBroadPhaseLayerFilter = first.ObjectVsBroadPhaseLayerFilter;

        Assert.Throws<ArgumentException>(() => new PhysicsSystem(invalid));
        system.OptimizeBroadPhase();
        using PhysicsSystem other = new(second);
        Assert.That(other.MaxBodies, Is.EqualTo(16));
    }

    [Test]
    public void RepeatedLifecycles()
    {
        // Also used for native debugger tracing of create/destroy counts. Wrapper
        // state assertions alone do not prove native destruction or absence of leaks.
        for (int i = 0; i < 300; ++i)
        {
            PhysicsSystemSettings settings = CreateSettings(i % 2 != 0);
            using PhysicsSystem system = new(settings);
            system.OptimizeBroadPhase();
            system.Dispose();
            AssertDisposed(system);
            AssertDisposed(settings.ObjectLayerPairFilter!);
            AssertDisposed(settings.BroadPhaseLayerInterface!);
            AssertDisposed(settings.ObjectVsBroadPhaseLayerFilter!);
        }
    }

    private static void AssertDisposed(NativeObject value)
    {
        Assert.That(value.IsDisposed, Is.True);
        Assert.That(value.Handle, Is.EqualTo(nint.Zero));
    }

    private static void ConfigureFilters(PhysicsSystemSettings settings)
    {
        if (settings.BroadPhaseLayerInterface is BroadPhaseLayerInterfaceMask mask)
            mask.ConfigureLayer(new BroadPhaseLayer(0), uint.MaxValue, 0);
        else
        {
            ((BroadPhaseLayerInterfaceTable)settings.BroadPhaseLayerInterface!).MapObjectToBroadPhaseLayer(new ObjectLayer(0), new BroadPhaseLayer(0));
            ((ObjectLayerPairFilterTable)settings.ObjectLayerPairFilter!).EnableCollision(new ObjectLayer(0), new ObjectLayer(0));
        }
    }

    private static PhysicsSystemSettings CreateSettings(bool useMask = false)
    {
        ObjectLayerPairFilter pairFilter = useMask ? new ObjectLayerPairFilterMask() : new ObjectLayerPairFilterTable(1);
        BroadPhaseLayerInterface broadPhase = useMask ? new BroadPhaseLayerInterfaceMask(1) : new BroadPhaseLayerInterfaceTable(1, 1);
        ObjectVsBroadPhaseLayerFilter broadPhaseFilter = useMask
            ? new ObjectVsBroadPhaseLayerFilterMask(broadPhase)
            : new ObjectVsBroadPhaseLayerFilterTable(broadPhase, 1, pairFilter, 1);
        PhysicsSystemSettings settings = new()
        {
            MaxBodies = 16,
            MaxBodyPairs = 128,
            MaxContactConstraints = 128,
            ObjectLayerPairFilter = pairFilter,
            BroadPhaseLayerInterface = broadPhase,
            ObjectVsBroadPhaseLayerFilter = broadPhaseFilter
        };
        ConfigureFilters(settings);
        return settings;
    }
}
