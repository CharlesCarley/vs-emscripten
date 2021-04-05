#include(VSEmscripten)
set(TargetName SDL)

#enable_vs_emscripten_sdl(2)
#emscripten_WebGLVersion(1 2)
#emscripten_FullOpenGles3()


add_executable(${TargetName} Main.cpp)

#set_target_properties(SDL PROPERTIES VS_GLOBAL_VerboseOutput VerboseOutputMode)
#enable_vs_emscripten_html_app(SDL)

set_target_properties(${TargetName} PROPERTIES SUFFIX .html)
set_target_properties(${TargetName} PROPERTIES VS_CONFIGURATION_TYPE HTMLApplication)
set_target_properties(${TargetName} PROPERTIES VS_GLOBAL_VerboseOutput VerboseOutputMode)
set_target_properties(${TargetName} PROPERTIES VS_GLOBAL_EmSDLVersion 2)
set_target_properties(${TargetName} PROPERTIES VS_GLOBAL_EmUseFullOpenGles2 true)
set_target_properties(${TargetName} PROPERTIES VS_GLOBAL_EmUseFullOpenGles3 true)
set_target_properties(${TargetName} PROPERTIES VS_GLOBAL_EmMinWebGLVersion 1)
set_target_properties(${TargetName} PROPERTIES VS_GLOBAL_EmMaxWebGLVersion 44)
