// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace JoltPhysicsSharp;

public record struct PhysicsSettings
{
    public int MaxInFlightBodyPairs;
    public int StepListenersBatchSize;
    public int StepListenerBatchesPerJob;
    public float Baumgarte;
    public float SpeculativeContactDistance;
    public float PenetrationSlop;
    public float LinearCastThreshold;
    public float LinearCastMaxPenetration;
    public float ManifoldToleranceSq;
    public float MaxPenetrationDistance;
    public float BodyPairCacheMaxDeltaPositionSq;
    public float BodyPairCacheCosMaxDeltaRotationDiv2;
    public float ContactNormalCosMaxDeltaRotation;
    public float ContactPointPreserveLambdaMaxDistSq;
    public uint NumVelocitySteps;
    public uint NumPositionSteps;
    public float MinVelocityForRestitution;
    public float TimeBeforeSleep;
    public float PointVelocitySleepThreshold;
    public Bool32 DeterministicSimulation;
    public Bool32 ConstraintWarmStart;
    public Bool32 UseBodyPairContactCache;
    public Bool32 UseManifoldReduction;
    public Bool32 UseLargeIslandSplitter;
    public Bool32 AllowSleeping;
    public Bool32 CheckActiveEdges;
}
