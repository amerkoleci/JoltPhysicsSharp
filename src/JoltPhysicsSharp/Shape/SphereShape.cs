// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class SphereShapeSettings : ConvexShapeSettings
{
    public unsafe SphereShapeSettings(float radius)
        : base(JPH_SphereShapeSettings_Create(radius))
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="SphereShapeSettings" /> class.
    /// </summary>
    ~SphereShapeSettings() => Dispose(disposing: false);

    public float Radius
    {
        get => JPH_SphereShapeSettings_GetRadius(Handle);
        set => JPH_SphereShapeSettings_SetRadius(Handle, value);
    }

    public override Shape Create() => new SphereShape(this);
}

public sealed class SphereShape : ConvexShape
{
    public SphereShape(float radius)
        : base(JPH_SphereShape_Create(radius))
    {
    }

    public SphereShape(SphereShapeSettings settings)
        : base(JPH_SphereShapeSettings_CreateShape(settings.Handle))
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="SphereShape" /> class.
    /// </summary>
    ~SphereShape() => Dispose(disposing: false);

    public float Radius
    {
        get => JPH_SphereShape_GetRadius(Handle);
    }
}
