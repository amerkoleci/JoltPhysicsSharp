// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class HingeConstraintSettings : TwoBodyConstraintSettings
{
    public HingeConstraintSettings()
        : base(JPH_HingeConstraintSettings_Create())
    {
    }

    public override TwoBodyConstraint CreateConstraint(in Body body1, in Body body2)
    {
        return new HingeConstraint(JPH_HingeConstraintSettings_CreateConstraint(Handle, body1.Handle, body2.Handle));
    }

    public ConstraintSpace Space
    {
        get => JPH_FixedConstraintSettings_GetSpace(Handle);
        set => JPH_FixedConstraintSettings_SetSpace(Handle, value);
    }

    public Vector3 Point1
    {
        get
        {
            JPH_HingeConstraintSettings_GetPoint1(Handle, out Vector3 value);
            return value;
        }
        set
        {
            JPH_HingeConstraintSettings_SetPoint1(Handle, value);
        }
    }

    public Vector3 Point2
    {
        get
        {
            JPH_HingeConstraintSettings_GetPoint2(Handle, out Vector3 value);
            return value;
        }
        set
        {
            JPH_HingeConstraintSettings_SetPoint2(Handle, value);
        }
    }

    public Vector3 HingeAxis1
    {
        get
        {
            JPH_HingeConstraintSettings_GetHingeAxis1(Handle, out Vector3 value);
            return value;
        }
        set
        {
            JPH_HingeConstraintSettings_SetHingeAxis1(Handle, value);
        }
    }

    public Vector3 NormalAxis1
    {
        get
        {
            JPH_HingeConstraintSettings_GetNormalAxis1(Handle, out Vector3 value);
            return value;
        }
        set
        {
            JPH_HingeConstraintSettings_SetNormalAxis1(Handle, value);
        }
    }

    public Vector3 HingeAxis2
    {
        get
        {
            JPH_HingeConstraintSettings_GetHingeAxis2(Handle, out Vector3 value);
            return value;
        }
        set
        {
            JPH_HingeConstraintSettings_SetHingeAxis2(Handle, value);
        }
    }

    public Vector3 NormalAxis2
    {
        get
        {
            JPH_HingeConstraintSettings_GetNormalAxis2(Handle, out Vector3 value);
            return value;
        }
        set
        {
            JPH_HingeConstraintSettings_SetNormalAxis2(Handle, value);
        }
    }

    public void GetPoint1(out Vector3 value)
    {
        JPH_HingeConstraintSettings_GetPoint1(Handle, out value);
    }

    public void GetPoint2(out Vector3 value)
    {
        JPH_HingeConstraintSettings_GetPoint2(Handle, out value);
    }
}

public sealed unsafe class HingeConstraint : TwoBodyConstraint
{
    internal HingeConstraint(nint handle)
        : base(handle)
    {
    }

    public HingeConstraint(HingeConstraintSettings settings, in Body body1, in Body body2)
        : base(JPH_HingeConstraintSettings_CreateConstraint(settings.Handle, body1.Handle, body2.Handle))
    {
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
            JPH_HingeConstraint_GetTotalLambdaRotation(Handle, out float x, out float y);
            return new(x, y);
        }
    }

    public float TotalLambdaRotationLimits => JPH_HingeConstraint_GetTotalLambdaRotationLimits(Handle);
    public float TotalLambdaMotor => JPH_HingeConstraint_GetTotalLambdaMotor(Handle);
}
