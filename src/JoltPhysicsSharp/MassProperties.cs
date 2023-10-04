// Copyright © Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct MassProperties : IEquatable<MassProperties>
{
    /// Mass of the shape (kg)
	public float Mass = 0.0f;

    /// Inertia tensor of the shape (kg m^2)
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
        return Mass == other.Mass
            && Inertia.Equals(other.Inertia)
            ;
    }

    public static bool operator ==(MassProperties left, MassProperties right) => left.Equals(right);
    public static bool operator !=(MassProperties left, MassProperties right) => !left.Equals(right);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is MassProperties handle && Equals(handle);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Mass, Inertia);

    public void SetMassAndInertiaOfSolidBox(in Vector3 inBoxSize, float inDensity)
    {
        // Calculate mass
        Mass = inBoxSize.X * inBoxSize.Y * inBoxSize.Z * inDensity;

        // Calculate inertia
        Vector3 size_sq = inBoxSize * inBoxSize;
        Vector3 scale = (new Vector3(size_sq.Y, size_sq.X, size_sq.X) + new Vector3(size_sq.Z, size_sq.Z, size_sq.Y)) * (Mass / 12.0f);
        Inertia = Matrix4x4.CreateScale(scale);
    }

    public void ScaleToMass(float mass)
    {
        if (Mass > 0.0f)
        {
            // Calculate how much we have to scale the inertia tensor
            float mass_scale = Mass / Mass;

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
}
