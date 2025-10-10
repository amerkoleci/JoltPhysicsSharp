// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class HeightFieldShapeSettings : ConvexShapeSettings
{
    /// <summary>
    /// Create a height field shape of sampleCount * sampleCount vertices.
    /// The height field is a surface defined by: offset + scale * (x, samples[y * sampleCount + x], y).
	/// where x and y are integers in the range x and y e [0, sampleCount - 1].
    /// </summary>
    /// <param name="samples">sampleCount^2 vertices.</param>
    /// <param name="offset">The shape offset.</param>
    /// <param name="scale">Shape scale.</param>
    /// <param name="sampleCount">sampleCount / blockSize must be minimally 2 and a power of 2 is the most efficient in terms of performance and storage.</param>
    public unsafe HeightFieldShapeSettings(float* samples, in Vector3 offset, in Vector3 scale, uint sampleCount)
        : base(JPH_HeightFieldShapeSettings_Create(samples, offset, scale, sampleCount))
    {
    }

    /// <summary>
    /// Create a height field shape of sampleCount * sampleCount vertices.
    /// The height field is a surface defined by: offset + scale * (x, samples[y * sampleCount + x], y).
	/// where x and y are integers in the range x and y e [0, sampleCount - 1].
    /// </summary>
    /// <param name="samples">sampleCount^2 vertices.</param>
    /// <param name="offset">The shape offset.</param>
    /// <param name="scale">Shape scale.</param>
    /// <param name="sampleCount">sampleCount / blockSize must be minimally 2 and a power of 2 is the most efficient in terms of performance and storage.</param>
    public unsafe HeightFieldShapeSettings(Span<float> samples, in Vector3 offset, in Vector3 scale, uint sampleCount)
    {
        fixed (float* samplesPtr = samples)
        {
            Handle = JPH_HeightFieldShapeSettings_Create(samplesPtr, offset, scale, sampleCount);
        }
    }

    public Vector3 Offset
    {
        get
        {
            JPH_HeightFieldShapeSettings_GetOffset(Handle, out Vector3 result);
            return result;
        }
        set => JPH_HeightFieldShapeSettings_SetOffset(Handle, in value);
    }

    public Vector3 Scale
    {
        get
        {
            JPH_HeightFieldShapeSettings_GetScale(Handle, out Vector3 result);
            return result;
        }
        set => JPH_HeightFieldShapeSettings_SetScale(Handle, in value);
    }

    public uint SampleCount
    {
        get => JPH_HeightFieldShapeSettings_GetSampleCount(Handle);
        set => JPH_HeightFieldShapeSettings_SetSampleCount(Handle, value);
    }

    public float MinHeightValue
    {
        get => JPH_HeightFieldShapeSettings_GetMinHeightValue(Handle);
        set => JPH_HeightFieldShapeSettings_SetMinHeightValue(Handle, value);
    }

    public float MaxHeightValue
    {
        get => JPH_HeightFieldShapeSettings_GetMaxHeightValue(Handle);
        set => JPH_HeightFieldShapeSettings_SetMaxHeightValue(Handle, value);
    }

    public uint BlockSize
    {
        get => JPH_HeightFieldShapeSettings_GetBlockSize(Handle);
        set => JPH_HeightFieldShapeSettings_SetBlockSize(Handle, value);
    }

    public uint BitsPerSample
            {
        get => JPH_HeightFieldShapeSettings_GetBitsPerSample(Handle);
        set => JPH_HeightFieldShapeSettings_SetBitsPerSample(Handle, value);
    }

    public float ActiveEdgeCosThresholdAngle
    {
        get => JPH_HeightFieldShapeSettings_GetActiveEdgeCosThresholdAngle(Handle);
        set => JPH_HeightFieldShapeSettings_SetActiveEdgeCosThresholdAngle(Handle, value);
    }

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

    public PhysicsMaterial? GetMaterial(uint x, uint y)
    {
        return PhysicsMaterial.GetObject(JPH_HeightFieldShape_GetMaterial(Handle, x, y));
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
