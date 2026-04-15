// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class SoftBodySharedSettings : NativeObject
{
    public SoftBodySharedSettings()
        : base(JPH_SoftBodySharedSettings_Create())
    {
    }
    internal SoftBodySharedSettings(nint handle, bool owns = true)
        : base(handle, owns)
    {
    }

    public uint VertexCount => JPH_SoftBodySharedSettings_GetVertexCount(Handle);
    public int FaceCount => (int)JPH_SoftBodySharedSettings_GetFaceCount(Handle);

    protected override void DisposeNative()
    {
        JPH_SoftBodySharedSettings_Destroy(Handle);
    }

    public void AddVertex(in Vertex vertex)
    {
        JPH_SoftBodySharedSettings_AddVertex(Handle, in vertex);
    }
    public unsafe void AddVertex(Span<Vertex> vertices)
    {
        fixed (Vertex* pVertices = vertices)
            JPH_SoftBodySharedSettings_AddVertices(Handle, pVertices, (uint)vertices.Length);
    }

    public bool RemoveVertex(uint index)
    {
        return JPH_SoftBodySharedSettings_RemoveVertex(Handle, index);
    }

    public bool GetVertex(uint index, out Vertex vertex)
    {
        return JPH_SoftBodySharedSettings_GetVertex(Handle, index, out vertex);
    }

    public void AddFace(in Face face)
    {
        JPH_SoftBodySharedSettings_AddFace(Handle, in face);
    }

    public unsafe void AddFaces(Span<Face> faces)
    {
        fixed (Face* pFaces = faces)
            JPH_SoftBodySharedSettings_AddFaces(Handle, pFaces, (uint)faces.Length);
    }

    public bool RemoveFace(uint index)
    {
        return JPH_SoftBodySharedSettings_RemoveFace(Handle, index);
    }

    public bool GetFace(uint index, out Face face)
    {
        return JPH_SoftBodySharedSettings_GetFace(Handle, index, out face);
    }

    public void CreateConstraints(float compliance, SoftBodyBendType bendType = SoftBodyBendType.Distance/*, float angleTolerance = DegreesToRadians(8.0f)*/)
    {
        JPH_SoftBodySharedSettings_CreateConstraints(Handle, compliance, bendType);
    }

    public void Optimize()
    {
        JPH_SoftBodySharedSettings_Optimize(Handle);
    }

    internal static SoftBodySharedSettings? GetObject(nint handle)
    {
        return GetOrAddObject(handle, h => new SoftBodySharedSettings(h, false));
    }

    public record struct Vertex
    {
        public Vector3 Position = Vector3.Zero;
        public Vector3 Velocity = Vector3.Zero;
        public float InvMass = 1.0f;

        public Vertex(in Vector3 position)
        {
            Position = position;
            Velocity = Vector3.Zero;
            InvMass = 1.0f;
        }

        public Vertex(in Vector3 position, in Vector3 velocity)
        {
            Position = position;
            Velocity = velocity;
            InvMass = 1.0f;
        }

        public Vertex(in Vector3 position, in Vector3 velocity, float invMass)
        {
            Position = position;
            Velocity = velocity;
            InvMass = invMass;
        }
    }

    public record struct Face
    {
        public uint Vertex1;
        public uint Vertex2;
        public uint Vertex3;
        public uint MaterialIndex;

        public Face(in uint vertex1, in uint vertex2, in uint vertex3, uint materialIndex = 0)
        {
            Vertex1 = vertex1;
            Vertex2 = vertex2;
            Vertex3 = vertex3;
            MaterialIndex = materialIndex;
        }
    }
}
