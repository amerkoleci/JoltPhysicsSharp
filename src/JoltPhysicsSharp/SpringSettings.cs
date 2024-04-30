// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

namespace JoltPhysicsSharp;

public record struct SpringSettings
{
    public SpringSettings(SpringMode mode, float frequencyOrStiffness, float damping)
    {
        Mode = mode;
        FrequencyOrStiffness = frequencyOrStiffness;
        Damping = damping;
    }

    public SpringMode Mode;
    public float FrequencyOrStiffness;
    public float Damping;
}
