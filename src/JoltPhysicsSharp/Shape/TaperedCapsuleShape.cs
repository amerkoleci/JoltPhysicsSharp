// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class TaperedCapsuleShapeSettings : ConvexShapeSettings
{
    public TaperedCapsuleShapeSettings(float halfHeightOfTaperedCylinder, float topRadius, float bottomRadius)
        : base(JPH_TaperedCapsuleShapeSettings_Create(halfHeightOfTaperedCylinder, topRadius, bottomRadius))
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="TaperedCapsuleShapeSettings" /> class.
    /// </summary>
    ~TaperedCapsuleShapeSettings() => Dispose(isDisposing: false);
}
