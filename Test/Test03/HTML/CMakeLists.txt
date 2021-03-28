include(VSEmscripten)

add_executable(HTML Main.cpp)
enable_vs_emscripten_html_app(HTML)
