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

macro(emscripten_WebGLVersion WebGLMin WebGLMax)

	set(CMAKE_EXE_LINKER_FLAGS "${CMAKE_EXE_LINKER_FLAGS} -s MIN_WEBGL_VERSION=${WebGLMin} -s MAX_WEBGL_VERSION=${WebGLMax}")

endmacro()

macro(emscripten_FullOpenGles3)

	set(CMAKE_EXE_LINKER_FLAGS "${CMAKE_EXE_LINKER_FLAGS} -s FULL_ES3=1")

endmacro()


macro(enable_vs_emscripten_sdl EmSdlVersion)

	set(CMAKE_EXE_LINKER_FLAGS "${CMAKE_EXE_LINKER_FLAGS} -s USE_SDL=${EmSdlVersion}")

endmacro()


macro(enable_vs_emscripten_html_app TargetName)

	# Swap <TargetExt>
	set_target_properties(${TargetName} PROPERTIES SUFFIX .html)

	# Swap <ConfigurationType>
	set_target_properties(${TargetName} PROPERTIES VS_CONFIGURATION_TYPE HTMLApplication)

endmacro()

