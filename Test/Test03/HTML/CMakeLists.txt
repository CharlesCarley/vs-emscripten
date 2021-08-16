include(VSEmscripten)

add_executable(HTML Main.cpp)
enable_emscripten_html_executable(HTML)


set_target_properties(HTML PROPERTIES VS_GLOBAL_VerboseOutput    ${Verbose})
set_target_properties(HTML PROPERTIES VS_GLOBAL_EchoCommandLines ${EchoTaskCL})
