include_directories(../Lib)


add_executable(WavmExeTest main.cpp)

target_link_libraries(
	WavmExeTest
	MockAPI
)

set_target_properties(WavmExeTest PROPERTIES SUFFIX .wasm)


set_target_properties(WavmExeTest PROPERTIES VS_GLOBAL_VerboseOutput ${Verbose})
set_target_properties(WavmExeTest PROPERTIES VS_GLOBAL_EchoCommandLines ${EchoTaskCL})
