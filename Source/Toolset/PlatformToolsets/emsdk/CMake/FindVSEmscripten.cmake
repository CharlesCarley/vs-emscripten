# -----------------------------------------------------------------------------
#   Copyright (c) Charles Carley.
#
#   This software is provided 'as-is', without any express or implied
# warranty. In no event will the authors be held liable for any damages
# arising from the use of this software.
#
#   Permission is granted to anyone to use this software for any purpose,
# including commercial applications, and to alter it and redistribute it
# freely, subject to the following restrictions:
#
# 1. The origin of this software must not be misrepresented; you must not
#    claim that you wrote the original software. If you use this software
#    in a product, an acknowledgment in the product documentation would be
#    appreciated but is not required.
# 2. Altered source versions must be plainly marked as such, and must not be
#    misrepresented as being the original software.
# 3. This notice may not be removed or altered from any source distribution.
# ------------------------------------------------------------------------------
set(VS_DIR "$ENV{VS2019INSTALLDIR}")
find_path(VS_EMSCRIPTEN_PATH  NAMES VSEmscripten.cmake 
    PATHS  "${VS_DIR}/MSBuild/Microsoft/VC/v160/Platforms/Emscripten/PlatformToolsets/emsdk/CMake/")

if (VS_EMSCRIPTEN_PATH)

    string(COMPARE EQUAL "${CMAKE_SYSTEM_NAME}"           "Emscripten" _SystemNameEmscripten)
    string(COMPARE EQUAL "${CMAKE_VS_PLATFORM_TOOLSET}"   "emsdk"      _SystemToolsetEmscripten)

    if(_SystemNameEmscripten OR _SystemToolsetEmscripten)
        set(USING_EMSCRIPTEN TRUE)
        include(VSEmscripten)
    else()
        set(USING_EMSCRIPTEN FALSE)
    endif()

    unset(_SystemNameEmscripten)
    unset(_SystemToolsetEmscripten)
else()

endif()
