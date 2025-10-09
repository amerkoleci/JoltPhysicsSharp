// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct MassProperties : IEquatable<MassProperties>
{
    /// <summary>
    /// Mass of the shape (kg)
    /// </summary>
	public float Mass = 0.0f;

    /// <summary>
    /// Inertia tensor of the shape (kg m^2)
    /// </summary>
    public Matrix4x4 Inertia = default;

    public MassProperties()
    {

    }

    /// <summary>
    /// Determines whether the specified <see cref="Int4"/> is equal to this instance.
    /// </summary>
    /// <param name="other">The <see cref="Int4"/> to compare with this instance.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(MassProperties other)
    {
        return Mass == other.Mass && Inertia.Equals(other.Inertia);
    }

    public static bool operator ==(MassProperties left, MassProperties right) => left.Equals(right);
    public static bool operator !=(MassProperties left, MassProperties right) => !left.Equals(right);

    /// <inheritdoc/>
    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is MassProperties handle && Equals(handle);

    /// <inheritdoc/>
    public override readonly int GetHashCode() => HashCode.Combine(Mass, Inertia);

    /// <summary>
    /// Set the mass and inertia of a box with edge size inBoxSize and density inDensity
    /// </summary>
    /// <param name="boxSize"></param>
    /// <param name="density"></param>
    public void SetMassAndInertiaOfSolidBox(in Vector3 boxSize, float density)
    {
        // Calculate mass
        Mass = boxSize.X * boxSize.Y * boxSize.Z * density;

        // Calculate inertia
        Vector3 size_sq = boxSize * boxSize;
        Vector3 scale = (new Vector3(size_sq.Y, size_sq.X, size_sq.X) + new Vector3(size_sq.Z, size_sq.Z, size_sq.Y)) * (Mass / 12.0f);
        Inertia = Matrix4x4.CreateScale(scale);
    }

    /// <summary>
    /// Set the mass and scale the inertia tensor to match the mass
    /// </summary>
    /// <param name="mass"></param>
    public void ScaleToMass(float mass)
    {
        if (Mass > 0.0f)
        {
            // Calculate how much we have to scale the inertia tensor
            float mass_scale = mass / Mass;

            // Update mass
            Mass = mass;

            // Update inertia tensor
            for (int i = 0; i < 3; ++i)
            {
                //Inertia
                Inertia.SetColumn(i, Inertia.GetColumn(i) * mass_scale);
            }
        }
        else
        {
            // Just set the mass
            Mass = mass;
        }
    }

    public unsafe void DecomposePrincipalMomentsOfInertia(out Matrix4x4 rotation, out Vector3 diagonal)
    {
        Mat4 joltMatrix;
        JPH_MassProperties_DecomposePrincipalMomentsOfInertia(this, &joltMatrix, out diagonal);
        rotation = joltMatrix.FromJolt();
    }

    public static Vector3 GetEquivalentSolidBoxSize(float mass, in Vector3 inertiaDiagonal)
    {
        JPH_MassProperties_GetEquivalentSolidBoxSize(mass, in inertiaDiagonal, out Vector3 result);
        return result;
    }

    public static void GetEquivalentSolidBoxSize(float mass, in Vector3 inertiaDiagonal, out Vector3 result)
    {
        JPH_MassProperties_GetEquivalentSolidBoxSize(mass, in inertiaDiagonal, out result);
    }
}
