// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public unsafe class SliderConstraintSettings : TwoBodyConstraintSettings
{
    public SliderConstraintSettings()
    {
        JPH_SliderConstraintSettings native;
        JPH_SliderConstraintSettings_Init(&native);

        FromNative(native);
    }

    internal SliderConstraintSettings(in JPH_SliderConstraintSettings native)
    {
        FromNative(native);
    }

    public ConstraintSpace Space { get; set; }
    public bool AutoDetectPoint { get; set; }
    public Vector3 Point1 { get; set; }
    public Vector3 SliderAxis1 { get; set; }
    public Vector3 NormalAxis1 { get; set; }
    public Vector3 Point2 { get; set; }
    public Vector3 SliderAxis2 { get; set; }
    public Vector3 NormalAxis2 { get; set; }
    public float LimitsMin { get; set; }
    public float LimitsMax { get; set; }
    public SpringSettings LimitsSpringSettings { get; set; }
    public float MaxFrictionForce { get; set; }
    public MotorSettings MotorSettings { get; set; }

    public override TwoBodyConstraint CreateConstraint(in Body body1, in Body body2)
    {
        return new SliderConstraint(CreateConstraintNative(in body1, in body2));
    }

    public unsafe void SetSliderAxis(in Vector3 axis)
    {
        SliderAxis1 = SliderAxis2 = axis;
        NormalAxis1 = NormalAxis2 = axis.GetNormalizedPerpendicular();
    }

    internal nint CreateConstraintNative(in Body body1, in Body body2)
    {
        JPH_SliderConstraintSettings nativeSettings;
        ToNative(&nativeSettings);

        return JPH_SliderConstraint_Create(&nativeSettings, body1.Handle, body2.Handle);
    }

    private void FromNative(in JPH_SliderConstraintSettings native)
    {
        FromNative(native.baseSettings);

        Space = native.space;
        AutoDetectPoint = native.autoDetectPoint;
        Point1 = native.point1;
        SliderAxis1 = native.sliderAxis1;
        NormalAxis1 = native.normalAxis1;
        Point2 = native.point2;
        SliderAxis2 = native.sliderAxis2;
        NormalAxis2 = native.normalAxis2;
        LimitsMin = native.limitsMin;
        LimitsMax = native.limitsMax;
        LimitsSpringSettings = native.limitsSpringSettings;
        MaxFrictionForce = native.maxFrictionForce;
        MotorSettings = native.motorSettings;
    }

    internal void ToNative(JPH_SliderConstraintSettings* native)
    {
        ToNative(ref native->baseSettings);

        native->space = Space;
        native->autoDetectPoint = AutoDetectPoint;
        native->point1 = Point1;
        native->sliderAxis1 = SliderAxis1;
        native->normalAxis1 = NormalAxis1;
        native->point2 = Point2;
        native->sliderAxis2 = SliderAxis2;
        native->normalAxis2 = NormalAxis2;
        native->limitsMin = LimitsMin;
        native->limitsMax = LimitsMax;
        native->limitsSpringSettings = LimitsSpringSettings;
        native->maxFrictionForce = MaxFrictionForce;
        native->motorSettings = MotorSettings;
    }
}

public sealed unsafe class SliderConstraint : TwoBodyConstraint
{
    internal SliderConstraint(nint handle)
        : base(handle)
    {
    }

    public SliderConstraint(SliderConstraintSettings settings, in Body body1, in Body body2)
        : base(settings.CreateConstraintNative(in body1, in body2))
    {
    }

    public SliderConstraintSettings Settings
    {
        get
        {
            JPH_SliderConstraint_GetSettings(Handle, out JPH_SliderConstraintSettings native);
            return new(native);
        }
    }

    public float CurrentPosition => JPH_SliderConstraint_GetCurrentPosition(Handle);

    public float MaxFrictionForce
    {
        get => JPH_SliderConstraint_GetMaxFrictionForce(Handle);
        set => JPH_SliderConstraint_SetMaxFrictionForce(Handle, value);
    }

    public MotorSettings MotorSettings
    {
        get
        {
            JPH_SliderConstraint_GetMotorSettings(Handle, out MotorSettings settings);
            return settings;
        }
        set => JPH_SliderConstraint_SetMotorSettings(Handle, &value);
    }

    public MotorState MotorState
    {
        get => JPH_SliderConstraint_GetMotorState(Handle);
        set => JPH_SliderConstraint_SetMotorState(Handle, value);
    }

    public float TargetVelocity
    {
        get => JPH_SliderConstraint_GetTargetVelocity(Handle);
        set => JPH_SliderConstraint_SetTargetVelocity(Handle, value);
    }

    public float TargetPosition
    {
        get => JPH_SliderConstraint_GetTargetPosition(Handle);
        set => JPH_SliderConstraint_SetTargetPosition(Handle, value);
    }

    public float LimitsMin => JPH_SliderConstraint_GetLimitsMin(Handle);
    public float LimitsMax => JPH_SliderConstraint_GetLimitsMax(Handle);

    public bool HasLimits => JPH_SliderConstraint_HasLimits(Handle);

    public SpringSettings LimitsSpringSettings
    {
        get
        {
            JPH_SliderConstraint_GetLimitsSpringSettings(Handle, out SpringSettings result);
            return result;
        }
        set => JPH_SliderConstraint_SetLimitsSpringSettings(Handle, &value);
    }

    public Vector2 TotalLambdaPosition
    {
        get
        {
            JPH_SliderConstraint_GetTotalLambdaPosition(Handle, out Vector2 result);
            return result;
        }
    }

    public float TotalLambdaPositionLimits => JPH_SliderConstraint_GetTotalLambdaPositionLimits(Handle);

    public Vector3 TotalLambdaRotation
    {
        get
        {
            JPH_SliderConstraint_GetTotalLambdaRotation(Handle, out Vector3 result);
            return result;
        }
    }

    public float TotalLambdaMotor => JPH_SliderConstraint_GetTotalLambdaMotor(Handle);

    public void SetLimits(float min, float max)
    {
        JPH_SliderConstraint_SetLimits(Handle, min, max);
    }
}
