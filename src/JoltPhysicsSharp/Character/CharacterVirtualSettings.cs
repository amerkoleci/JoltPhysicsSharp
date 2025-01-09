// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class CharacterVirtualSettings : CharacterBaseSettings
{
    public unsafe CharacterVirtualSettings()
    {
        JPH_CharacterVirtualSettings native;
        JPH_CharacterVirtualSettings_Init(&native);

        FromNative(native.baseSettings);

        ID = native.ID;
        Mass = native.mass;
        MaxStrength = native.maxStrength;
        ShapeOffset = native.shapeOffset;
        BackFaceMode = native.backFaceMode;
        PredictiveContactDistance = native.predictiveContactDistance;
        MaxCollisionIterations = native.maxCollisionIterations;
        MaxConstraintIterations = native.maxConstraintIterations;
        MinTimeRemaining = native.minTimeRemaining;
        CollisionTolerance = native.collisionTolerance;
        CharacterPadding = native.characterPadding;
        MaxNumHits = native.maxNumHits;
        HitReductionCosMaxAngle = native.hitReductionCosMaxAngle;
        PenetrationRecoverySpeed = native.penetrationRecoverySpeed;
        InnerBodyShape = native.innerBodyShape != 0 ? Shape.GetObject(native.innerBodyShape) : null;
        InnerBodyIDOverride = native.innerBodyIDOverride;
        InnerBodyLayer = native.innerBodyLayer;
    }

    public CharacterID ID { get; set; }
    public float Mass { get; set; }

    public float MaxStrength { get; set; }

    public Vector3 ShapeOffset { get; set; }

    public BackFaceMode BackFaceMode { get; set; }

    public float PredictiveContactDistance { get; set; }

    public uint MaxCollisionIterations { get; set; }

    public uint MaxConstraintIterations { get; set; }

    public float MinTimeRemaining { get; set; }

    public float CollisionTolerance { get; set; }

    public float CharacterPadding { get; set; }

    public uint MaxNumHits { get; set; }

    public float HitReductionCosMaxAngle { get; set; }

    public float PenetrationRecoverySpeed { get; set; }

    public Shape? InnerBodyShape { get; set; }
    public BodyID InnerBodyIDOverride { get; set; }
    public ObjectLayer InnerBodyLayer { get; set; }

    internal unsafe void ToNative(JPH_CharacterVirtualSettings* native)
    {
        native->baseSettings.up = Up;
        native->baseSettings.supportingVolume = SupportingVolume;
        native->baseSettings.maxSlopeAngle = MaxSlopeAngle;
        native->baseSettings.enhancedInternalEdgeRemoval = EnhancedInternalEdgeRemoval;
        native->baseSettings.shape = Shape != null ? Shape.Handle : 0;

        native->ID = ID;
        native->mass = Mass;
        native->maxStrength = MaxStrength;
        native->shapeOffset = ShapeOffset;
        native->backFaceMode = BackFaceMode;
        native->predictiveContactDistance = PredictiveContactDistance;
        native->maxCollisionIterations = MaxCollisionIterations;
        native->maxConstraintIterations = MaxConstraintIterations;
        native->minTimeRemaining = MinTimeRemaining;
        native->collisionTolerance = CollisionTolerance;
        native->characterPadding = CharacterPadding;
        native->maxNumHits = MaxNumHits;
        native->hitReductionCosMaxAngle = HitReductionCosMaxAngle;
        native->penetrationRecoverySpeed = PenetrationRecoverySpeed;
        native->innerBodyShape = InnerBodyShape != null ? InnerBodyShape.Handle : 0;
        native->innerBodyIDOverride = InnerBodyIDOverride;
        native->innerBodyLayer = InnerBodyLayer;
    }
}

