#include <SDL2/SDL.h>
#include <emscripten/emscripten.h>

struct AppInfo
{
    SDL_Window*   window;
    SDL_Renderer* renderer;
};

void render(void* arg)
{
    AppInfo* inf = (AppInfo*)arg;
    if (inf)
    {
        SDL_SetRenderDrawColor(inf->renderer, 0x80, 0x80, 0x80, 0xFF);
        SDL_RenderClear(inf->renderer);


        SDL_Rect r1 = {0, 0, 20, 20}, r2 = {20, 20, 20, 20}, r3 = {40, 40, 20, 20};
        SDL_SetRenderDrawColor(inf->renderer, 0xFF, 0x00, 0x00, 0xFF);
        SDL_RenderFillRect(inf->renderer, &r1);

        SDL_SetRenderDrawColor(inf->renderer, 0x00, 0xFF, 0x00, 0xFF);
        SDL_RenderFillRect(inf->renderer, &r2);

        SDL_SetRenderDrawColor(inf->renderer, 0x00, 0x00, 0xFF, 0xFF);
        SDL_RenderFillRect(inf->renderer, &r3);

        SDL_RenderPresent(inf->renderer);
    }
}


int main()
{
    SDL_Init(SDL_INIT_VIDEO);

    AppInfo inf = {};

    SDL_CreateWindowAndRenderer(100, 100, SDL_RENDERER_ACCELERATED, &inf.window, &inf.renderer);

    if (inf.renderer)
        emscripten_set_main_loop_arg(render, &inf, 0, 1);
    SDL_Quit();
    return 0;
}
