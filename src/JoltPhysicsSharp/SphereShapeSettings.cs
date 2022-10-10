// Copyright © Amer Koleci and Contributors.
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
    ~SphereShapeSettings() => Dispose(isDisposing: false);

    public float Radius
    {
        get => JPH_SphereShapeSettings_GetRadius(Handle);
        set => JPH_SphereShapeSettings_SetRadius(Handle, value);
    }
}
