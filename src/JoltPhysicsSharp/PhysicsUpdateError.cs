// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace JoltPhysicsSharp;

/// <summary>
/// Enum used by <see cref="PhysicsSystem"/> to report error conditions during the <see cref="PhysicsSystem.Update(float, int, int, in JoltPhysicsSharp.TempAllocator, in JoltPhysicsSharp.JobSystemThreadPool)"/> call. This is a bit field, multiple errors can trigger in the same update.
/// </summary>
[Flags]
public enum PhysicsUpdateError
{
    /// <summary>
    /// No errors.
    /// </summary>
    None = 0,
    /// <summary>
    /// The manifold cache is full, this means that the total number of contacts between bodies is too high.
    /// Some contacts were ignored. Increase maxContactConstraints in <see cref="PhysicsSystem.Init(uint, uint, uint, uint, BroadPhaseLayerInterfaceTable, ObjectVsBroadPhaseLayerFilter, ObjectLayerPairFilterTable)"/>.
    /// </summary>
    ManifoldCacheFull = 1 << 0,
    /// <summary>
    /// The body pair cache is full, this means that too many bodies contacted. Some contacts were ignored.
    /// Increase maxBodyPairs in <see cref="PhysicsSystem.Init(uint, uint, uint, uint, BroadPhaseLayerInterfaceTable, ObjectVsBroadPhaseLayerFilter, ObjectLayerPairFilterTable)"/>.
    /// </summary>
	BodyPairCacheFull = 1 << 1,
    /// <summary>
    /// The contact constraints buffer is full. Some contacts were ignored.
    /// Increase inMaxContactConstraints in <see cref="PhysicsSystem.Init(uint, uint, uint, uint, BroadPhaseLayerInterfaceTable, ObjectVsBroadPhaseLayerFilter, ObjectLayerPairFilterTable)"/>.
    /// </summary>
	ContactConstraintsFull = 1 << 2,
}
