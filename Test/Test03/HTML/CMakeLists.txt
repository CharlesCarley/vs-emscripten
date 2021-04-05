include(VSEmscripten)

add_executable(HTML Main.cpp)
enable_emscripten_html_executable(HTML)
