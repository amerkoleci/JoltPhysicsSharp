// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class CharacterSettings : CharacterBaseSettings
{
    public unsafe CharacterSettings()
    {
        JPH_CharacterSettings native;
        JPH_CharacterSettings_Init(&native);

        FromNative(native.baseSettings);

        Layer = native.layer;
        Mass = native.mass;
        Friction = native.friction;
        GravityFactor = native.gravityFactor;
        AllowedDOFs = native.allowedDOFs;
    }

    public ObjectLayer Layer { get; set; }

    public float Mass { get; set; }

    public float Friction { get; set; }

    public float GravityFactor { get; set; }

    public AllowedDOFs AllowedDOFs { get; set; }

    internal unsafe void ToNative(JPH_CharacterSettings* native)
    {
        native->baseSettings.up = Up;
        native->baseSettings.supportingVolume = SupportingVolume;
        native->baseSettings.maxSlopeAngle = MaxSlopeAngle;
        native->baseSettings.enhancedInternalEdgeRemoval = EnhancedInternalEdgeRemoval;
        native->baseSettings.shape = Shape != null ? Shape.Handle : 0;

        native->layer = Layer;
        native->mass = Mass;
        native->friction = Friction;
        native->gravityFactor = GravityFactor;
        native->allowedDOFs = AllowedDOFs;
    }
}

