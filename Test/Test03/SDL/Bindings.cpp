#include "Bindings.h"
#include <SDL2/SDL_render.h>

AppInfo* AppBridge::Main = nullptr;

void AppBridge::Initialize(AppInfo* info)
{
    //
    Main = info;
}


void AppBridge::setBackgroundColor(char r, char g, char b)
{
    if (Main != nullptr)
    {
        Main->_background[0] = r;
        Main->_background[1] = g;
        Main->_background[2] = b;
    }
}
