set(VS_DIR "$ENV{VS2019INSTALLDIR}")

find_path(VS_EMSCRIPTEN_PATH 
    NAMES VSEmscripten.cmake 
    PATHS  
    "${VS_DIR}/MSBuild/Microsoft/VC/v160/Platforms/Emscripten/PlatformToolsets/emsdk/CMake/")

if (VS_EMSCRIPTEN_PATH)
    set(CMAKE_MODULE_PATH "${CMAKE_MODULE_PATH};${VS_EMSCRIPTEN_PATH}")
    include(VSEmscripten)

    string(COMPARE EQUAL "${CMAKE_SYSTEM_NAME}"           "Emscripten" _SystemNameEmscripten)
    string(COMPARE EQUAL "${CMAKE_VS_PLATFORM_TOOLSET}"   "emsdk"      _SystemToolsetEmscripten)


    if(_SystemNameEmscripten OR _SystemToolsetEmscripten)
        set(USING_EMSCRIPTEN TRUE)
    else()
        set(USING_EMSCRIPTEN FALSE)
    endif()

    unset(_SystemNameEmscripten)
    unset(_SystemToolsetEmscripten)

    endif()
