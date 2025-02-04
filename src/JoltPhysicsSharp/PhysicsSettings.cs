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
    public float ManifoldTolerance;
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
    public Bool8 DeterministicSimulation;
    public Bool8 ConstraintWarmStart;
    public Bool8 UseBodyPairContactCache;
    public Bool8 UseManifoldReduction;
    public Bool8 UseLargeIslandSplitter;
    public Bool8 AllowSleeping;
    public Bool8 CheckActiveEdges;
}
