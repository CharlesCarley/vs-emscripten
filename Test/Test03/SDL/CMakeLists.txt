
add_executable(SDL Main.cpp)
set_target_properties(SDL PROPERTIES SUFFIX .html)
set_target_properties(SDL PROPERTIES VS_CONFIGURATION_TYPE HTMLApplication)
set_target_properties(SDL PROPERTIES VS_GLOBAL_EmSDLVersion 2)
set_target_properties(HTML PROPERTIES VS_GLOBAL_VerboseOutput VerboseOutputMode)
