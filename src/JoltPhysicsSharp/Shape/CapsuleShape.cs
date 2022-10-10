// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class CapsuleShapeSettings : ConvexShapeSettings
{
    public unsafe CapsuleShapeSettings(float halfHeightOfCylinder, float radius)
        : base(JPH_CapsuleShapeSettings_Create(halfHeightOfCylinder, radius))
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="CapsuleShapeSettings" /> class.
    /// </summary>
    ~CapsuleShapeSettings() => Dispose(isDisposing: false);
}

