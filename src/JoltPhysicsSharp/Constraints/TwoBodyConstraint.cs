// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class TwoBodyConstraintSettings : ConvexShapeSettings
{
    protected TwoBodyConstraintSettings(IntPtr handle)
        : base(handle)
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="TwoBodyConstraintSettings" /> class.
    /// </summary>
    ~TwoBodyConstraintSettings() => Dispose(isDisposing: false);

    public abstract TwoBodyConstraint CreateConstraint(in Body body1, in Body body2);
}

public abstract class TwoBodyConstraint : Constraint
{
    protected TwoBodyConstraint(IntPtr handle)
        : base(handle)
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="TwoBodyConstraint" /> class.
    /// </summary>
    ~TwoBodyConstraint() => Dispose(isDisposing: false);

    public Body Body1
    {
        get => JPH_TwoBodyConstraint_GetBody1(Handle);
    }

    public Body Body2
    {
        get => JPH_TwoBodyConstraint_GetBody2(Handle);
    }

    public Matrix4x4 GetConstraintToBody1Matrix()
    {
        JPH_TwoBodyConstraint_GetConstraintToBody1Matrix(Handle, out Matrix4x4 result);
        return result;
    }

    public Matrix4x4 GetConstraintToBody2Matrix()
    {
        JPH_TwoBodyConstraint_GetConstraintToBody2Matrix(Handle, out Matrix4x4 result);
        return result;
    }
}
