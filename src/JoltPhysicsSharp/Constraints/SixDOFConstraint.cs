// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

/// <summary>
/// Constraint is split up into translation/rotation around X, Y and Z axis.
/// </summary>
public enum SixDOFConstraintAxis
{
    TranslationX,
    TranslationY,
    TranslationZ,

    RotationX,
    RotationY,
    RotationZ,

    Count,
    NumTranslation = TranslationZ + 1,
}

public unsafe class SixDOFConstraintSettings : TwoBodyConstraintSettings
{
    public SixDOFConstraintSettings()
    {
        JPH_SixDOFConstraintSettings native;
        JPH_SixDOFConstraintSettings_Init(&native);

        FromNative(native);
    }

    internal SixDOFConstraintSettings(in JPH_SixDOFConstraintSettings native)
    {
        FromNative(native);
    }

    public ConstraintSpace Space { get; set; }
    public Vector3 Position1 { get; set; }
    public Vector3 AxisX1 { get; set; }
    public Vector3 AxisY1 { get; set; }
    public Vector3 Position2 { get; set; }
    public Vector3 AxisX2 { get; set; }
    public Vector3 AxisY2 { get; set; }
    public float[] MaxFriction { get; } = new float[(int)SixDOFConstraintAxis.Count];
    public SwingType SwingType { get; set; }
    public float[] LimitMin { get; } = new float[(int)SixDOFConstraintAxis.Count];
    public float[] LimitMax { get; } = new float[(int)SixDOFConstraintAxis.Count];
    public SpringSettings[] LimitsSpringSettings { get; } = new SpringSettings[(int)SixDOFConstraintAxis.NumTranslation];
    public MotorSettings[] MotorSettings { get; } = new MotorSettings[(int)SixDOFConstraintAxis.Count];

    public void MakeFreeAxis(SixDOFConstraintAxis axis)
    {
        LimitMin[(int)axis] = float.MinValue;
        LimitMax[(int)axis] = float.MaxValue;
    }

    public bool IsFreeAxis(SixDOFConstraintAxis axis)
    {
        return LimitMin[(int)axis] == float.MinValue && LimitMax[(int)axis] == float.MaxValue;
    }

    public void MakeFixedAxis(SixDOFConstraintAxis axis)
    {
        LimitMin[(int)axis] = float.MaxValue;
        LimitMax[(int)axis] = float.MinValue;
    }

    public bool IsFixedAxis(SixDOFConstraintAxis axis)
    {
        return LimitMin[(int)axis] >= LimitMax[(int)axis];
    }

    public void SetLimitedAxis(SixDOFConstraintAxis axis, float min, float max)
    {
        LimitMin[(int)axis] = min;
        LimitMax[(int)axis] = max;
    }

    public override TwoBodyConstraint CreateConstraint(in Body body1, in Body body2)
    {
        return new SixDOFConstraint(CreateConstraintNative(in body1, in body2));
    }

    internal nint CreateConstraintNative(in Body body1, in Body body2)
    {
        JPH_SixDOFConstraintSettings nativeSettings;
        ToNative(&nativeSettings);

        return JPH_SixDOFConstraint_Create(&nativeSettings, body1.Handle, body2.Handle);
    }

    private void FromNative(in JPH_SixDOFConstraintSettings native)
    {
        FromNative(native.baseSettings);

        Space = native.space;
        Position1 = native.position1;
        AxisX1 = native.axisX1;
        AxisY1 = native.axisY1;
        Position2 = native.position2;
        AxisX2 = native.axisX2;
        AxisY2 = native.axisY2;
        SwingType = native.swingType;

        for (int i = 0; i < (int)SixDOFConstraintAxis.Count; i++)
        {
            MaxFriction[i] = native.maxFriction[i];
            LimitMin[i] = native.limitMin[i];
            LimitMax[i] = native.limitMax[i];
            MotorSettings[i] = native.motorSettings[i];
        }

        for (int i = 0; i < (int)SixDOFConstraintAxis.NumTranslation; i++)
        {
            LimitsSpringSettings[i] = native.limitsSpringSettings[i];
        }
    }

    internal void ToNative(JPH_SixDOFConstraintSettings* native)
    {
        ToNative(ref native->baseSettings);

        native->space = Space;
        native->position1 = Position1;
        native->axisX1 = AxisX1;
        native->axisY1 = AxisY1;
        native->position2 = Position2;
        native->axisX2 = AxisX2;
        native->axisY2 = AxisY2;
        native->swingType = SwingType;

        for (int i = 0; i < (int)SixDOFConstraintAxis.Count; i++)
        {
            native->maxFriction[i] = MaxFriction[i];
            native->limitMin[i] = LimitMin[i];
            native->limitMax[i] = LimitMax[i];
            native->motorSettings[i] = MotorSettings[i];
        }

        for (int i = 0; i < (int)SixDOFConstraintAxis.NumTranslation; i++)
        {
            native->limitsSpringSettings[i] = LimitsSpringSettings[i];
        }
    }
}

public sealed class SixDOFConstraint : TwoBodyConstraint
{
    internal SixDOFConstraint(nint handle)
        : base(handle)
    {
    }

    public SixDOFConstraint(SixDOFConstraintSettings settings, in Body body1, in Body body2)
        : base(settings.CreateConstraintNative(in body1, in body2))
    {
    }

    public SixDOFConstraintSettings Settings
    {
        get
        {
            JPH_SixDOFConstraint_GetSettings(Handle, out JPH_SixDOFConstraintSettings native);
            return new(native);
        }
    }

    public float GetLimitsMin(SixDOFConstraintAxis axis) => JPH_SixDOFConstraint_GetLimitsMin(Handle, (uint)axis);
    public float GetLimitsMax(SixDOFConstraintAxis axis) => JPH_SixDOFConstraint_GetLimitsMax(Handle, (uint)axis);

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
