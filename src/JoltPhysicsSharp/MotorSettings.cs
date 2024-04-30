// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics;

namespace JoltPhysicsSharp;

public record struct MotorSettings
{
    public SpringSettings SpringSettings;
    public float MinForceLimit = float.MinValue;
    public float MaxForceLimit = float.MaxValue;
    public float MinTorqueLimit = float.MinValue;
    public float MaxTorqueLimit = float.MaxValue;

    public MotorSettings()
    {
        SpringSettings = new SpringSettings(SpringMode.FrequencyAndDamping, 2.0f, 1.0f);
    }

    public MotorSettings(float frequency, float damping)
    {
        SpringSettings = new SpringSettings(SpringMode.FrequencyAndDamping, frequency, damping);
        Debug.Assert(IsValid);
    }

    public MotorSettings(float frequency, float damping, float forceLimit, float torqueLimit)
    {
        SpringSettings = new SpringSettings(SpringMode.FrequencyAndDamping, frequency, damping);
        MinForceLimit = -forceLimit;
        MaxForceLimit = forceLimit;
        MinTorqueLimit = -torqueLimit;
        MaxTorqueLimit = torqueLimit;
        Debug.Assert(IsValid);
    }

    /// <summary>
    /// Check if settings are valid
    /// </summary>
    public readonly bool IsValid
    {
        get
        {
            return SpringSettings.FrequencyOrStiffness >= 0.0f && SpringSettings.Damping >= 0.0f && MinForceLimit <= MaxForceLimit && MinTorqueLimit <= MaxTorqueLimit;
        }
    }

    /// <summary>
    /// Set asymmetric force limits
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
	public void SetForceLimits(float min, float max)
    {
        Debug.Assert(min <= max);

        MinForceLimit = min;
        MaxForceLimit = max;
    }

    /// <summary>
    /// Set asymmetric torque limits
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public void SetTorqueLimits(float min, float max)
    {
        Debug.Assert(min <= max);

        MinTorqueLimit = min;
        MaxTorqueLimit = max;
    }

    /// <summary>
    /// Set symmetric force limits
    /// </summary>
    /// <param name="limit"></param>
    public void SetForceLimit(float limit)
    {
        MinForceLimit = -limit;
        MaxForceLimit = limit;
    }

    /// <summary>
    /// Set symmetric torque limits
    /// </summary>
    /// <param name="limit"></param>
    public void SetTorqueLimit(float limit)
    {
        MinTorqueLimit = -limit;
        MaxTorqueLimit = limit;
    }
}
