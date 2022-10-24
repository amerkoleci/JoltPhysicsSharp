@echo off

cmake -B ".\build\win_arm64" -S "." -G "Visual Studio 17 2022" -A ARM64 -DCMAKE_INSTALL_PREFIX:String="Sdk"
pause