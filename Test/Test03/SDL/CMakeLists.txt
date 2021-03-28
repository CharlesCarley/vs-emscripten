include(VSEmscripten)

enable_vs_emscripten_sdl(2)
emscripten_WebGLVersion(1 2)
emscripten_FullOpenGles3()


add_executable(SDL Main.cpp)

set_target_properties(SDL PROPERTIES VS_GLOBAL_VerboseOutput VerboseOutputMode)
enable_vs_emscripten_html_app(SDL)
