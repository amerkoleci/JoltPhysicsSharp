// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace JoltPhysicsSharp;

/// <summary>
/// Defines how to color soft body constraints
/// </summary>
public enum SoftBodyConstraintColor
{
    /// <summary>
    /// Draw different types of constraints in different colors
    /// </summary>
    ConstraintType,
    /// <summary>
    /// Draw constraints in the same group in the same color, non-parallel group will be red
    /// </summary>
    ConstraintGroup,
    /// <summary>
    /// Draw constraints in the same group in the same color, non-parallel group will be red, and order within each group will be indicated with gradient
    /// </summary>
    ConstraintOrder,
}
