// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class SixDOFConstraintSettings : TwoBodyConstraintSettings
{
    public SixDOFConstraintSettings()
        : base(JPH_SixDOFConstraintSettings_Create())
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="SixDOFConstraintSettings" /> class.
    /// </summary>
    ~SixDOFConstraintSettings() => Dispose(disposing: false);

    public override TwoBodyConstraint CreateConstraint(in Body body1, in Body body2)
    {
        return new SliderConstraint(JPH_SixDOFConstraintSettings_CreateConstraint(Handle, body1.Handle, body2.Handle));
    }
}

public sealed class SixDOFConstraint : TwoBodyConstraint
{
    /// <summary>
    /// Constraint is split up into translation/rotation around X, Y and Z axis.
    /// </summary>
    public enum Axis
    {
        TranslationX,
        TranslationY,
        TranslationZ,

        RotationX,
        RotationY,
        RotationZ,
    }


    internal SixDOFConstraint(nint handle)
        : base(handle)
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="SixDOFConstraint" /> class.
    /// </summary>
    ~SixDOFConstraint() => Dispose(disposing: false);

    public float GetLimitsMin(Axis axis) => JPH_SixDOFConstraint_GetLimitsMin(Handle, (uint)axis);
    public float GetLimitsMax(Axis axis) => JPH_SixDOFConstraint_GetLimitsMax(Handle, (uint)axis);
}
