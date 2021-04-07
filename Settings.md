# Settings.js

The following options have been converted from [settings.js](https://github.com/emscripten-core/emscripten/blob/main/src/settings.js).

All options can be passed through the AdditionalOptions property in VisualStudio. To pass the options with CMake, use CMAKE_EXE_LINKER_FLAGS for the settings that pertain to link time settings and CMAKE_CXX_FLAGS for compile time settings.

| Property Name       | Switch Value              | Applies To    |
|---------------------|---------------------------|---------------|
| EmVerbose           | -s VERBOSE=1              | EmLink/EmCxx  |
| EmSdlVersion        | -s USE_SDL=n              | EmLink        |
| EmUseFullOpenGles2  | -s FULL_ES2=1             | EmLink        |
| EmUseFullOpenGles3  | -s FULL_ES3=1             | EmLink        |
| EmMinWebGlVersion   | -s MIN_WEBGL_VERSION=n    | EmLink        |
| EmMaxWebGlVersion   | -s MAX_WEBGL_VERSION=n    | EmLink        |
| EmPreloadFile       | --preload-file src@dst    | EmLink        |
| EmEmbeddedFile      | --embed-file src@dst      | EmLink        |
| EmTestStackOverflow | -s STACK_OVERFLOW_CHECK=1 | EmLink        |
| EmRuntimeLogging    | -s RUNTIME_LOGGING=1      | EmLink        |
| EmAllowMemoryGrowth | -s ALLOW_MEMORY_GROWTH=1  | EmLink        |
| EmInitialMemory     | -s INITIAL_MEMORY=(n > 0) | EmLink        |
| EmUseUBSan          | -fsanitize=undefined      | EmLink /EmCxx |
| EmUseASan           | -fsanitize=address        | EmLink /EmCxx |
