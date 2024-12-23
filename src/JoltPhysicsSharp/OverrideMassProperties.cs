// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace JoltPhysicsSharp;

/// <summary>
/// Enum used in <see cref="BodyCreationSettings"/> to indicate how mass and inertia should be calculated.
/// </summary>
public enum OverrideMassProperties
{
    /// <summary>
    /// Tells the system to calculate the mass and inertia based on density
    /// </summary>
    CalculateMassAndInertia,
    /// <summary>
    /// Tells the system to take the mass from <see cref="BodyCreationSettings.MassPropertiesOverride"/> and to calculate the inertia based on density of the shapes and to scale it to the provided mass
    /// </summary>
	CalculateInertia,
    /// <summary>
    /// Tells the system to take the mass and inertia from <see cref="BodyCreationSettings.MassPropertiesOverride"/>
    /// </summary>
	MassAndInertiaProvided
}
