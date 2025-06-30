// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

#pragma warning disable CA2255

using System.Runtime.CompilerServices;

namespace JoltPhysicsSharp;

public static class ModuleInit
{
    [ModuleInitializer]
    public static void Register()
    {
        HandleDictionary.RegisterFactory((handle) => new Wheel(handle, false));
        HandleDictionary.RegisterFactory((handle) => new WheelTV(handle, false));
        HandleDictionary.RegisterFactory((handle) => new WheelWV(handle, false));

        HandleDictionary.RegisterFactory((handle) => new WheeledVehicleController(handle, false));
        HandleDictionary.RegisterFactory((handle) => new MotorcycleController(handle, false));
        HandleDictionary.RegisterFactory((handle) => new TrackedVehicleController(handle, false));
    }
}

#pragma warning restore CA2255
