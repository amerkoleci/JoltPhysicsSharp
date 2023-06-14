// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public static class Foundation
{
    /// <summary>
    /// If objects are closer than this distance, they are considered to be colliding (used for GJK) (unit: meter)
    /// </summary>
    public const float DefaultCollisionTolerance = 1.0e-4f;

    /// <summary>
    /// A factor that determines the accuracy of the penetration depth calculation. If the change of the squared distance is less than tolerance * current_penetration_depth^2 the algorithm will terminate. (unit: dimensionless)
    /// </summary>
    public const float DefaultPenetrationTolerance = 1.0e-4f;

    /// <summary>
    /// How much padding to add around objects
    /// </summary>
    public const float DefaultConvexRadius = 0.05f;

    /// <summary>
    /// Used by (Tapered)CapsuleShape to determine when supporting face is an edge rather than a point (unit: meter)
    /// </summary>
    public const float CapsuleProjectionSlop = 0.02f;

    /// <summary>
    /// Maximum amount of jobs to allow
    /// </summary>
    public const int MaxPhysicsJobs = 2048;

    /// <summary>
    ///  Maximum amount of barriers to allow
    /// </summary>
    public const int MaxPhysicsBarriers = 8;

    public delegate bool AssertFailedImpl(string inExpression, string inMessage, string inFile, uint inLine);

    public static bool Init(bool doublePrecision = false)
    {
        JoltApi.DoublePrecision = doublePrecision;
#if !NET6_0_OR_GREATER
        JoltApi.LoadNativeLibrary();
#endif

        return JPH_Init() == 1;
    }

    public static void Shutdown() => JPH_Shutdown();

    public static void SetAssertFailureHandler(AssertFailedImpl impl) => JPH_SetAssertFailureHandler(Marshal.GetFunctionPointerForDelegate(impl));
}
