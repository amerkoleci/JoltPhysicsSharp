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
    set_property(TARGET Jolt PROPERTY POSITION_INDEPENDENT_CODE ON)

    target_compile_definitions(${TARGET_NAME} PRIVATE JPH_SHARED_LIBRARY_BUILD=1)
	set_property(TARGET ${TARGET_NAME} PROPERTY POSITION_INDEPENDENT_CODE ON)
endif()
