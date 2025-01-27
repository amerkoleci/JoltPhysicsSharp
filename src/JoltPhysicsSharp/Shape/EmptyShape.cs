// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

/// <summary>
/// Class that constructs an <see cref="EmptyShape"/>
/// </summary>
public class EmptyShapeSettings : ShapeSettings
{
    internal EmptyShapeSettings(nint handle)
        : base(handle)
    {
    }

    public EmptyShapeSettings(in Vector3 centerOfMass)
      : base(JPH_EmptyShapeSettings_Create(in centerOfMass))
    {
    }

    public override Shape Create() => new EmptyShape(this);
}

/// <summary>
/// An empty shape that has no volume and collides with nothing.
///
/// Possible use cases:
/// - As a placeholder for a shape that will be created later. E.g. if you first need to create a body and only then know what shape it will have.
/// - If you need a kinematic body to attach a constraint to, but you don't want the body to collide with anything.
///
/// Note that, if possible, you should also put your body in an ObjectLayer that doesn't collide with anything.
/// This ensures that collisions will be filtered out at broad phase level instead of at narrow phase level, this is more efficient.
/// </summary>
public class EmptyShape : Shape
{
    internal EmptyShape(nint handle)
        : base(handle)
    {
    }

    public EmptyShape(in EmptyShapeSettings settings)
        : base(JPH_EmptyShapeSettings_CreateShape(settings.Handle))
    {
    }
}
