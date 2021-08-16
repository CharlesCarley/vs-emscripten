cmake_minimum_required(VERSION 3.0)
project(TestCMakeSupport)

set(CMAKE_MODULE_PATH 
	${CMAKE_SOURCE_DIR}/../../Source/Toolset/PlatformToolsets/emsdk/CMake
)

set(Verbose NoVerboseOutputMode)
set(EchoTaskCL true)

# for testing cross linking against static libraries.
add_subdirectory(Lib)

# For testing a WAVM executable
add_subdirectory(Exe)

# For testing non SDL HTML embedding.
add_subdirectory(HTML)

# For testing SDL 
add_subdirectory(SDL)


