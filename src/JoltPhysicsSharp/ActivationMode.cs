// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace JoltPhysicsSharp;

public enum ActivationMode
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
