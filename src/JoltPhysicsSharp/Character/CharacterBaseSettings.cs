// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public abstract class CharacterBaseSettings
{
    protected CharacterBaseSettings()
    {
    }

    internal void FromNative(in JPH_CharacterBaseSettings settings)
    {
        Up = settings.up;
        SupportingVolume = settings.supportingVolume;
        MaxSlopeAngle = settings.maxSlopeAngle;
        EnhancedInternalEdgeRemoval = settings.enhancedInternalEdgeRemoval;
        //Shape = settings.shape != 0 ? new 
    }


    public Vector3 Up { get; set; }

    public Plane SupportingVolume { get; set; }

    public float MaxSlopeAngle { get; set; }

    public bool EnhancedInternalEdgeRemoval { get; set; }

    public Shape? Shape { get; set; }
}
