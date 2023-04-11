//used for logging message from native to managed side
#pragma once

static void(_cdecl* ExternLogFunc)(const char* message);

//use this in source code to send message to extern
void ExternLog(const char* message);

extern "C" void _declspec(dllexport) InitLogger(void(_cdecl * Log)(const char* message));
