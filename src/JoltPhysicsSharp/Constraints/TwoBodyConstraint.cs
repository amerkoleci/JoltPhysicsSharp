// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class TwoBodyConstraintSettings : ConstraintSettings
{
    protected TwoBodyConstraintSettings(IntPtr handle)
        : base(handle)
    {
    }

    public abstract TwoBodyConstraint CreateConstraint(in Body body1, in Body body2);
}

public abstract unsafe class TwoBodyConstraint : Constraint
{
    protected TwoBodyConstraint(IntPtr handle)
        : base(handle)
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="TwoBodyConstraint" /> class.
    /// </summary>
    ~TwoBodyConstraint() => Dispose(disposing: false);

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
        Matrix4x4 result;
        JPH_TwoBodyConstraint_GetConstraintToBody1Matrix(Handle, &result);
        return result;
    }

    public void GetConstraintToBody1Matrix(out Matrix4x4 result)
    {
        Unsafe.SkipInit(out result);

        fixed (Matrix4x4* resultPtr = &result)
            JPH_TwoBodyConstraint_GetConstraintToBody1Matrix(Handle, resultPtr);
    }

    public Matrix4x4 GetConstraintToBody2Matrix()
    {
        Matrix4x4 result;
        JPH_TwoBodyConstraint_GetConstraintToBody2Matrix(Handle, &result);
        return result;
    }

    public void GetConstraintToBody2Matrix(out Matrix4x4 result)
    {
        Unsafe.SkipInit(out result);

        fixed (Matrix4x4* resultPtr = &result)
            JPH_TwoBodyConstraint_GetConstraintToBody2Matrix(Handle, resultPtr);
    }
}
