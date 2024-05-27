// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public unsafe class FixedConstraintSettings : TwoBodyConstraintSettings
{
    public FixedConstraintSettings()
        : base(JPH_FixedConstraintSettings_Create())
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="FixedConstraintSettings" /> class.
    /// </summary>
    ~FixedConstraintSettings() => Dispose(disposing: false);

    public ConstraintSpace Space
    {
        get => JPH_PointConstraintSettings_GetSpace(Handle);
        set => JPH_PointConstraintSettings_SetSpace(Handle, value);
    }

    public bool AutoDetectPoint
    {
        get => JPH_FixedConstraintSettings_GetAutoDetectPoint(Handle);
        set => JPH_FixedConstraintSettings_SetAutoDetectPoint(Handle, value);
    }

    public Vector3 Point1
    {
        get
        {
            Vector3 result;
            JPH_FixedConstraintSettings_GetPoint1(Handle, &result);
            return result;
        }
        set
        {
            JPH_FixedConstraintSettings_SetPoint1(Handle, &value);
        }
    }

    public Vector3 AxisX1
    {
        get
        {
            Vector3 result;
            JPH_FixedConstraintSettings_GetAxisX1(Handle, &result);
            return result;
        }
        set
        {
            JPH_FixedConstraintSettings_SetAxisX1(Handle, &value);
        }
    }

    public Vector3 AxisY1
    {
        get
        {
            Vector3 result;
            JPH_FixedConstraintSettings_GetAxisY1(Handle, &result);
            return result;
        }
        set
        {
            JPH_FixedConstraintSettings_SetAxisY1(Handle, &value);
        }
    }

    public Vector3 Point2
    {
        get
        {
            Vector3 result;
            JPH_FixedConstraintSettings_GetPoint2(Handle, &result);
            return result;
        }
        set
        {
            JPH_FixedConstraintSettings_SetPoint2(Handle, &value);
        }
    }



    public Vector3 AxisX2
    {
        get
        {
            Vector3 result;
            JPH_FixedConstraintSettings_GetAxisX2(Handle, &result);
            return result;
        }
        set
        {
            JPH_FixedConstraintSettings_SetAxisX2(Handle, &value);
        }
    }

    public Vector3 AxisY2
    {
        get
        {
            Vector3 result;
            JPH_FixedConstraintSettings_GetAxisY2(Handle, &result);
            return result;
        }
        set
        {
            JPH_FixedConstraintSettings_SetAxisY2(Handle, &value);
        }
    }

    public override TwoBodyConstraint CreateConstraint(in Body body1, in Body body2)
    {
        return new FixedConstraint(JPH_FixedConstraintSettings_CreateConstraint(Handle, body1.Handle, body2.Handle));
    }
}

public unsafe class FixedConstraint : TwoBodyConstraint
{
    internal FixedConstraint(nint handle)
        : base(handle)
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="FixedConstraint" /> class.
    /// </summary>
    ~FixedConstraint() => Dispose(disposing: false);

    public Vector3 GetTotalLambdaPosition()
    {
        Vector3 result;
        JPH_FixedConstraint_GetTotalLambdaPosition(Handle, &result);
        return result;
    }

    public Vector3 GetTotalLambdaRotation()
    {
        Vector3 result;
        JPH_FixedConstraint_GetTotalLambdaRotation(Handle, &result);
        return result;
    }

    public void GetTotalLambdaPosition(out Vector3 result)
    {
        Unsafe.SkipInit(out result);
        fixed (Vector3* resultPtr = &result)
        {
            JPH_FixedConstraint_GetTotalLambdaPosition(Handle, resultPtr);
        }
    }

    public void GetTotalLambdaRotation(out Vector3 result)
    {
        Unsafe.SkipInit(out result);
        fixed (Vector3* resultPtr = &result)
        {
            JPH_FixedConstraint_GetTotalLambdaRotation(Handle, resultPtr);
        }
    }
}
