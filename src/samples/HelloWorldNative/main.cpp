// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

#include "joltc.h"
#include <stdbool.h>
#include <stdlib.h> // malloc, free
#include <string.h> // memset

#if defined(_WIN32)
#include <Windows.h>
int WINAPI wWinMain(_In_ HINSTANCE hInstance, _In_opt_ HINSTANCE hPrevInstance, _In_ LPWSTR lpCmdLine, _In_ int nCmdShow)
#else
int main()
#endif
{
	if(!JPH_Init(0))
		return 1;

	JPH_PhysicsSystemSettings settings = {};
	JPH_PhysicsSystem* system = JPH_PhysicsSystem_Create(&settings);

	JPH_Shutdown();
	return 0;
}
