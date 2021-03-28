include(VSEmscripten)
enable_vs_emscripten_sdl(2)
add_executable(SDL Main.cpp)
enable_vs_emscripten_html_app(SDL)
