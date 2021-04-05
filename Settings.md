# Settings.js

The following options have been converted from [settings.js](https://github.com/emscripten-core/emscripten/blob/main/src/settings.js).

All options can be passed through the AdditionalOptions property in VisualStudio. To pass the options with CMake, use CMAKE_EXE_LINKER_FLAGS for the settings that pertain to link time settings and CMAKE_CXX_FLAGS for compile time settings.

| Property Name      | Setting Name           |
|--------------------|------------------------|
| EmSdlVersion       | -s USE_SDL=n           |
| EmUseFullOpenGles2 | -s FULL_ES2=1          |
| EmUseFullOpenGles3 | -s FULL_ES3=1          |
| EmMinWebGlVersion  | -s MIN_WEBGL_VERSION=n |
| EmMaxWebGlVersion  | -s MAX_WEBGL_VERSION=n |
| EmPreloadFile      | --preload-file src@dst |
| EmEmbeddedFile     | --embed-file src@dst   |
