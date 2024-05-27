# Define target name
set (TARGET_NAME HelloWorldNative)

add_executable(${TARGET_NAME} main.cpp)

 if (WIN32)
    set_target_properties(${TARGET_NAME} PROPERTIES WIN32_EXECUTABLE TRUE)
endif ()

if (DOUBLE_PRECISION)
	target_link_libraries(${TARGET_NAME} joltc_double)
else()
	target_link_libraries(${TARGET_NAME} joltc)
endif ()

set_property(TARGET ${TARGET_NAME} PROPERTY FOLDER "Samples")

install(TARGETS ${TARGET_NAME}
    EXPORT ${TARGET_NAME}
    RUNTIME DESTINATION ${CMAKE_INSTALL_BINDIR}
)
