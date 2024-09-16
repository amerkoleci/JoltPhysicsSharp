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
        JPH_HeightFieldShapeSettings_DetermineMinAndMaxSample(Handle, out minValue, out maxValue, out quantizationScale);
    }

    public uint CalculateBitsPerSampleForError(float maxError) => JPH_HeightFieldShapeSettings_CalculateBitsPerSampleForError(Handle, maxError);
}

public sealed class HeightFieldShape : Shape
{
    public HeightFieldShape(in HeightFieldShapeSettings settings)
        : base(JPH_HeightFieldShapeSettings_CreateShape(settings.Handle))
    {
    }

    public uint SampleCount => JPH_HeightFieldShape_GetSampleCount(Handle);
    public uint BlockSize => JPH_HeightFieldShape_GetBlockSize(Handle);

    public float MinHeightValue => JPH_HeightFieldShape_GetMinHeightValue(Handle);
    public float MaxHeightValue => JPH_HeightFieldShape_GetMaxHeightValue(Handle);

    public /*JPH_PhysicsMaterial**/nint GetMaterial(uint x, uint y)
    {
        return JPH_HeightFieldShape_GetMaterial(Handle, x, y);
    }

    public Vector3 GetPosition(uint x, uint y)
    {
        JPH_HeightFieldShape_GetPosition(Handle, x, y, out Vector3 result);
        return result;
    }

    public void GetPosition(uint x, uint y, out Vector3 result)
    {
        JPH_HeightFieldShape_GetPosition(Handle, x, y, out result);
    }

    public bool IsNoCollision(uint x, uint y)
    {
        return JPH_HeightFieldShape_IsNoCollision(Handle, x, y);
    }

    public bool ProjectOntoSurface(in Vector3 localPosition, out Vector3 surfacePosition, out SubShapeID subShapeID)
    {
        return JPH_HeightFieldShape_ProjectOntoSurface(Handle, in localPosition, out surfacePosition, out subShapeID);
    }
    
}
