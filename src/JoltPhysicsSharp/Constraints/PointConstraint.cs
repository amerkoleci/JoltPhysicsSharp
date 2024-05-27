// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed unsafe class PointConstraintSettings : TwoBodyConstraintSettings
{
    public PointConstraintSettings()
        : base(JPH_PointConstraintSettings_Create())
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="PointConstraintSettings" /> class.
    /// </summary>
    ~PointConstraintSettings() => Dispose(disposing: false);

    public override TwoBodyConstraint CreateConstraint(in Body body1, in Body body2)
    {
        return new PointConstraint(JPH_PointConstraintSettings_CreateConstraint(Handle, body1.Handle, body2.Handle));
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
            Vector3 result;
            JPH_PointConstraintSettings_GetPoint1(Handle, &result);
            return result;
        }
        set
        {
            JPH_PointConstraintSettings_SetPoint1(Handle, &value);
        }
    }

    public Vector3 Point2
    {
        get
        {
            Vector3 result;
            JPH_PointConstraintSettings_GetPoint2(Handle, &result);
            return result;
        }
        set
        {
            JPH_PointConstraintSettings_SetPoint2(Handle, &value);
        }
    }

    public void GetPoint1(out Vector3 value)
    {
        Unsafe.SkipInit(out value);

        fixed (Vector3* valuePtr = &value)
            JPH_PointConstraintSettings_GetPoint1(Handle, valuePtr);
    }

    public void GetPoint2(out Vector3 value)
    {
        Unsafe.SkipInit(out value);

        fixed (Vector3* valuePtr = &value)
            JPH_PointConstraintSettings_GetPoint2(Handle, valuePtr);
    }
}

public sealed unsafe class PointConstraint : TwoBodyConstraint
{
    internal PointConstraint(nint handle)
        : base(handle)
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="PointConstraint" /> class.
    /// </summary>
    ~PointConstraint() => Dispose(disposing: false);

    public void SetPoint1(ConstraintSpace space, in Vector3 value)
    {
        fixed (Vector3* valuePtr = &value)
            JPH_PointConstraint_SetPoint1(Handle, space, valuePtr);
    }

    public void SetPoint2(ConstraintSpace space, in Vector3 value)
    {
        fixed (Vector3* valuePtr = &value)
            JPH_PointConstraint_SetPoint2(Handle, space, valuePtr);
    }

    public Vector3 GetTotalLambdaPosition()
    {
        Vector3 result;
        JPH_PointConstraint_GetTotalLambdaPosition(Handle, &result);
        return result;
    }
}
