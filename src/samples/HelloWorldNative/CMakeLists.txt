# Define target name
set (TARGET_NAME HelloWorldNative)

set(SOURCE_FILES main.cpp)

if (ANDROID)
    add_library(${TARGET_NAME} SHARED ${SOURCE_FILES})
    target_link_libraries(${TARGET_NAME} log android native_app_glue)
else ()
    add_executable(${TARGET_NAME} ${SOURCE_FILES})
endif ()

 if (WIN32)
    set_target_properties(${TARGET_NAME} PROPERTIES WIN32_EXECUTABLE TRUE)
endif ()

target_link_libraries(${TARGET_NAME} joltc)

set_property(TARGET ${TARGET_NAME} PROPERTY FOLDER "Samples")

install(TARGETS ${TARGET_NAME}
    EXPORT ${TARGET_NAME}
    RUNTIME DESTINATION ${CMAKE_INSTALL_BINDIR}
)
