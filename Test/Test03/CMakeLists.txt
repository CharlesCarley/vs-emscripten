cmake_minimum_required(VERSION 3.0)
project(Main1)


set(CMAKE_MODULE_PATH ${Main1_SOURCE_DIR}/../../Source/Toolset/PlatformToolsets/emsdk/CMake)

subdirs(Lib Exe HTML SDL)

