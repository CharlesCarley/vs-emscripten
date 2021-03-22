cmake_minimum_required(VERSION 3.0)
project(Main1)

add_executable(Main1 main.cpp)
set_target_properties(Main1 PROPERTIES SUFFIX .wasm)
