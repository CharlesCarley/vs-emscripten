include_directories(../Lib ${CMAKE_CURRENT_BINARY_DIR})

add_executable(SDL 
    Main.cpp 
    Main.html 
    Bindings.h 
    Bindings.idl
    Bindings.cpp
    WrappedBindings.cpp
    )


target_link_libraries(SDL MockAPI)


set_target_properties(SDL PROPERTIES SUFFIX .html)
set_target_properties(SDL PROPERTIES VS_CONFIGURATION_TYPE HTMLApplication)


set_target_properties(SDL PROPERTIES VS_GLOBAL_EmSdlVersion 2)


set_target_properties(SDL PROPERTIES VS_GLOBAL_VerboseOutput ${Verbose})
set_target_properties(SDL PROPERTIES VS_GLOBAL_EchoCommandLines ${EchoTaskCL})
set_target_properties(SDL PROPERTIES VS_GLOBAL_EchoCommandLines ${EchoTaskCL})


emscripten_copy_wasm_target_wasm_js(SDL ${CMAKE_BINARY_DIR}/Bin)


set(CMAKE_EXE_LINKER_FLAGS "${CMAKE_EXE_LINKER_FLAGS} --post-js ${CMAKE_CURRENT_BINARY_DIR}/BindingsGlue.js")

add_custom_command(TARGET SDL 
                    POST_BUILD
                    COMMAND ${CMAKE_COMMAND} -E copy "${CMAKE_CURRENT_SOURCE_DIR}/Main.html"  "${CMAKE_BINARY_DIR}/Bin/Main.html")
