cmake_minimum_required(VERSION 3.0)
project(VsGlobals)

set(TargetName VsGlobals)
add_executable(${TargetName} Main.cpp)

set_target_properties(${TargetName} PROPERTIES SUFFIX .wasm)
set_target_properties(${TargetName} PROPERTIES VS_CONFIGURATION_TYPE HTMLApplication)
set_target_properties(${TargetName} PROPERTIES VS_GLOBAL_EmEchoCommandLines true)


## Test Values ##
set_target_properties(${TargetName} PROPERTIES VS_GLOBAL_EmAssertions 2)
set_target_properties(${TargetName} PROPERTIES VS_GLOBAL_EmTestStackOverflow Binaryen)
set_target_properties(${TargetName} PROPERTIES VS_GLOBAL_EmAllowMemoryGrowth false)
set_target_properties(${TargetName} PROPERTIES VS_GLOBAL_EmRuntimeLogging true)
set_target_properties(${TargetName} PROPERTIES VS_GLOBAL_EmUseUBSan true)
set_target_properties(${TargetName} PROPERTIES VS_GLOBAL_EmUseASan true)
set_target_properties(${TargetName} PROPERTIES VS_GLOBAL_EmVerbose true)



