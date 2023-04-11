#include "ExternLog.hpp"
//#include <iostream>

void ExternLog(const char* message)
{
    if (ExternLogFunc)
        ExternLogFunc(message);
    //else std::cout << message << std::endl;
}

void InitLogger(void(_cdecl* Log)(const char* message))
{
    if (!Log)
    {
        //std::cout << "InitLogger func pointer should not be null" << std::endl;
        return;
    }
    ExternLogFunc = Log;
    ExternLog("Cpp Message: Log has initialized");
}
