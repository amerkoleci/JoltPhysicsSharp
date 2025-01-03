// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public unsafe class HingeConstraintSettings : TwoBodyConstraintSettings
{
    public HingeConstraintSettings()
    {
        JPH_HingeConstraintSettings native;
        JPH_HingeConstraintSettings_Init(&native);

        FromNative(native);
    }

    internal HingeConstraintSettings(in JPH_HingeConstraintSettings native)
    {
        FromNative(native);
    }

    public ConstraintSpace Space { get; set; }

    public Vector3 Point1 { get; set; }

    public Vector3 Point2 { get; set; }

    public Vector3 HingeAxis1 { get; set; }

    public Vector3 NormalAxis1 { get; set; }
    public Vector3 HingeAxis2 { get; set; }
    public Vector3 NormalAxis2 { get; set; }
    public float LimitsMin { get; set; }
    public float LimitsMax { get; set; }
    public SpringSettings LimitsSpringSettings { get; set; }
    public float MaxFrictionTorque { get; set; }
    public MotorSettings MotorSettings { get; set; }

    public override TwoBodyConstraint CreateConstraint(in Body body1, in Body body2)
    {
        return new HingeConstraint(CreateConstraintNative(in body1, in body2));
    }

    internal nint CreateConstraintNative(in Body body1, in Body body2)
    {
        JPH_HingeConstraintSettings nativeSettings;
        ToNative(&nativeSettings);

        return JPH_HingeConstraint_Create(&nativeSettings, body1.Handle, body2.Handle);
    }

    private void FromNative(in JPH_HingeConstraintSettings native)
    {
        FromNative(native.baseSettings);

        Space = native.space;
        Point1 = native.point1;
        HingeAxis1 = native.hingeAxis1;
        NormalAxis1 = native.normalAxis1;
        Point2 = native.point2;
        HingeAxis2 = native.hingeAxis2;
        NormalAxis2 = native.normalAxis2;
        LimitsMin = native.limitsMin;
        LimitsMax = native.limitsMax;
        LimitsSpringSettings = native.limitsSpringSettings;
        MaxFrictionTorque = native.maxFrictionTorque;
        MotorSettings = native.motorSettings;
    }

    internal void ToNative(JPH_HingeConstraintSettings* native)
    {
        ToNative(ref native->baseSettings);

        native->space = Space;
        native->point1 = Point1;
        native->hingeAxis1 = HingeAxis1;
        native->normalAxis1 = NormalAxis1;
        native->point2 = Point2;
        native->hingeAxis2 = HingeAxis2;
        native->normalAxis2 = NormalAxis2;
        native->limitsMin = LimitsMin;
        native->limitsMax = LimitsMax;
        native->limitsSpringSettings = LimitsSpringSettings;
        native->maxFrictionTorque = MaxFrictionTorque;
        native->motorSettings = MotorSettings;
    }
}

public sealed unsafe class HingeConstraint : TwoBodyConstraint
{
    internal HingeConstraint(nint handle)
        : base(handle)
    {
    }

    public HingeConstraint(HingeConstraintSettings settings, in Body body1, in Body body2)
        : base(settings.CreateConstraintNative(in body1, in body2))
    {
    }

    public HingeConstraintSettings Settings
    {
        get
        {
            JPH_HingeConstraint_GetSettings(Handle, out JPH_HingeConstraintSettings native);
            return new(native);
        }
    }

    public Vector3 LocalSpacePoint1
    {
        get
        {
            JPH_HingeConstraint_GetLocalSpacePoint1(Handle, out Vector3 result);
            return result;
        }
    }

    public Vector3 LocalSpacePoint2
    {
        get
        {
            JPH_HingeConstraint_GetLocalSpacePoint2(Handle, out Vector3 result);
            return result;
        }
    }

    public Vector3 LocalSpaceHingeAxis1
    {
        get
        {
            JPH_HingeConstraint_GetLocalSpaceHingeAxis1(Handle, out Vector3 result);
            return result;
        }
    }

    public Vector3 LocalSpaceHingeAxis2
    {
        get
        {
            JPH_HingeConstraint_GetLocalSpaceHingeAxis2(Handle, out Vector3 result);
            return result;
        }
    }

    public Vector3 LocalSpaceNormalAxis1
    {
        get
        {
            JPH_HingeConstraint_GetLocalSpaceNormalAxis1(Handle, out Vector3 result);
            return result;
        }
    }

    public Vector3 LocalSpaceNormalAxis2
    {
        get
        {
            JPH_HingeConstraint_GetLocalSpaceNormalAxis2(Handle, out Vector3 result);
            return result;
        }
    }

    public float CurrentAngle => JPH_HingeConstraint_GetCurrentAngle(Handle);

    public float MaxFrictionTorque
    {
        get => JPH_HingeConstraint_GetMaxFrictionTorque(Handle);
        set => JPH_HingeConstraint_SetMaxFrictionTorque(Handle, value);
    }

    public MotorSettings MotorSettings
    {
        get
        {
            JPH_HingeConstraint_GetMotorSettings(Handle, out MotorSettings result);
            return result;
        }
        set => JPH_HingeConstraint_SetMotorSettings(Handle, &value);
    }

    public MotorState MotorState
    {
        get => JPH_HingeConstraint_GetMotorState(Handle);
        set => JPH_HingeConstraint_SetMotorState(Handle, value);
    }

    public float TargetAngularVelocity
    {
        get => JPH_HingeConstraint_GetTargetAngularVelocity(Handle);
        set => JPH_HingeConstraint_SetTargetAngularVelocity(Handle, value);
    }

    public float TargetAngle
    {
        get => JPH_HingeConstraint_GetTargetAngle(Handle);
        set => JPH_HingeConstraint_SetTargetAngle(Handle, value);
    }

    public void SetLimits(float min, float max)
    {
        JPH_HingeConstraint_SetLimits(Handle, min, max);
    }

    public float LimitsMin => JPH_HingeConstraint_GetLimitsMin(Handle);
    public float LimitsMax => JPH_HingeConstraint_GetLimitsMax(Handle);
    public bool HasLimits => JPH_HingeConstraint_HasLimits(Handle);

    public SpringSettings LimitsSpringSettings
    {
        get
        {
            JPH_HingeConstraint_GetLimitsSpringSettings(Handle, out SpringSettings result);
            return result;
        }
        set => JPH_HingeConstraint_SetLimitsSpringSettings(Handle, &value);
    }

    public Vector3 TotalLambdaPosition
    {
        get
        {
            JPH_HingeConstraint_GetTotalLambdaPosition(Handle, out Vector3 result);
            return result;
        }
    }

    public Vector2 TotalLambdaRotation
    {
        get
        {
            JPH_HingeConstraint_GetTotalLambdaRotation(Handle, out Vector2 rotation);
            return rotation;
        }
    }

    public float TotalLambdaRotationLimits => JPH_HingeConstraint_GetTotalLambdaRotationLimits(Handle);
    public float TotalLambdaMotor => JPH_HingeConstraint_GetTotalLambdaMotor(Handle);
}
