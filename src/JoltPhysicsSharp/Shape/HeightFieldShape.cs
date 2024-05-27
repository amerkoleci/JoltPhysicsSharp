// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class HeightFieldShapeSettings : ConvexShapeSettings
{
    public unsafe HeightFieldShapeSettings(float* samples, in Vector3 offset, in Vector3 scale, int sampleCount)
      : base(JPH_HeightFieldShapeSettings_Create(samples, offset, scale, sampleCount))
    {
    }

    public HeightFieldShapeSettings(float[] samples, in Vector3 offset, in Vector3 scale)
        : this(samples.AsSpan(), offset, scale)
    {
    }

    public unsafe HeightFieldShapeSettings(ReadOnlySpan<float> samples, in Vector3 offset, in Vector3 scale)
    {
        fixed (float* samplesPtr = samples)
        {
            Handle = JPH_HeightFieldShapeSettings_Create(samplesPtr, offset, scale, samples.Length);
        }
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="HeightFieldShapeSettings" /> class.
    /// </summary>
    ~HeightFieldShapeSettings() => Dispose(disposing: false);

    public override Shape Create() => new HeightFieldShape(this);

    public void Sanitize() => JPH_MeshShapeSettings_Sanitize(Handle);

    public void DetermineMinAndMaxSample(out float minValue, out float maxValue, out float quantizationScale)
    {
        JPH_MeshShapeSettings_DetermineMinAndMaxSample(Handle, out minValue, out maxValue, out quantizationScale);
    }

    public uint CalculateBitsPerSampleForError(float maxError)
    {
        return JPH_MeshShapeSettings_CalculateBitsPerSampleForError(Handle, maxError);
    }
}

public sealed class HeightFieldShape : Shape
{
    public HeightFieldShape(in HeightFieldShapeSettings settings)
        : base(JPH_HeightFieldShapeSettings_CreateShape(settings.Handle))
    {
    }
}
