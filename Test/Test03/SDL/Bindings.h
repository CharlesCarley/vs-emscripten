#pragma once
#include <cstdio>

struct SDL_Window;
struct SDL_Renderer;

struct AppInfo
{
    SDL_Window*   window;
    SDL_Renderer* renderer;
    unsigned char _background[4]{};
};



class AppBridge
{
private:
    static AppInfo* Main;

public:
    static void Initialize(AppInfo* info);
    

public:

    AppBridge()
    {
    }


    void setBackgroundColor(char r, char g, char b);
    
};
