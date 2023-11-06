// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace JoltPhysicsSharp;

/// <summary>
/// Enum used by AddBody to determine if the body needs to be initially active
/// </summary>
public enum Activation
{
    /// <summary>
    /// Activate the body, making it part of the simulation.
    /// </summary>
    Activate = 0,
    /// <summary>
    /// Leave activation state as it is (will not deactivate an active body)
    /// </summary>
    DontActivate = 1,
}
