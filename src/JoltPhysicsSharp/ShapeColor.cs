// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace JoltPhysicsSharp;

/// <summary>
/// Defines how to color soft body constraints
/// </summary>
public enum ShapeColor
{
    /// <summary>
    /// Random color per instance
    /// </summary>
    InstanceColor,
    /// <summary>
    /// Convex = green, scaled = yellow, compound = orange, mesh = red
    /// </summary>
    ShapeTypeColor,
    /// <summary>
    /// Static = grey, keyframed = green, dynamic = random color per instance
    /// </summary>
    MotionTypeColor,
    /// <summary>
    /// Static = grey, keyframed = green, dynamic = yellow, sleeping = red
    /// </summary>
    SleepColor,
    /// <summary>
    /// Static = grey, active = random color per island, sleeping = light grey
    /// </summary>
    IslandColor,
    /// <summary>
    /// Color as defined by the PhysicsMaterial of the shape
    /// </summary>
    MaterialColor,
}
