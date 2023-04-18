// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

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
}
