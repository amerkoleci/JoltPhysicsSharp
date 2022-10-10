// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class SphereShape : ConvexShape
{
    public unsafe SphereShape(float radius)
        : base(JPH_SphereShape_Create(radius))
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="SphereShape" /> class.
    /// </summary>
    ~SphereShape() => Dispose(isDisposing: false);

    public float Radius
    {
        get => JPH_SphereShape_GetRadius(Handle);
    }
}
