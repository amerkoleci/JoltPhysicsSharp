// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class ConstraintSettings : NativeObject
{
    protected ConstraintSettings()
    {
    }

    protected ConstraintSettings(nint handle)
        : base(handle)
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="ConstraintSettings" /> class.
    /// </summary>
    ~ConstraintSettings() => Dispose(isDisposing: false);

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            JPH_ConstraintSettings_Destroy(Handle);
        }
    }
}


public abstract class Constraint : NativeObject
{
    protected Constraint(IntPtr handle)
        : base(handle)
    {
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="Constraint" /> class.
    /// </summary>
    ~Constraint() => Dispose(isDisposing: false);

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            JPH_Constraint_Destroy(Handle);
        }
    }
}
