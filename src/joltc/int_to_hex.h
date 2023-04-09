#pragma once
#include <iomanip>
#include <sstream>
#include <string>

template<typename T>
std::string int_to_hex_str(T i)
{
    std::stringstream str{};
    str << "0x" << std::setfill('0') << std::setw(sizeof(T) * 2) << std::hex << i;
    return str.str();
}
