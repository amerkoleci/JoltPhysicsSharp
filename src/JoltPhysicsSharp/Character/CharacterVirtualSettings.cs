// Copyright � Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Numerics;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public sealed class CharacterVirtualSettings : CharacterBaseSettings
{
    public CharacterVirtualSettings()
        : base(JPH_CharacterVirtualSettings_Create())
    {
    }

    // TODO add setters for CharacterVirtualSettings soon
}

