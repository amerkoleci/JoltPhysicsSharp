# Define target name
set (TARGET_NAME joltc)

set(SOURCE_FILES
    joltc.h
    joltc.cpp
    joltc_assert.cpp
)

add_library(${TARGET_NAME} ${SOURCE_FILES})
target_link_libraries(${TARGET_NAME} PUBLIC Jolt)

if(BUILD_SHARED_LIBS)
    set_property(TARGET ${TARGET_NAME} PROPERTY POSITION_INDEPENDENT_CODE ON)

    target_compile_definitions(${TARGET_NAME} PRIVATE JPH_SHARED_LIBRARY_BUILD=1)
	set_property(TARGET ${TARGET_NAME} PROPERTY POSITION_INDEPENDENT_CODE ON)
endif()

if(ENABLE_SPECTRE_MITIGATION
    AND (MSVC_VERSION GREATER_EQUAL 1913)
    AND (NOT WINDOWS_STORE)
    AND (NOT (CMAKE_CXX_COMPILER_ID MATCHES "Clang")))
    message(STATUS "Building Spectre-mitigated libraries.")
    target_compile_options(${TARGET_NAME} PRIVATE "/Qspectre")
endif()
