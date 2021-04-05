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




# emscripten_verbose() 
#
#   ==> <VerboseOutput>VerboseOutputMode</VerboseOutput>
#
macro(emscripten_verbose TargetName)
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -s VERBOSE=1")
	set_target_properties(${TargetName} PROPERTIES VS_GLOBAL_VerboseOutput VerboseOutputMode)
endmacro()


# emscripten_web_gl_version(1 2) 
#
#   ==> <AdditionalOptions>$(AdditionalOptions) -s MIN_WEBGL_VERSION=1 -s MAX_WEBGL_VERSION=2"</AdditionalOptions>
#
macro(emscripten_web_gl_version WebGLMin WebGLMax)
	set(CMAKE_EXE_LINKER_FLAGS "${CMAKE_EXE_LINKER_FLAGS} -s MIN_WEBGL_VERSION=${WebGLMin} -s MAX_WEBGL_VERSION=${WebGLMax}")
endmacro()

# emscripten_full_es2() 
#
#   ==> <AdditionalOptions>$(AdditionalOptions) -s FULL_ES2=1</AdditionalOptions>
#
macro(emscripten_full_es2)
	set(CMAKE_EXE_LINKER_FLAGS "${CMAKE_EXE_LINKER_FLAGS} -s FULL_ES2=1")
endmacro()


# emscripten_full_es3() 
#
#   ==> <AdditionalOptions>$(AdditionalOptions) -s FULL_ES3=1</AdditionalOptions>
#
macro(emscripten_full_es3)
	set(CMAKE_EXE_LINKER_FLAGS "${CMAKE_EXE_LINKER_FLAGS} -s FULL_ES3=1")
endmacro()


# emscripten_sdl_version(2) 
#
#   ==> <AdditionalOptions>$(AdditionalOptions) -s USE_SDL=2</AdditionalOptions>
#
macro(emscripten_sdl_version EmSdlVersion)

	set(CMAKE_EXE_LINKER_FLAGS "${CMAKE_EXE_LINKER_FLAGS} -s USE_SDL=${EmSdlVersion}")

endmacro()

# add_executable(${TargetName}) 
#
# enable_emscripten_html_app((${TargetName}) 
#
#   ==> <TargetExt>.html</TargetExt>
#   ==> <ConfigurationType>HTMLApplication</ConfigurationType>
#   ==> Internally -s WASM=2 
#
macro(enable_emscripten_html_executable TargetName)
	set_target_properties(${TargetName} PROPERTIES SUFFIX .html)
	set_target_properties(${TargetName} PROPERTIES VS_CONFIGURATION_TYPE HTMLApplication)
endmacro()


# add_executable(${TargetName}) 
#
# enable_emscripten_html_app((${TargetName}) 
#
#   ==> <TargetExt>.wasm</TargetExt>
#   ==> Internally -s WASM=1 
#
macro(enable_emscripten_wasm_executable TargetName)
	set_target_properties(${TargetName} PROPERTIES SUFFIX .wasm)
endmacro()




macro(emscripten_copy_wasm_target TARNAME DESTDIR)

    add_custom_command(TARGET ${TARNAME} 
                       POST_BUILD
                       COMMAND ${CMAKE_COMMAND} -E copy $<TARGET_FILE:${TARNAME}> 
                       "${DESTDIR}/$<TARGET_FILE_NAME:${TARNAME}>"
                       )

    add_custom_command(TARGET ${TARNAME} 
                       POST_BUILD
                       COMMAND ${CMAKE_COMMAND} -E copy "$<TARGET_FILE_DIR:${TARNAME}>/${TARNAME}.wasm" 
                       "${DESTDIR}/${TARNAME}.wasm"
                       )

    add_custom_command(TARGET ${TARNAME} 
                       POST_BUILD
                       COMMAND ${CMAKE_COMMAND} -E copy "$<TARGET_FILE_DIR:${TARNAME}>/${TARNAME}.js" 
                       "${DESTDIR}/${TARNAME}.js"
                       )
    set_target_properties(
        ${TARNAME} 
        PROPERTIES 
        VS_DEBUGGER_WORKING_DIRECTORY  
        "${DESTDIR}"
    )

endmacro(emscripten_copy_wasm_target)




macro(emscripten_copy_wasm_target_wasm_js TARNAME DESTDIR)

    add_custom_command(TARGET ${TARNAME} 
                       POST_BUILD
                       COMMAND ${CMAKE_COMMAND} -E copy "$<TARGET_FILE_DIR:${TARNAME}>/${TARNAME}.wasm" 
                       "${DESTDIR}/${TARNAME}.wasm"
                       )

    add_custom_command(TARGET ${TARNAME} 
                       POST_BUILD
                       COMMAND ${CMAKE_COMMAND} -E copy "$<TARGET_FILE_DIR:${TARNAME}>/${TARNAME}.js" 
                       "${DESTDIR}/${TARNAME}.js"
                       )
    set_target_properties(
        ${TARNAME} 
        PROPERTIES 
        VS_DEBUGGER_WORKING_DIRECTORY  
        "${DESTDIR}"
    )

endmacro(emscripten_copy_wasm_target_wasm_js)