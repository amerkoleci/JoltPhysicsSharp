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

    public SixDOFConstraint(SixDOFConstraintSettings settings, in Body body1, in Body body2)
        : base(JPH_SixDOFConstraintSettings_CreateConstraint(settings.Handle, body1.Handle, body2.Handle))
    {
    }

    public float GetLimitsMin(Axis axis) => JPH_SixDOFConstraint_GetLimitsMin(Handle, (uint)axis);
    public float GetLimitsMax(Axis axis) => JPH_SixDOFConstraint_GetLimitsMax(Handle, (uint)axis);

    public Vector3 TotalLambdaPosition
    {
        get
        {
            JPH_SixDOFConstraint_GetTotalLambdaPosition(Handle, out Vector3 value);
            return value;
        }
    }

    public Vector3 TotalLambdaRotation
    {
        get
        {
            JPH_SixDOFConstraint_GetTotalLambdaRotation(Handle, out Vector3 value);
            return value;
        }
    }

    public Vector3 TotalLambdaMotorTranslation
    {
        get
        {
            JPH_SixDOFConstraint_GetTotalLambdaMotorTranslation(Handle, out Vector3 value);
            return value;
        }
    }

    public Vector3 TotalLambdaMotorRotation
    {
        get
        {
            JPH_SixDOFConstraint_GetTotalLambdaMotorRotation(Handle, out Vector3 value);
            return value;
        }
    }
}
