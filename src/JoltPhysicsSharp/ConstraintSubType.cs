// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace JoltPhysicsSharp;

/// <summary>
/// Defines the subtype of <see cref="Constraint"/>
/// </summary>
public enum ConstraintSubType
{
    Fixed = 0,
    Point = 1,
    Hinge = 2,
    Slider = 3,
    Distance = 4,
    Cone = 5,
    SwingTwist = 6,
    SixDOF = 7,
    Path = 8,
    Vehicle = 9,
    RackAndPinion = 10,
    Gear = 11,
    Pulley = 12,

    User1 = 13,
    User2 = 14,
    User3 = 15,
    User4 = 16,
}
