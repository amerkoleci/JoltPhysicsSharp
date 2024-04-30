// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace JoltPhysicsSharp;

/// <summary>
/// Enum used by constraints to specify how the spring is defined
/// </summary>
public enum SpringMode
{
    /// <summary>
    /// Frequency and damping are specified
    /// </summary>
    FrequencyAndDamping,
    /// <summary>
    /// Stiffness and damping are specified
    /// </summary>
	StiffnessAndDamping
}
