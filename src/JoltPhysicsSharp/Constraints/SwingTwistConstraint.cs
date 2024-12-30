// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class SwingTwistConstraintSettings : TwoBodyConstraintSettings
{
    public SwingTwistConstraintSettings()
        : base(JPH_SwingTwistConstraintSettings_Create())
    {
    }

    public override TwoBodyConstraint CreateConstraint(in Body body1, in Body body2)
    {
        return new SliderConstraint(JPH_SwingTwistConstraintSettings_CreateConstraint(Handle, body1.Handle, body2.Handle));
    }
}

public sealed class SwingTwistConstraint : TwoBodyConstraint
{
    internal SwingTwistConstraint(nint handle)
        : base(handle)
    {
    }

    public SwingTwistConstraint(SwingTwistConstraintSettings settings, in Body body1, in Body body2)
        : base(JPH_SwingTwistConstraintSettings_CreateConstraint(settings.Handle, body1.Handle, body2.Handle))
    {
    }

    public float GetNormalHalfConeAngle() => JPH_SwingTwistConstraint_GetNormalHalfConeAngle(Handle);

    public Vector3 TotalLambdaPosition
    {
        get
        {
            JPH_SwingTwistConstraint_GetTotalLambdaPosition(Handle, out Vector3 value);
            return value;
        }
    }
    public float TotalLambdaTwist => JPH_SwingTwistConstraint_GetTotalLambdaTwist(Handle);

    public float TotalLambdaSwingY => JPH_SwingTwistConstraint_GetTotalLambdaSwingY(Handle);

    public float TotalLambdaSwingZ => JPH_SwingTwistConstraint_GetTotalLambdaSwingZ(Handle);

    public Vector3 TotalLambdaMotor
    {
        get
        {
            JPH_SwingTwistConstraint_GetTotalLambdaMotor(Handle, out Vector3 value);
            return value;
        }
    }
}
