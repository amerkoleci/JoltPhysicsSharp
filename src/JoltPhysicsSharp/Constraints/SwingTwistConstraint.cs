// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public unsafe class SwingTwistConstraintSettings : TwoBodyConstraintSettings
{
    public SwingTwistConstraintSettings()
    {
        JPH_SwingTwistConstraintSettings native;
        JPH_SwingTwistConstraintSettings_Init(&native);

        FromNative(native);
    }

    internal SwingTwistConstraintSettings(in JPH_SwingTwistConstraintSettings native)
    {
        FromNative(native);
    }

    public ConstraintSpace Space { get; set; }
    public Vector3 Position1 { get; set; }
    public Vector3 TwistAxis1 { get; set; }
    public Vector3 PlaneAxis1 { get; set; }
    public Vector3 Position2 { get; set; }
    public Vector3 TwistAxis2 { get; set; }
    public Vector3 PlaneAxis2 { get; set; }
    public SwingType SwingType { get; set; }
    public float NormalHalfConeAngle { get; set; }
    public float PlaneHalfConeAngle { get; set; }
    public float TwistMinAngle { get; set; }
    public float TwistMaxAngle { get; set; }
    public float MaxFrictionTorque { get; set; }
    public MotorSettings SwingMotorSettings { get; set; }
    public MotorSettings TwistMotorSettings { get; set; }

    public override TwoBodyConstraint CreateConstraint(in Body body1, in Body body2)
    {
        return new SwingTwistConstraint(CreateConstraintNative(in body1, in body2));
    }

    internal nint CreateConstraintNative(in Body body1, in Body body2)
    {
        JPH_SwingTwistConstraintSettings nativeSettings;
        ToNative(&nativeSettings);

        return JPH_SwingTwistConstraint_Create(&nativeSettings, body1.Handle, body2.Handle);
    }

    private void FromNative(in JPH_SwingTwistConstraintSettings native)
    {
        FromNative(native.baseSettings);

        Space = native.space;
        Position1 = native.position1;
        TwistAxis1 = native.twistAxis1;
        PlaneAxis1 = native.planeAxis1;
        Position2 = native.position2;
        TwistAxis2 = native.twistAxis2;
        PlaneAxis2 = native.planeAxis2;
        SwingType = native.swingType;
        NormalHalfConeAngle = native.normalHalfConeAngle;
        PlaneHalfConeAngle = native.planeHalfConeAngle;
        TwistMinAngle = native.twistMinAngle;
        TwistMaxAngle = native.twistMaxAngle;
        MaxFrictionTorque = native.maxFrictionTorque;
        SwingMotorSettings = native.swingMotorSettings;
        TwistMotorSettings = native.twistMotorSettings;
    }

    internal void ToNative(JPH_SwingTwistConstraintSettings* native)
    {
        ToNative(ref native->baseSettings);

        native->space = Space;
        native->position1 = Position1;
        native->twistAxis1 = TwistAxis1;
        native->planeAxis1 = PlaneAxis1;
        native->position2 = Position2;
        native->twistAxis2 = TwistAxis2;
        native->planeAxis2 = PlaneAxis2;
        native->swingType = SwingType;
        native->normalHalfConeAngle = NormalHalfConeAngle;
        native->planeHalfConeAngle = PlaneHalfConeAngle;
        native->twistMinAngle = TwistMinAngle;
        native->twistMaxAngle = TwistMaxAngle;
        native->maxFrictionTorque = MaxFrictionTorque;
        native->swingMotorSettings = SwingMotorSettings;
        native->twistMotorSettings = TwistMotorSettings;
    }
}

public sealed class SwingTwistConstraint : TwoBodyConstraint
{
    internal SwingTwistConstraint(nint handle)
        : base(handle)
    {
    }

    public SwingTwistConstraint(SwingTwistConstraintSettings settings, in Body body1, in Body body2)
        : base(settings.CreateConstraintNative(in body1, in body2))
    {
    }

    public SwingTwistConstraintSettings Settings
    {
        get
        {
            JPH_SwingTwistConstraint_GetSettings(Handle, out JPH_SwingTwistConstraintSettings native);
            return new(native);
        }
    }

    public float NormalHalfConeAngle => JPH_SwingTwistConstraint_GetNormalHalfConeAngle(Handle);

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
