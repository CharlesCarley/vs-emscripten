
set(CMAKE_EXE_LINKER_FLAGS "-s USE_SDL=2 -s FULL_ES3=1")
add_executable(SDL Main.cpp)
set_target_properties(SDL PROPERTIES SUFFIX .html)
set_target_properties(SDL PROPERTIES VS_CONFIGURATION_TYPE HTMLApplication)
set_target_properties(SDL PROPERTIES VS_GLOBAL_EchoCommandLines true)
set_target_properties(SDL PROPERTIES VS_GLOBAL_DebugTask false)
