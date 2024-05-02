include(FetchContent)
FetchContent_Declare(
        JoltPhysics
        GIT_REPOSITORY "https://github.com/jrouwe/JoltPhysics"
        GIT_TAG "6b7681d920bb9b8649e4daed67f63849396daa9e"
		SOURCE_SUBDIR "Build"
)
FetchContent_MakeAvailable(JoltPhysics)

if (XCODE)
	# Ensure that we enable SSE4.2 for the x86_64 build, XCode builds multiple architectures
	set_property(TARGET Jolt PROPERTY XCODE_ATTRIBUTE_OTHER_CPLUSPLUSFLAGS[arch=x86_64] "$(inherited) -msse4.2 -mpopcnt")
endif()

# Define target name
if (DOUBLE_PRECISION)
    set (TARGET_NAME joltc_double)
else()
    set (TARGET_NAME joltc)
endif ()

set(SOURCE_FILES
    joltc.h
    joltc.cpp
    joltc_assert.cpp
	joltc.c
)

add_library(${TARGET_NAME} ${SOURCE_FILES})
target_include_directories(${TARGET_NAME} PUBLIC
	$<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}>
	${JoltPhysics_SOURCE_DIR}/..
)
target_link_libraries(${TARGET_NAME} PUBLIC Jolt)

set_target_properties(${TARGET_NAME} PROPERTIES
    CXX_STANDARD 17
)

if (MSVC)
    target_compile_options(${TARGET_NAME} PRIVATE /Wall /WX)
else()
    target_compile_options(${TARGET_NAME} PRIVATE -Wall -Werror)
endif()

if(BUILD_SHARED_LIBS)
	set_property(TARGET Jolt PROPERTY POSITION_INDEPENDENT_CODE ON)
    set_property(TARGET ${TARGET_NAME} PROPERTY POSITION_INDEPENDENT_CODE ON)

    target_compile_definitions(${TARGET_NAME} PRIVATE JPH_SHARED_LIBRARY_BUILD=1)
	set_property(TARGET ${TARGET_NAME} PROPERTY POSITION_INDEPENDENT_CODE ON)
endif()

install(TARGETS ${TARGET_NAME}
    EXPORT ${TARGET_NAME}
    ARCHIVE DESTINATION ${CMAKE_INSTALL_LIBDIR}
    LIBRARY DESTINATION ${CMAKE_INSTALL_LIBDIR}
    RUNTIME DESTINATION ${CMAKE_INSTALL_BINDIR}
)
