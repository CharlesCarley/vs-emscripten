
add_executable(HTML Main.cpp)

set_target_properties(HTML PROPERTIES SUFFIX .html)
set_target_properties(HTML PROPERTIES VS_CONFIGURATION_TYPE HTMLApplication)
set_target_properties(HTML PROPERTIES VS_GLOBAL_VerboseOutput VerboseOutputMode)
