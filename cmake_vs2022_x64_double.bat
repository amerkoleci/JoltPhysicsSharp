@echo off

cmake -B ".\build\win64_double" -S "." -G "Visual Studio 17 2022" -A x64 -DCMAKE_INSTALL_PREFIX="Sdk" -DDOUBLE_PRECISION=ON
pause