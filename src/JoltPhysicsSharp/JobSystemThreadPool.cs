// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public readonly struct JobSystemThreadPool : IEquatable<JobSystemThreadPool>, IDisposable
{
    public JobSystemThreadPool(int maxJobs, int maxBarriers, int inNumThreads = -1)
        : this(JPH_JobSystemThreadPool_Create((uint)maxJobs, (uint)maxBarriers, inNumThreads))
    {
    }

    public void Dispose()
    {
        JPH_JobSystemThreadPool_Destroy(Handle);
    }

    public JobSystemThreadPool(IntPtr handle) { Handle = handle; }
    public IntPtr Handle { get; }
    public bool IsNull => Handle == IntPtr.Zero;
    public static JobSystemThreadPool Null => new(IntPtr.Zero);
    public static implicit operator JobSystemThreadPool(IntPtr handle) => new(handle);
    public static bool operator ==(JobSystemThreadPool left, JobSystemThreadPool right) => left.Handle == right.Handle;
    public static bool operator !=(JobSystemThreadPool left, JobSystemThreadPool right) => left.Handle != right.Handle;
    public static bool operator ==(JobSystemThreadPool left, IntPtr right) => left.Handle == right;
    public static bool operator !=(JobSystemThreadPool left, IntPtr right) => left.Handle != right;
    public bool Equals(JobSystemThreadPool other) => Handle == other.Handle;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is JobSystemThreadPool handle && Equals(handle);

    /// <inheritdoc/>
    public override int GetHashCode() => Handle.GetHashCode();
}
