// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public class ConstraintSettings : NativeObject
{
    protected ConstraintSettings()
    {
    }

    internal ConstraintSettings(nint handle)
        : base(handle)
    {
    }

    protected override void DisposeNative()
    {
        JPH_ConstraintSettings_Destroy(Handle);
    }

    public bool Enabled
    {
        get => JPH_ConstraintSettings_GetEnabled(Handle);
        set => JPH_ConstraintSettings_SetEnabled(Handle, value);
    }

    public uint ConstraintPriority
    {
        get => JPH_ConstraintSettings_GetConstraintPriority(Handle);
        set => JPH_ConstraintSettings_SetConstraintPriority(Handle, value);
    }

    public uint NumVelocityStepsOverride
    {
        get => JPH_ConstraintSettings_GetNumVelocityStepsOverride(Handle);
        set => JPH_ConstraintSettings_SetNumVelocityStepsOverride(Handle, value);
    }

    public uint NumPositionStepsOverride
    {
        get => JPH_ConstraintSettings_GetNumPositionStepsOverride(Handle);
        set => JPH_ConstraintSettings_SetNumPositionStepsOverride(Handle, value);
    }

    public float DrawConstraintSize
    {
        get => JPH_ConstraintSettings_GetDrawConstraintSize(Handle);
        set => JPH_ConstraintSettings_SetDrawConstraintSize(Handle, value);
    }

    public ulong UserData
    {
        get => JPH_ConstraintSettings_GetUserData(Handle);
        set => JPH_ConstraintSettings_SetUserData(Handle, value);
    }

    internal static ConstraintSettings? GetObject(nint handle)
    {
        return GetOrAddObject(handle, (nint h) => new ConstraintSettings(h));
    }
}

public abstract class Constraint : NativeObject
{
    protected Constraint(nint handle)
        : base(handle)
    {
    }

    protected override void DisposeNative()
    {
        JPH_Constraint_Destroy(Handle);
    }

    public ConstraintType Type
    {
        get => JPH_Constraint_GetType(Handle);
    }

    public ConstraintSubType SubType
    {
        get => JPH_Constraint_GetSubType(Handle);
    }

    public ConstraintSettings? ConstraintSettings
    {
        get => ConstraintSettings.GetObject(JPH_Constraint_GetConstraintSettings(Handle));
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

    public void NotifyShapeChanged(in BodyID bodyID, in Vector3 deltaCOM)
    {
        JPH_Constraint_NotifyShapeChanged(Handle, bodyID, in deltaCOM);
    }
}
