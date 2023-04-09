//used for logging message from native to managed side
#pragma once
#include <iostream>

static void(_cdecl* ExternLogFunc)(const char* message);

//use this in source code to send message to extern
extern "C" void _declspec(dllexport) ExternLog(const char* message)
{
    if (ExternLogFunc)
        ExternLogFunc(message);
    else
        std::cout << message << std::endl;
}

extern "C" void _declspec(dllexport) InitLogger(void(_cdecl* Log)(const char* message))
{
    if (!Log)
    {
        std::cout << "InitLogger func pointer should not be null" << std::endl;
        return;
    }
    ExternLogFunc = Log;
    ExternLog("Cpp Message: Log has initialized");
}
