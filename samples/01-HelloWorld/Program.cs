// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Diagnostics;
using System.Numerics;
using JoltPhysicsSharp;
using Alimer.SampleFramework;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace HelloWorld;

public static class Program
{
    class HelloWorldApp : Application
    {
        const int NumberOfBoxes = 12;

        public HelloWorldApp()
            : base("01 - Hello World")
        {
            _ = CreateFloor(100, Layers.NonMoving);

            VehicleSettings settings = new();
            VehicleConstraint constraint = AddVehicle(in settings);
            Body body = constraint.VehicleBody;
            WheeledVehicleController controller = constraint.GetController<WheeledVehicleController>();
            foreach(var wheel in constraint.GetWheels())
            {
            }

            // add NumberOfBoxes cubes
            for (int i = 0; i < NumberOfBoxes; i++)
            {
                _ = CreateBox(
                    new Vector3(0.5f),
                    new Vector3(0, i * 2 + 0.5f, 0),
                    Quaternion.Identity,
                    MotionType.Dynamic,
                    Layers.Moving
                    );
            }
        }
    }

    public static unsafe void Main()
    {
        using HelloWorldApp app = new();
        app.Run();
    }
}
