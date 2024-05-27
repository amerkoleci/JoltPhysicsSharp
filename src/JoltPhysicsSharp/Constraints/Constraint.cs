// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
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
    ~ConstraintSettings() => Dispose(disposing: false);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
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
    ~Constraint() => Dispose(disposing: false);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            JPH_Constraint_Destroy(Handle);
        }
    }

    public ConstraintType Type
    {
        get => JPH_Constraint_GetType(Handle);
    }

    public ConstraintSubType SubType
    {
        get => JPH_Constraint_GetSubType(Handle);
    }

    // TODO: Handle type of settings here stuff here
    public nint ConstraintSettings
    {
        get => JPH_Constraint_GetConstraintSettings(Handle);
    }

    public bool Enabled
    {
        get => JPH_Constraint_GetEnabled(Handle);
        set => JPH_Constraint_SetEnabled(Handle, value);
    }

    public uint Priority
    {
        get => JPH_Constraint_GetConstraintPriority(Handle);
        set => JPH_Constraint_SetConstraintPriority(Handle, value);
    }

    public ulong UserData
    {
        get => JPH_Constraint_GetUserData(Handle);
        set => JPH_Constraint_SetUserData(Handle, value);
    }

    public unsafe void NotifyShapeChanged(in BodyID bodyID, in Vector3 deltaCOM)
    {
        fixed (Vector3* deltaCOMPtr = &deltaCOM)
        {
            JPH_Constraint_NotifyShapeChanged(Handle, bodyID, deltaCOMPtr);
        }
    }
}
