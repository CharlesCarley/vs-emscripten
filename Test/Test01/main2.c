#include "main.h"
#include <stdio.h>

int main(int argc, char **argv) 
{
    printf("Main2");
    fflush(stdout);
    fn1(); fn2(); fn3();
    return 0;
}

