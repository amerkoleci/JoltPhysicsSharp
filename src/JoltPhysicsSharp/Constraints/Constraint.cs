// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class ConstraintSettings
{
    internal void FromNative(in JPH_ConstraintSettings settings)
    {
        Enabled = settings.enabled;
        ConstraintPriority = settings.constraintPriority;
        NumVelocityStepsOverride = settings.numVelocityStepsOverride;
        NumPositionStepsOverride = settings.numPositionStepsOverride;
        DrawConstraintSize = settings.drawConstraintSize;
        UserData = settings.userData;
    }

    internal void ToNative(ref JPH_ConstraintSettings native)
    {
        native.enabled = Enabled;
        native.constraintPriority = ConstraintPriority;
        native.numVelocityStepsOverride = NumVelocityStepsOverride;
        native.numPositionStepsOverride = NumPositionStepsOverride;
        native.drawConstraintSize = DrawConstraintSize;
        native.userData = UserData;
    }

    public bool Enabled { get; set; }

    public uint ConstraintPriority { get; set; }

    public uint NumVelocityStepsOverride { get; set; }
    public uint NumPositionStepsOverride { get; set; }

    public float DrawConstraintSize { get; set; }

    public ulong UserData { get; set; }
}

public abstract class Constraint : NativeObject
{
    protected Constraint(nint handle, bool ownsHandle = true)
        : base(handle, ownsHandle)
    {
    }

    protected override void DisposeNative()
    {
        JPH_Constraint_Destroy(Handle);
    }

    public ConstraintType Type => JPH_Constraint_GetType(Handle);

    public ConstraintSubType SubType => JPH_Constraint_GetSubType(Handle);

    public uint NumVelocityStepsOverride
    {
        get => JPH_Constraint_GetNumVelocityStepsOverride(Handle);
        set => JPH_Constraint_SetNumVelocityStepsOverride(Handle, value);
    }

    public uint NumPositionStepsOverride
    {
        get => JPH_Constraint_GetNumPositionStepsOverride(Handle);
        set => JPH_Constraint_SetNumPositionStepsOverride(Handle, value);
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

    public void ResetWarmStart()
    {
        JPH_Constraint_ResetWarmStart(Handle);
    }

    public void SetupVelocityConstraint(float deltaTime)
    {
        JPH_Constraint_SetupVelocityConstraint(Handle, deltaTime);
    }

    public void WarmStartVelocityConstraint(float warmStartImpulseRatio)
    {
        JPH_Constraint_WarmStartVelocityConstraint(Handle, warmStartImpulseRatio);
    }

    public bool SolveVelocityConstraint(float deltaTime)
    {
        return JPH_Constraint_SolveVelocityConstraint(Handle, deltaTime);
    }

    public bool SolvePositionConstraint(float deltaTime, float baumgarte)
    {
        return JPH_Constraint_SolvePositionConstraint(Handle, deltaTime, baumgarte);
    }
}
